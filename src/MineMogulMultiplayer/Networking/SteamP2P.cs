using System;
using System.Collections.Generic;
using BepInEx.Logging;
using MineMogulMultiplayer.Models;
using MineMogulMultiplayer.Serialization;
using Steamworks;
using Steamworks.Data;

namespace MineMogulMultiplayer.Networking
{
    /// <summary>
    /// Steam P2P networking using Steam Networking Sockets (relay).
    /// Supports both host and client roles through a single class.
    /// All callbacks are queued and dispatched on the main thread via Poll().
    /// </summary>
    public class SteamP2P
    {
        internal ManualLogSource _log;
        private bool _isHost;
        private HostSocketManager _socketManager;   // host only
        private ClientConnectionManager _connectionManager; // client only

        // Connected clients (host tracks these)
        private readonly Dictionary<uint, Connection> _clients = new Dictionary<uint, Connection>();
        private uint _nextClientId = 1;
        private readonly Dictionary<Connection, uint> _connectionToId = new Dictionary<Connection, uint>();

        // ── Thread-safe event queues ──
        // Steam callbacks fire on worker threads; we queue and dispatch on main thread in Poll().
        private readonly object _queueLock = new object();

        private enum CallbackType { ClientConnected, ClientDisconnected, Message, ConnectedToHost, DisconnectedFromHost }

        private struct QueuedCallback
        {
            public CallbackType Type;
            public uint ClientId;
            public MessageType MsgType;
            public byte[] Payload;
        }

        private readonly List<QueuedCallback> _callbackQueue = new List<QueuedCallback>();

        // ── Events (raised on the main thread during Poll) ──

        public event Action<uint> OnClientConnected;
        public event Action<uint> OnClientDisconnected;
        public event Action<uint, MessageType, byte[]> OnMessageReceived;

        // Client-side events
        public event Action OnConnectedToHost;
        public event Action OnDisconnectedFromHost;

        public bool IsRunning { get; private set; }

        public void StartHost(ManualLogSource log)
        {
            _log = log;
            _isHost = true;

            // Must init relay access before creating relay sockets
            SteamNetworkingUtils.InitRelayNetworkAccess();

            _socketManager = SteamNetworkingSockets.CreateRelaySocket<HostSocketManager>();
            if (_socketManager == null)
            {
                _log.LogError("[SteamP2P] Failed to create relay socket");
                return;
            }
            ((HostSocketManager)_socketManager).Parent = this;

            IsRunning = true;
            _log.LogInfo($"Steam P2P host started. SteamID: {SteamClient.SteamId}");
        }

        public void StartClient(ManualLogSource log, SteamId hostSteamId)
        {
            _log = log;
            _isHost = false;

            // Must init relay access before connecting via relay
            SteamNetworkingUtils.InitRelayNetworkAccess();

            _connectionManager = SteamNetworkingSockets.ConnectRelay<ClientConnectionManager>(hostSteamId);
            if (_connectionManager == null)
            {
                _log.LogError("[SteamP2P] Failed to connect relay to host");
                return;
            }
            ((ClientConnectionManager)_connectionManager).Parent = this;

            IsRunning = true;
            _log.LogInfo($"Connecting to host {hostSteamId}...");
        }

        public void Poll()
        {
            if (!IsRunning) return;

            if (_isHost && _socketManager != null)
                _socketManager.Receive();

            if (!_isHost && _connectionManager != null)
                _connectionManager.Receive();

            // Dispatch all queued callbacks on the main thread
            List<QueuedCallback> toProcess;
            lock (_queueLock)
            {
                if (_callbackQueue.Count == 0) return;
                toProcess = new List<QueuedCallback>(_callbackQueue);
                _callbackQueue.Clear();
            }

            foreach (var cb in toProcess)
            {
                try
                {
                    switch (cb.Type)
                    {
                        case CallbackType.ClientConnected:
                            OnClientConnected?.Invoke(cb.ClientId);
                            break;
                        case CallbackType.ClientDisconnected:
                            OnClientDisconnected?.Invoke(cb.ClientId);
                            break;
                        case CallbackType.Message:
                            OnMessageReceived?.Invoke(cb.ClientId, cb.MsgType, cb.Payload);
                            break;
                        case CallbackType.ConnectedToHost:
                            OnConnectedToHost?.Invoke();
                            break;
                        case CallbackType.DisconnectedFromHost:
                            OnDisconnectedFromHost?.Invoke();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log?.LogError($"[SteamP2P] Error dispatching {cb.Type}: {ex}");
                }
            }
        }

        public void Stop()
        {
            try
            {
                if (_isHost && _socketManager != null)
                {
                    foreach (var kv in _clients)
                    {
                        try { kv.Value.Close(); }
                        catch (Exception ex) { _log?.LogError($"[SteamP2P] Error closing client {kv.Key}: {ex.Message}"); }
                    }
                    _clients.Clear();
                    _connectionToId.Clear();
                    try { _socketManager.Close(); }
                    catch (Exception ex) { _log?.LogError($"[SteamP2P] Error closing socket: {ex.Message}"); }
                    _socketManager = null;
                }

                if (!_isHost && _connectionManager != null)
                {
                    try { _connectionManager.Close(); }
                    catch (Exception ex) { _log?.LogError($"[SteamP2P] Error closing connection: {ex.Message}"); }
                    _connectionManager = null;
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"[SteamP2P] Stop error: {ex}");
            }

            lock (_queueLock) { _callbackQueue.Clear(); }

            IsRunning = false;
            _log?.LogInfo("Steam P2P stopped");
        }

        // ── Send (host → specific client) ──

        public void SendToClient<T>(uint clientId, MessageType type, T payload)
        {
            if (!_isHost || !_clients.TryGetValue(clientId, out var conn)) return;
            try
            {
                var data = NetSerializer.Pack(type, payload);
                conn.SendMessage(data, SendType.Reliable);
            }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] SendToClient failed: {ex}"); }
        }

        // ── Send (host → all clients) ──

        public void SendToAll<T>(MessageType type, T payload)
        {
            if (!_isHost || _clients.Count == 0) return;
            byte[] data;
            try { data = NetSerializer.Pack(type, payload); }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] SendToAll pack failed: {ex}"); return; }

            foreach (var kv in _clients)
            {
                try { kv.Value.SendMessage(data, SendType.Reliable); }
                catch (Exception ex) { _log?.LogError($"[SteamP2P] SendToAll send to {kv.Key} failed: {ex}"); }
            }
        }

        /// <summary>Send unreliable (no retransmit) — ideal for frequent position updates that get superseded.</summary>
        public void SendToAllUnreliable<T>(MessageType type, T payload)
        {
            if (!_isHost || _clients.Count == 0) return;
            byte[] data;
            try { data = NetSerializer.Pack(type, payload); }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] SendToAllUnreliable pack failed: {ex}"); return; }

            foreach (var kv in _clients)
            {
                try { kv.Value.SendMessage(data, SendType.Unreliable); }
                catch (Exception) { /* drop silently — expected with unreliable */ }
            }
        }

        // ── Send (client → host) ──

        public void SendToHost<T>(MessageType type, T payload)
        {
            if (_isHost || _connectionManager == null) return;
            try
            {
                var data = NetSerializer.Pack(type, payload);
                _connectionManager.Connection.SendMessage(data, SendType.Reliable);
            }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] SendToHost failed: {ex}"); }
        }

        /// <summary>Send unreliable to host — ideal for frequent position updates.</summary>
        public void SendToHostUnreliable<T>(MessageType type, T payload)
        {
            if (_isHost || _connectionManager == null) return;
            try
            {
                var data = NetSerializer.Pack(type, payload);
                _connectionManager.Connection.SendMessage(data, SendType.Unreliable);
            }
            catch { /* drop silently */ }
        }

        // ── Internal callbacks from socket/connection managers ──
        // These fire on Steam's worker threads — enqueue for main-thread dispatch.

        internal void HandleClientConnected(Connection connection)
        {
            lock (_queueLock)
            {
                var id = _nextClientId++;
                _clients[id] = connection;
                _connectionToId[connection] = id;
                _log?.LogInfo($"Client {id} connected via Steam relay");
                _callbackQueue.Add(new QueuedCallback { Type = CallbackType.ClientConnected, ClientId = id });
            }
        }

        internal void HandleClientDisconnected(Connection connection)
        {
            lock (_queueLock)
            {
                if (_connectionToId.TryGetValue(connection, out var id))
                {
                    _connectionToId.Remove(connection);
                    _clients.Remove(id);
                    _log?.LogInfo($"Client {id} disconnected");
                    _callbackQueue.Add(new QueuedCallback { Type = CallbackType.ClientDisconnected, ClientId = id });
                }
            }
        }

        internal void HandleMessageFromClient(Connection connection, IntPtr data, int size)
        {
            try
            {
                uint clientId;
                lock (_queueLock)
                {
                    if (!_connectionToId.TryGetValue(connection, out clientId))
                    {
                        _log?.LogWarning($"[SteamP2P] Message from unknown connection {connection.Id} — dropping ({size} bytes)");
                        return;
                    }
                }
                var bytes = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(data, bytes, 0, size);
                var msg = NetSerializer.Unpack(bytes);
                if (msg == null)
                {
                    _log?.LogWarning($"[SteamP2P] Failed to unpack message from client {clientId} ({size} bytes)");
                    return;
                }
                lock (_queueLock)
                {
                    _callbackQueue.Add(new QueuedCallback
                    {
                        Type = CallbackType.Message,
                        ClientId = clientId,
                        MsgType = msg.Type,
                        Payload = msg.Payload
                    });
                }
            }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] HandleMessageFromClient error: {ex}"); }
        }

        internal void HandleConnectedToHost()
        {
            _log?.LogInfo("Connected to host via Steam relay");
            lock (_queueLock)
            {
                _callbackQueue.Add(new QueuedCallback { Type = CallbackType.ConnectedToHost });
            }
        }

        internal void HandleDisconnectedFromHost()
        {
            _log?.LogInfo("Disconnected from host");
            lock (_queueLock)
            {
                _callbackQueue.Add(new QueuedCallback { Type = CallbackType.DisconnectedFromHost });
            }
        }

        internal void HandleMessageFromHost(IntPtr data, int size)
        {
            try
            {
                var bytes = new byte[size];
                System.Runtime.InteropServices.Marshal.Copy(data, bytes, 0, size);
                var msg = NetSerializer.Unpack(bytes);
                if (msg == null)
                {
                    _log?.LogWarning($"[SteamP2P] Failed to unpack message from host ({size} bytes)");
                    return;
                }
                lock (_queueLock)
                {
                    _callbackQueue.Add(new QueuedCallback
                    {
                        Type = CallbackType.Message,
                        ClientId = 0,
                        MsgType = msg.Type,
                        Payload = msg.Payload
                    });
                }
            }
            catch (Exception ex) { _log?.LogError($"[SteamP2P] HandleMessageFromHost error: {ex}"); }
        }
    }

    // ── Host socket manager (accepts incoming relay connections) ──

    internal class HostSocketManager : SocketManager
    {
        internal SteamP2P Parent;

        public override void OnConnecting(Connection connection, ConnectionInfo info)
        {
            Parent?._log?.LogInfo($"[SteamP2P] Client connecting from {info.Identity}");
            base.OnConnecting(connection, info); // accepts the connection
        }

        public override void OnConnected(Connection connection, ConnectionInfo info)
        {
            // CRITICAL: base.OnConnected sets the connection's poll group,
            // which is required for SocketManager.Receive() → ReceiveMessagesOnPollGroup to work.
            base.OnConnected(connection, info);
            Parent?._log?.LogInfo($"[SteamP2P] Client connected: {info.Identity}");
            Parent.HandleClientConnected(connection);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo info)
        {
            Parent?._log?.LogInfo($"[SteamP2P] Client disconnected: {info.Identity} reason: {info.EndReason}");
            Parent.HandleClientDisconnected(connection);
            base.OnDisconnected(connection, info); // removes from poll group, closes connection
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            Parent.HandleMessageFromClient(connection, data, size);
        }
    }

    // ── Client connection manager (connects to host via relay) ──

    internal class ClientConnectionManager : ConnectionManager
    {
        internal SteamP2P Parent;

        public override void OnConnecting(ConnectionInfo info)
        {
            Parent?._log?.LogInfo($"[SteamP2P] Connecting to host... state: {info.State}");
            base.OnConnecting(info);
        }

        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            Parent?._log?.LogInfo($"[SteamP2P] Connected to host! state: {info.State}");
            Parent.HandleConnectedToHost();
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            Parent?._log?.LogInfo($"[SteamP2P] Disconnected from host. state: {info.State}, reason: {info.EndReason}");
            Parent.HandleDisconnectedFromHost();
            base.OnDisconnected(info);
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            Parent.HandleMessageFromHost(data, size);
        }
    }
}
