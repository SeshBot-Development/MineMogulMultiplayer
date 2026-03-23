using MessagePack;

namespace MineMogulMultiplayer.Models
{
    // ──────────────────────────────────────────────
    //  Mirrors the game's ResourceType enum for serialization
    // ──────────────────────────────────────────────

    public enum NetResourceType : byte
    {
        INVALID = 0,
        Iron, Coal, Gold, Slag, Diamond, Emerald, Copper,
        Broken, Ruby, Steel, Celestite, Quartz, Amethyst, Mystery
    }

    public enum NetPieceType : byte
    {
        Ore = 0, Crushed, Ingot, Plate, Rod, Threaded, Pipe
    }

    // ──────────────────────────────────────────────
    //  Player state
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class PlayerState
    {
        [Key(0)] public int PlayerId;
        [Key(1)] public string DisplayName;
        [Key(2)] public NetVector3 Position;
        [Key(3)] public NetQuaternion Rotation;
        [Key(4)] public float Money;
        [Key(5)] public int ResearchTickets;
        /// <summary>SavableObjectID name of the currently held tool, or null/empty if empty-handed.</summary>
        [Key(6)] public string EquippedTool;
        [Key(7)] public bool IsCrouching;
    }

    // ──────────────────────────────────────────────
    //  Building (machine/structure) state
    //  Matches BuildingObject from the game
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class BuildingState
    {
        /// <summary>Local-only — not valid across processes. Use Position+SavableObjectId for matching.</summary>
        [Key(0)] public int LocalInstanceId;

        /// <summary>SavableObjectID name (matches the game's BuildingObject.SavableObjectID).</summary>
        [Key(1)] public string SavableObjectId;

        [Key(2)] public NetVector3 Position;
        [Key(3)] public NetQuaternion Rotation;

        /// <summary>Custom save data JSON (the game's ICustomSaveDataProvider).</summary>
        [Key(4)] public string CustomSaveData;
    }

    /// <summary>Identifies a building across processes using position + type (buildings don't move).</summary>
    [MessagePackObject]
    public class BuildingRemovalInfo
    {
        [Key(0)] public NetVector3 Position;
        [Key(1)] public string SavableObjectId;
    }

    // ──────────────────────────────────────────────
    //  Ore piece state (for replicating OrePiece objects)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class OrePieceState
    {
        /// <summary>Host-assigned network ID — stable across processes.</summary>
        [Key(0)] public int NetworkId;
        [Key(1)] public NetResourceType ResourceType;
        [Key(2)] public NetPieceType PieceType;
        [Key(3)] public bool IsPolished;
        [Key(4)] public NetVector3 Position;
        [Key(5)] public NetQuaternion Rotation;
        [Key(6)] public NetVector3 Velocity;
        [Key(7)] public float SellValue;
    }

    // ──────────────────────────────────────────────
    //  Conveyor belt state
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ConveyorState
    {
        /// <summary>Local-only — not valid across processes. Use Position for matching.</summary>
        [Key(0)] public int LocalInstanceId;
        [Key(1)] public float Speed;
        [Key(2)] public bool Disabled;
        [Key(3)] public NetVector3 Position;
        [Key(4)] public NetQuaternion Rotation;
    }

    // ──────────────────────────────────────────────
    //  Breakable crate state (for position sync)
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class CrateState
    {
        [Key(0)] public int NetworkId;
        [Key(1)] public NetVector3 Position;
        [Key(2)] public NetQuaternion Rotation;
    }

    // ──────────────────────────────────────────────
    //  Global / economy / world state
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class WorldState
    {
        [Key(0)] public float Money;
        [Key(1)] public int ResearchTickets;
        [Key(2)] public string[] CompletedResearchIds;
        [Key(3)] public string[] CompletedQuestIds;
        [Key(4)] public ActiveQuestData[] ActiveQuests;
        [Key(5)] public ShopPurchaseData[] ShopPurchases;
        [Key(6)] public ContractData ActiveContract;
        [Key(7)] public ContractData[] InactiveContracts;
    }

    // ──────────────────────────────────────────────
    //  Active quest progress tracking
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ActiveQuestData
    {
        [Key(0)] public string QuestId;
        [Key(1)] public ResourceRequirementProgress[] ResourceProgress;
        [Key(2)] public TriggeredRequirementProgress[] TriggeredProgress;
    }

    [MessagePackObject]
    public class ResourceRequirementProgress
    {
        [Key(0)] public string ResourceType;
        [Key(1)] public string PieceType;
        [Key(2)] public bool RequirePolished;
        [Key(3)] public int CurrentAmount;
    }

    [MessagePackObject]
    public class TriggeredRequirementProgress
    {
        [Key(0)] public string Type;
        [Key(1)] public int CurrentAmount;
    }

    // ──────────────────────────────────────────────
    //  Shop purchase tracking
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ShopPurchaseData
    {
        [Key(0)] public string SavableObjectId;
        [Key(1)] public int Amount;
    }

    // ──────────────────────────────────────────────
    //  Contract state tracking
    // ──────────────────────────────────────────────

    [MessagePackObject]
    public class ContractData
    {
        [Key(0)] public string Name;
        [Key(1)] public ResourceRequirementProgress[] Progress;
        [Key(2)] public float RewardMoney;
    }
}
