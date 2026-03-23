public interface ISaveLoadableWorldEvent
{
	SavableWorldEventType GetWorldEventType();

	int GetWorldEventID();

	bool GetHasHappened();

	void LoadFromSave(string customDataJson);

	string GetCustomSaveData();
}
