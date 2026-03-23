using System;
using BepInEx.Logging;
using MessagePack;
using MineMogulMultiplayer.Models;

namespace MineMogulMultiplayer.Serialization
{
    /// <summary>
    /// Central serialization helper using MessagePack.
    /// All network messages are wrapped in NetMessage { Type, Payload }.
    /// </summary>
    public static class NetSerializer
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("NetSerializer");

        // ── Envelope ─────────────────────────────────

        public static byte[] Pack<T>(MessageType type, T payload)
        {
            var envelope = new NetMessage
            {
                Type = type,
                Payload = MessagePackSerializer.Serialize(payload)
            };
            return MessagePackSerializer.Serialize(envelope);
        }

        public static NetMessage Unpack(byte[] data)
        {
            try
            {
                return MessagePackSerializer.Deserialize<NetMessage>(data);
            }
            catch (Exception ex)
            {
                _log.LogError($"[NetSerializer] Failed to unpack message ({data?.Length ?? 0} bytes): {ex.Message}");
                return null;
            }
        }

        public static T UnpackPayload<T>(NetMessage msg)
        {
            try
            {
                return MessagePackSerializer.Deserialize<T>(msg.Payload);
            }
            catch (Exception ex)
            {
                _log.LogError($"[NetSerializer] Failed to unpack payload for {msg.Type}: {ex.Message}");
                return default;
            }
        }

        // ── Convenience helpers ──────────────────────

        public static byte[] Serialize<T>(T obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }

        public static T Deserialize<T>(byte[] data)
        {
            try
            {
                return MessagePackSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _log.LogError($"[NetSerializer] Failed to deserialize {typeof(T).Name} ({data?.Length ?? 0} bytes): {ex.Message}");
                return default;
            }
        }
    }
}
