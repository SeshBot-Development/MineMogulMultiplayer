using System;

[Serializable]
public class SaveFileHeader
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

	public bool IsPlayableInDemoVersion()
	{
		if (IsDemoVersion)
		{
			return true;
		}
		if (SaveVersion < 5)
		{
			return true;
		}
		return false;
	}
}
