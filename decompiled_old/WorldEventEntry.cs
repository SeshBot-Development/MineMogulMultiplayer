using System;

[Serializable]
public class WorldEventEntry
{
	public SavableWorldEventType SavableWorldEventType;

	public int WorldEventID;

	public string CustomDataJson;
}
