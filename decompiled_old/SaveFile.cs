using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveFile
{
	public int SaveVersion;

	public string GameVersion = "Unknown";

	public string SaveFileName = "Unnamed Save File";

	public string SaveTimestamp = "Unknown Time";

	public string LevelID = "DemoCave";

	public float Money;

	public int ResearchTickets;

	public double TotalPlayTimeSeconds;

	public bool IsDemoVersion;

	public Vector3 PlayerPosition = Vector3.zero;

	public Vector3 PlayerRotation = Vector3.zero;

	public List<SaveEntry> Entries = new List<SaveEntry>();

	public List<BuildingObjectEntry> BuildingObjects = new List<BuildingObjectEntry>();

	public List<Vector3> DestroyedStaticBreakablePositions = new List<Vector3>();

	public List<QuestID> CompletedQuestsIDs = new List<QuestID>();

	public List<ActiveQuestEntry> ActiveQuests = new List<ActiveQuestEntry>();

	public List<OrePieceEntry> OrePieces = new List<OrePieceEntry>();

	public List<WorldEventEntry> WorldEventEntries = new List<WorldEventEntry>();

	public ShopPurchases ShopPurchases = new ShopPurchases();

	public List<SavableObjectID> CompletedResearchItems = new List<SavableObjectID>();

	public bool HasShownOreLimitPopup;
}
