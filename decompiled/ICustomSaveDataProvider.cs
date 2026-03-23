public interface ICustomSaveDataProvider
{
	void LoadFromSave(string customDataJson);

	string GetCustomSaveData();
}
