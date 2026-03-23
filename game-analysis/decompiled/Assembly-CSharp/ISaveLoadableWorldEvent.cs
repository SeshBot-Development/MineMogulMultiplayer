using System;

// Token: 0x020000B6 RID: 182
public interface ISaveLoadableWorldEvent
{
	// Token: 0x060004F3 RID: 1267
	SavableWorldEventType GetWorldEventType();

	// Token: 0x060004F4 RID: 1268
	int GetWorldEventID();

	// Token: 0x060004F5 RID: 1269
	bool GetHasHappened();

	// Token: 0x060004F6 RID: 1270
	void LoadFromSave(string customDataJson);

	// Token: 0x060004F7 RID: 1271
	string GetCustomSaveData();
}
