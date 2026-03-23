using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DebugManager : Singleton<DebugManager>
{
	public bool DevModeEnabled;

	public bool UnlimitedBuilding;

	public bool PlayerSpawnsWithItems;

	public bool ShowDevTestShopItems;

	public SoundDefinition DevModeEnabledSound;

	public bool DontShowErrorAgainThisSession;

	private string _secretCode = "shaftmaster";

	private float _resetDelay = 3f;

	private string _inputBuffer = "";

	private float _lastInputTime;

	public event Action ClearedAllPhysicsOrePieces;

	protected override void Awake()
	{
		base.Awake();
		DevModeEnabled = false;
		UnlimitedBuilding = false;
		PlayerSpawnsWithItems = false;
		ShowDevTestShopItems = false;
	}

	private void OnEnable()
	{
		Application.logMessageReceived += HandleLog;
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= HandleLog;
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		if (!DontShowErrorAgainThisSession && !(Singleton<UIManager>.Instance == null) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
		{
			Singleton<UIManager>.Instance.PauseMenu.ShowErrorPopup(message, stackTrace);
		}
	}

	private void Update()
	{
		if (Singleton<UIManager>.Instance != null && Singleton<UIManager>.Instance.IsInEditTextPopup())
		{
			return;
		}
		if (DevModeEnabled)
		{
			HandleTimeScaleAdjustment();
			if (Input.GetKeyDown(KeyCode.I))
			{
				Singleton<EconomyManager>.Instance.UnlockAllShopItems();
			}
			if (Input.GetKeyDown(KeyCode.U))
			{
				UnlimitedBuilding = !UnlimitedBuilding;
				Debug.Log("UnlimitedBuilding: " + UnlimitedBuilding);
			}
			if (Input.GetKeyDown(KeyCode.M))
			{
				Singleton<EconomyManager>.Instance.AddMoney(1000f);
			}
			if (Input.GetKeyDown(KeyCode.Z))
			{
				Singleton<QuestManager>.Instance.DebugCompleteNextQuest();
			}
		}
		else
		{
			CheckDevModeSecretMessage();
		}
	}

	public void ClearAllPhysicsOrePieces(bool keepOrePiecesThatAreInBaskets = true)
	{
		HashSet<OrePiece> hashSet = new HashSet<OrePiece>();
		if (keepOrePiecesThatAreInBaskets)
		{
			BaseBasket[] array = UnityEngine.Object.FindObjectsOfType<BaseBasket>();
			foreach (BaseBasket baseBasket in array)
			{
				hashSet.AddRange(baseBasket.GetOrePiecesInFilter());
			}
		}
		OrePiece[] array2 = UnityEngine.Object.FindObjectsOfType<OrePiece>();
		foreach (OrePiece orePiece in array2)
		{
			if (!hashSet.Contains(orePiece))
			{
				orePiece.Delete();
			}
		}
		ConveyorBelt[] array3 = UnityEngine.Object.FindObjectsOfType<ConveyorBelt>();
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].ClearNullObjectsOnBelt();
		}
		RollingMill[] array4 = UnityEngine.Object.FindObjectsOfType<RollingMill>();
		for (int i = 0; i < array4.Length; i++)
		{
			array4[i].OnAllOreCleared();
		}
		this.ClearedAllPhysicsOrePieces?.Invoke();
	}

	private void HandleTimeScaleAdjustment()
	{
		if (Input.GetKeyDown(KeyCode.Minus))
		{
			Time.timeScale = Mathf.Max(0.0625f, Time.timeScale * 0.5f);
			Debug.Log("Time Scale: " + Time.timeScale);
		}
		if (Input.GetKeyDown(KeyCode.Equals))
		{
			Time.timeScale *= 2f;
			Debug.Log("Time Scale: " + Time.timeScale);
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			Time.timeScale = 1f;
			Debug.Log("Reset Timescale");
		}
	}

	private void CheckDevModeSecretMessage()
	{
		string inputString = Input.inputString;
		foreach (char c in inputString)
		{
			if (Time.time - _lastInputTime > _resetDelay)
			{
				_inputBuffer = "";
			}
			_lastInputTime = Time.time;
			_inputBuffer += c;
			if (_inputBuffer.Length > _secretCode.Length)
			{
				_inputBuffer = _inputBuffer.Substring(_inputBuffer.Length - _secretCode.Length);
			}
			if (_inputBuffer.ToLower() == _secretCode.ToLower())
			{
				DevModeEnabled = true;
				Debug.Log("Developer Mode Enabled!");
				Singleton<SoundManager>.Instance.PlaySoundAtLocation(DevModeEnabledSound, UnityEngine.Object.FindObjectOfType<PlayerController>().transform.position);
				ComputerShopUI computerShopUI = UnityEngine.Object.FindObjectOfType<ComputerShopUI>(includeInactive: true);
				if (computerShopUI != null)
				{
					computerShopUI.SetupCategories();
				}
			}
		}
	}
}
