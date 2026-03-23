using System;

// Token: 0x02000054 RID: 84
public interface ICustomSaveDataProvider
{
	// Token: 0x0600022D RID: 557
	void LoadFromSave(string customDataJson);

	// Token: 0x0600022E RID: 558
	string GetCustomSaveData();
}
