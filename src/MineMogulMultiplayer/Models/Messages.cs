using MessagePack;

namespace MineMogulMultiplayer.Models
{
    // ──────────────────────────────────────────────
    //  Network message envelope
    // ──────────────────────────────────────────────

    public enum MessageType : byte
    {
        // Connection
        Handshake        = 0,
        HandshakeAck     = 1,
        Disconnect       = 2,

        // State sync
        FullSnapshot     = 10,
        DeltaUpdate      = 11,
        HashCheck        = 12,
        ResyncRequest    = 13,

        // Client → Host RPCs
        PlayerInput      = 20,
        PlaceBuilding    = 21,
        RemoveBuilding   = 22,
        InteractBuilding = 23,
        MineNode         = 24,
        ResearchItem     = 25,
        CrateDamage      = 26,

        // Host → Client events
        BuildingSpawned  = 30,
        BuildingRemoved  = 31,
        OreSpawned       = 32,
        OreRemoved       = 33,
        OreSpawnedBatch  = 34,
        OreRemovedBatch  = 35,
        OrePositionBatch = 36,
        InventorySync    = 37,
        CratePositionBatch = 38,
        ItemDropped      = 39,
        ChatMessage      = 40,
        ShopPurchaseNotify = 41,
        ClientInventoryReport = 42,
        SoundEvent = 43,

        // Lobby flow
        LobbyLaunch      = 50,
    }

    [MessagePackObject]
    public class NetMessage
    {
        [Key(0)] public MessageType Type;
        [Key(1)] public byte[] Payload;
    }

    // ──────────────────────────────────────────────
    //  Client → Host input
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class PlayerInputMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public NetVector3 Position;
        [Key(2)] public NetQuaternion Rotation;
        [Key(3)] public bool Sprinting;
        [Key(4)] public long ClientTick;
        /// <summary>SavableObjectID name of the currently held tool, or null if empty-handed.</summary>
        [Key(5)] public string EquippedTool;
        [Key(6)] public bool IsCrouching;
        /// <summary>SavableObjectID name of a physics-held world object (e.g. lantern), if any.</summary>
        [Key(7)] public string HeldObjectId;
        [Key(8)] public NetVector3 HeldObjectPosition;
        [Key(9)] public NetQuaternion HeldObjectRotation;
    }

    [MessagePackObject]
    public class PlaceBuildingMessage
    {
        [Key(0)] public int PlayerId;
        /// <summary>SavableObjectID of the building to place.</summary>
        [Key(1)] public string SavableObjectId;
        [Key(2)] public NetVector3 Position;
        [Key(3)] public NetQuaternion Rotation;
    }

    [MessagePackObject]
    public class RemoveBuildingMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public NetVector3 Position;
        [Key(2)] public string SavableObjectId;
    }

    [MessagePackObject]
    public class InteractBuildingMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public int BuildingInstanceId;
        /// <summary>Interaction name: "toggle", "toggleAutoMiner", "toggleHatch", "toggleBlocker", "editSign", "triggerDetonator", "buyDetonator".</summary>
        [Key(2)] public string Action;
        /// <summary>Building world position for cross-process matching.</summary>
        [Key(3)] public NetVector3 Position;
        /// <summary>Optional payload (e.g. sign text).</summary>
        [Key(4)] public string Data;
    }

    [MessagePackObject]
    public class MineNodeMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public NetVector3 NodePosition;
        [Key(2)] public float Damage;
        [Key(3)] public NetVector3 HitPosition;
    }

    [MessagePackObject]
    public class CrateDamageMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public NetVector3 CratePosition;
        [Key(2)] public float Damage;
        [Key(3)] public NetVector3 HitPosition;
    }

    [MessagePackObject]
    public class ResearchItemMessage
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public string ResearchItemId;
    }

    // ──────────────────────────────────────────────
    //  Handshake
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class HandshakeMessage
    {
        [Key(0)] public string PlayerName;
        [Key(1)] public string ModVersion;
        [Key(2)] public ulong SteamId;
    }

    [MessagePackObject]
    public class HandshakeAckMessage
    {
        [Key(0)] public int AssignedPlayerId;
        [Key(1)] public bool Accepted;
        [Key(2)] public string RejectionReason;
        [Key(3)] public string SceneName;
    }

    [MessagePackObject]
    public class ChatMessageData
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public string Text;
    }

    [MessagePackObject]
    public class LobbyLaunchMessage
    {
        [Key(0)] public string SceneName;
        [Key(1)] public string SaveFileName;
    }

    // ──────────────────────────────────────────────
    //  Ore position batch (host → client, for grabbed/moving ores)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class OrePositionUpdate
    {
        [Key(0)] public int NetworkId;
        [Key(1)] public NetVector3 Position;
        [Key(2)] public NetQuaternion Rotation;
    }

    [MessagePackObject]
    public class CratePositionUpdate
    {
        [Key(0)] public int NetworkId;
        [Key(1)] public NetVector3 Position;
        [Key(2)] public NetQuaternion Rotation;
    }

    // ──────────────────────────────────────────────
    //  Inventory sync (host → client, shared tool list)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class InventorySyncMessage
    {
        /// <summary>List of SavableObjectID names that should be in the player's inventory.</summary>
        [Key(0)] public string[] Tools;
    }

    // ──────────────────────────────────────────────
    //  Shop purchase notification (client → host)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ShopPurchaseNotifyMessage
    {
        /// <summary>SavableObjectID names of tools the client purchased.</summary>
        [Key(0)] public string[] PurchasedTools;
        /// <summary>Total money spent (so host can deduct).</summary>
        [Key(1)] public float TotalCost;
    }

    // ──────────────────────────────────────────────
    //  Item drop/throw notification (bidirectional)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ItemDroppedMessage
    {
        [Key(0)] public int PlayerId;
        /// <summary>"ore" or "crate"</summary>
        [Key(1)] public string ItemType;
        [Key(2)] public int NetworkId;
        [Key(3)] public NetVector3 Position;
        [Key(4)] public NetQuaternion Rotation;
        [Key(5)] public NetVector3 Velocity;
        [Key(6)] public NetVector3 AngularVelocity;
    }

    // ──────────────────────────────────────────────
    //  Client inventory report (client → host, periodic)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ClientInventoryReportMessage
    {
        [Key(0)] public ulong SteamId;
        [Key(1)] public string[] Tools;
    }

    // ──────────────────────────────────────────────
    //  Sound event (bidirectional, spatial audio sync)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class SoundEventMessage
    {
        [Key(0)] public int PlayerId;
        /// <summary>SoundDefinition name/identifier.</summary>
        [Key(1)] public string SoundName;
        [Key(2)] public NetVector3 Position;
    }
}
