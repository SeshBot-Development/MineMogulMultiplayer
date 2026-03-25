using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveManager : Singleton<AutoSaveManager>
{
	public float AutoSaveFrequency = 5f;

	public bool AutoSaveEnabled = true;

	private float _lastAutoSaveTime;

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<AutoSaveManager>.Instance != this))
		{
			ApplySavedSettings();
		}
	}

	public void ApplySavedSettings()
	{
		AutoSaveFrequency = PlayerPrefs.GetFloat("AutoSaveFrequency", 5f);
		AutoSaveEnabled = PlayerPrefs.GetInt("AutoSaveEnabled", 1) > 0;
	}

	private void Update()
	{
		if (AutoSaveEnabled && AutoSaveEnabled && Time.time - _lastAutoSaveTime >= AutoSaveFrequency * 60f)
		{
			_lastAutoSaveTime = Time.time;
			StartCoroutine(AutoSave());
		}
	}

	public IEnumerator AutoSave()
	{
		string text = SceneManager.GetActiveScene().name;
		if (text.ToLower() == "MainMenu")
		{
			Debug.Log("Trying to autosave on the main menu??? (canceling)");
			yield break;
		}
		if (text.ToLower() == "DemoCave")
		{
			Debug.Log("Trying to autosave on the Legacy Demo Map (canceling)");
			yield break;
		}
		Debug.Log("Autosaving '" + Singleton<SavingLoadingManager>.Instance.ActiveSaveFileName + "' on scene '" + text + "' ...");
		yield return new WaitForSeconds(1f);
		Singleton<UIManager>.Instance.ShowAutoSavingWarning();
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		yield return new WaitForSeconds(1f);
		Singleton<UIManager>.Instance.HideAutoSavingWarning();
	}
}
