namespace MineMogulMultiplayer.Core
{
    /// <summary>
    /// Tracks whether we are running as host, client, or offline (singleplayer).
    /// Globally accessible so patches can branch on role.
    /// </summary>
    public static class MultiplayerState
    {
        public enum Role
        {
            Offline,
            Host,
            Client
        }

        public static Role CurrentRole { get; set; } = Role.Offline;
        public static int LocalPlayerId { get; set; } = 0;

        public static bool IsHost => CurrentRole == Role.Host;
        public static bool IsClient => CurrentRole == Role.Client;
        public static bool IsOnline => CurrentRole != Role.Offline;
    }
}
