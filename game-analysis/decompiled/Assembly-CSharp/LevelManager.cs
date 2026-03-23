using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000B8 RID: 184
public class LevelManager : Singleton<LevelManager>
{
	// Token: 0x060004F8 RID: 1272 RVA: 0x0001A154 File Offset: 0x00018354
	public List<LevelInfo> GetLevelsToShowInMapSelect()
	{
		return this._allLevels.Where((LevelInfo level) => level.ShouldAppearInMapSelect).ToList<LevelInfo>();
	}

	// Token: 0x060004F9 RID: 1273 RVA: 0x0001A188 File Offset: 0x00018388
	public LevelInfo GetLevelByID(string levelID)
	{
		return this._allLevels.Find((LevelInfo level) => level.LevelID == levelID);
	}

	// Token: 0x060004FA RID: 1274 RVA: 0x0001A1BC File Offset: 0x000183BC
	public LevelInfo GetCurrentLevelInfo()
	{
		string currentSceneName = SceneManager.GetActiveScene().name;
		return this._allLevels.Find((LevelInfo level) => level.SceneName == currentSceneName);
	}

	// Token: 0x060004FB RID: 1275 RVA: 0x0001A1FC File Offset: 0x000183FC
	public string GetCurrentLevelID()
	{
		LevelInfo currentLevelInfo = this.GetCurrentLevelInfo();
		if (currentLevelInfo == null)
		{
			Debug.LogError("No LevelID Found for Current level (" + SceneManager.GetActiveScene().name + "), Save File will have a Null LevelID!");
		}
		if (currentLevelInfo == null)
		{
			return "Null";
		}
		return currentLevelInfo.LevelID;
	}

	// Token: 0x060004FC RID: 1276 RVA: 0x0001A244 File Offset: 0x00018444
	public string GetCurrentMapName()
	{
		LevelInfo currentLevelInfo = this.GetCurrentLevelInfo();
		if (currentLevelInfo != null)
		{
			return "Map: " + currentLevelInfo.DisplayName;
		}
		return "Scene: " + SceneManager.GetActiveScene().name;
	}

	// Token: 0x0400061E RID: 1566
	[SerializeField]
	private List<LevelInfo> _allLevels = new List<LevelInfo>();
}
