using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// Token: 0x0200003E RID: 62
[DefaultExecutionOrder(-1000)]
public class DebugManager : global::Singleton<DebugManager>
{
	// Token: 0x14000002 RID: 2
	// (add) Token: 0x060001A6 RID: 422 RVA: 0x00008F48 File Offset: 0x00007148
	// (remove) Token: 0x060001A7 RID: 423 RVA: 0x00008F80 File Offset: 0x00007180
	public event Action ClearedAllPhysicsOrePieces;

	// Token: 0x060001A8 RID: 424 RVA: 0x00008FB5 File Offset: 0x000071B5
	protected override void Awake()
	{
		base.Awake();
		this.DevModeEnabled = false;
		this.UnlimitedBuilding = false;
		this.PlayerSpawnsWithItems = false;
		this.ShowDevTestShopItems = false;
	}

	// Token: 0x060001A9 RID: 425 RVA: 0x00008FD9 File Offset: 0x000071D9
	private void OnEnable()
	{
		Application.logMessageReceived += this.HandleLog;
	}

	// Token: 0x060001AA RID: 426 RVA: 0x00008FEC File Offset: 0x000071EC
	private void OnDisable()
	{
		Application.logMessageReceived -= this.HandleLog;
	}

	// Token: 0x060001AB RID: 427 RVA: 0x00008FFF File Offset: 0x000071FF
	private void HandleLog(string message, string stackTrace, LogType type)
	{
		if (this.DontShowErrorAgainThisSession)
		{
			return;
		}
		if (global::Singleton<UIManager>.Instance == null)
		{
			return;
		}
		if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
		{
			global::Singleton<UIManager>.Instance.PauseMenu.ShowErrorPopup(message, stackTrace);
		}
	}

	// Token: 0x060001AC RID: 428 RVA: 0x00009034 File Offset: 0x00007234
	private void Update()
	{
		if (global::Singleton<UIManager>.Instance != null && global::Singleton<UIManager>.Instance.IsInEditTextPopup())
		{
			return;
		}
		if (this.DevModeEnabled)
		{
			this.HandleTimeScaleAdjustment();
			if (Input.GetKeyDown(KeyCode.I))
			{
				global::Singleton<EconomyManager>.Instance.UnlockAllShopItems();
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				this.UnlimitedBuilding = !this.UnlimitedBuilding;
				Debug.Log("UnlimitedBuilding: " + this.UnlimitedBuilding.ToString());
			}
			if (Input.GetKeyDown(KeyCode.M))
			{
				global::Singleton<EconomyManager>.Instance.AddMoney(1000f);
			}
			if (Input.GetKeyDown(KeyCode.Z))
			{
				global::Singleton<QuestManager>.Instance.DebugCompleteNextQuest();
				return;
			}
		}
		else
		{
			this.CheckDevModeSecretMessage();
		}
	}

	// Token: 0x060001AD RID: 429 RVA: 0x000090E0 File Offset: 0x000072E0
	public void ClearAllPhysicsOrePieces(bool keepOrePiecesThatAreInBaskets = true)
	{
		HashSet<OrePiece> hashSet = new HashSet<OrePiece>();
		if (keepOrePiecesThatAreInBaskets)
		{
			foreach (BaseBasket baseBasket in Object.FindObjectsOfType<BaseBasket>())
			{
				hashSet.AddRange(baseBasket.GetOrePiecesInFilter());
			}
		}
		foreach (OrePiece orePiece in Object.FindObjectsOfType<OrePiece>())
		{
			if (!hashSet.Contains(orePiece))
			{
				orePiece.Delete();
			}
		}
		ConveyorBelt[] array3 = Object.FindObjectsOfType<ConveyorBelt>();
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].ClearNullObjectsOnBelt();
		}
		RollingMill[] array4 = Object.FindObjectsOfType<RollingMill>();
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].OnAllOreCleared();
		}
		Action clearedAllPhysicsOrePieces = this.ClearedAllPhysicsOrePieces;
		if (clearedAllPhysicsOrePieces == null)
		{
			return;
		}
		clearedAllPhysicsOrePieces();
	}

	// Token: 0x060001AE RID: 430 RVA: 0x00009198 File Offset: 0x00007398
	private void HandleTimeScaleAdjustment()
	{
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Time.timeScale = Mathf.Max(0.0625f, Time.timeScale * 0.5f);
			Debug.Log("Time Scale: " + Time.timeScale.ToString());
		}
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale *= 2f;
			Debug.Log("Time Scale: " + Time.timeScale.ToString());
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			Time.timeScale = 1f;
			Debug.Log("Reset Timescale");
		}
	}

	// Token: 0x060001AF RID: 431 RVA: 0x00009238 File Offset: 0x00007438
	private void CheckDevModeSecretMessage()
	{
		foreach (char c in Input.inputString)
		{
			if (Time.time - this._lastInputTime > this._resetDelay)
			{
				this._inputBuffer = "";
			}
			this._lastInputTime = Time.time;
			this._inputBuffer += c.ToString();
			if (this._inputBuffer.Length > this._secretCode.Length)
			{
				this._inputBuffer = this._inputBuffer.Substring(this._inputBuffer.Length - this._secretCode.Length);
			}
			if (this._inputBuffer.ToLower() == this._secretCode.ToLower())
			{
				this.DevModeEnabled = true;
				Debug.Log("Developer Mode Enabled!");
				global::Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.DevModeEnabledSound, Object.FindObjectOfType<PlayerController>().transform.position, 1f, 1f, true, false);
				ComputerShopUI computerShopUI = Object.FindObjectOfType<ComputerShopUI>(true);
				if (computerShopUI != null)
				{
					computerShopUI.SetupCategories();
				}
			}
		}
	}

	// Token: 0x0400018F RID: 399
	public bool DevModeEnabled;

	// Token: 0x04000190 RID: 400
	public bool UnlimitedBuilding;

	// Token: 0x04000191 RID: 401
	public bool PlayerSpawnsWithItems;

	// Token: 0x04000192 RID: 402
	public bool ShowDevTestShopItems;

	// Token: 0x04000193 RID: 403
	public SoundDefinition DevModeEnabledSound;

	// Token: 0x04000194 RID: 404
	public bool DontShowErrorAgainThisSession;

	// Token: 0x04000196 RID: 406
	private string _secretCode = "shaftmaster";

	// Token: 0x04000197 RID: 407
	private float _resetDelay = 3f;

	// Token: 0x04000198 RID: 408
	private string _inputBuffer = "";

	// Token: 0x04000199 RID: 409
	private float _lastInputTime;
}
