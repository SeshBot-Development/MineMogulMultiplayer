using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : Singleton<LevelManager>
{
	[SerializeField]
	private List<LevelInfo> _allLevels = new List<LevelInfo>();

	public List<LevelInfo> GetLevelsToShowInMapSelect()
	{
		return _allLevels.Where((LevelInfo level) => level.ShouldAppearInMapSelect).ToList();
	}

	public LevelInfo GetLevelByID(string levelID)
	{
		return _allLevels.Find((LevelInfo level) => level.LevelID == levelID);
	}

	public LevelInfo GetCurrentLevelInfo()
	{
		string currentSceneName = SceneManager.GetActiveScene().name;
		return _allLevels.Find((LevelInfo level) => level.SceneName == currentSceneName);
	}

	public string GetCurrentLevelID()
	{
		LevelInfo currentLevelInfo = GetCurrentLevelInfo();
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

	public string GetCurrentMapName()
	{
		LevelInfo currentLevelInfo = GetCurrentLevelInfo();
		if (currentLevelInfo != null)
		{
			return "Map: " + currentLevelInfo.DisplayName;
		}
		return "Scene: " + SceneManager.GetActiveScene().name;
	}
}
