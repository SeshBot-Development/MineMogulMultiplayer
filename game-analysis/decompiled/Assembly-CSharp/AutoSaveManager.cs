using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Token: 0x020000B0 RID: 176
public class AutoSaveManager : Singleton<AutoSaveManager>
{
	// Token: 0x060004DC RID: 1244 RVA: 0x00019FAF File Offset: 0x000181AF
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<AutoSaveManager>.Instance != this)
		{
			return;
		}
		this.ApplySavedSettings();
	}

	// Token: 0x060004DD RID: 1245 RVA: 0x00019FCB File Offset: 0x000181CB
	public void ApplySavedSettings()
	{
		this.AutoSaveFrequency = PlayerPrefs.GetFloat("AutoSaveFrequency", 5f);
		this.AutoSaveEnabled = PlayerPrefs.GetInt("AutoSaveEnabled", 1) > 0;
	}

	// Token: 0x060004DE RID: 1246 RVA: 0x00019FFC File Offset: 0x000181FC
	private void Update()
	{
		if (this.AutoSaveEnabled && this.AutoSaveEnabled && Time.time - this._lastAutoSaveTime >= this.AutoSaveFrequency * 60f)
		{
			this._lastAutoSaveTime = Time.time;
			base.StartCoroutine(this.AutoSave());
		}
	}

	// Token: 0x060004DF RID: 1247 RVA: 0x0001A04B File Offset: 0x0001824B
	public IEnumerator AutoSave()
	{
		string name = SceneManager.GetActiveScene().name;
		if (name.ToLower() == "MainMenu")
		{
			Debug.Log("Trying to autosave on the main menu??? (canceling)");
			yield break;
		}
		if (name.ToLower() == "DemoCave")
		{
			Debug.Log("Trying to autosave on the Legacy Demo Map (canceling)");
			yield break;
		}
		Debug.Log(string.Concat(new string[]
		{
			"Autosaving '",
			Singleton<SavingLoadingManager>.Instance.ActiveSaveFileName,
			"' on scene '",
			name,
			"' ..."
		}));
		yield return new WaitForSeconds(1f);
		Singleton<UIManager>.Instance.ShowAutoSavingWarning();
		Singleton<SavingLoadingManager>.Instance.SaveGameWithActiveSaveFileName();
		yield return new WaitForSeconds(1f);
		Singleton<UIManager>.Instance.HideAutoSavingWarning();
		yield break;
	}

	// Token: 0x0400057F RID: 1407
	public float AutoSaveFrequency = 5f;

	// Token: 0x04000580 RID: 1408
	public bool AutoSaveEnabled = true;

	// Token: 0x04000581 RID: 1409
	private float _lastAutoSaveTime;
}
