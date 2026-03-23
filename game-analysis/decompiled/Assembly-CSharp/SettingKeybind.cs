using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Token: 0x020000D1 RID: 209
public class SettingKeybind : BaseSettingOption
{
	// Token: 0x06000576 RID: 1398 RVA: 0x0001CD89 File Offset: 0x0001AF89
	protected override void OnEnable()
	{
		base.OnEnable();
		this.UpdateUI();
	}

	// Token: 0x06000577 RID: 1399 RVA: 0x0001CD98 File Offset: 0x0001AF98
	public void StartRebind()
	{
		InputAction action;
		int num;
		if (!Singleton<KeybindManager>.Instance.TryGetBinding(this.Action, out action, out num))
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			current.SetSelectedGameObject(null);
		}
		this._keybindLabel.text = "Press a button";
		action.Disable();
		InputActionRebindingExtensions.RebindingOperation rebindOp = this._rebindOp;
		if (rebindOp != null)
		{
			rebindOp.Dispose();
		}
		this._rebindOp = action.PerformInteractiveRebinding(num).WithCancelingThrough("<Keyboard>/escape").WithControlsExcluding("<Mouse>/position")
			.WithControlsExcluding("<Mouse>/delta")
			.OnMatchWaitForAnother(0.1f)
			.OnCancel(delegate(InputActionRebindingExtensions.RebindingOperation op)
			{
				action.Enable();
				op.Dispose();
			})
			.OnComplete(delegate(InputActionRebindingExtensions.RebindingOperation op)
			{
				action.Enable();
				op.Dispose();
				Singleton<KeybindManager>.Instance.HasUnsavedKeybindChanges = true;
				this.UpdateUI();
			});
		this._rebindOp.Start();
	}

	// Token: 0x06000578 RID: 1400 RVA: 0x0001CE72 File Offset: 0x0001B072
	private void OnDisable()
	{
		InputActionRebindingExtensions.RebindingOperation rebindOp = this._rebindOp;
		if (rebindOp != null)
		{
			rebindOp.Dispose();
		}
		this._rebindOp = null;
	}

	// Token: 0x06000579 RID: 1401 RVA: 0x0001CE8C File Offset: 0x0001B08C
	public void UpdateUI()
	{
		this._keybindLabel.text = Singleton<KeybindManager>.Instance.GetBindingText(this.Action);
		this._hideWhenUsingDefaultBind.SetActive(!Singleton<KeybindManager>.Instance.IsUsingDefaultBind(this.Action));
	}

	// Token: 0x0600057A RID: 1402 RVA: 0x0001CEC7 File Offset: 0x0001B0C7
	public void ResetBind()
	{
		Singleton<KeybindManager>.Instance.ResetBindingToDefault(this.Action);
		this.UpdateUI();
	}

	// Token: 0x0600057B RID: 1403 RVA: 0x0001CEDF File Offset: 0x0001B0DF
	protected override void OnValidate()
	{
		base.OnValidate();
		if (this.displayName != "Unnamed Keybind")
		{
			base.name = "Keybind - " + this.displayName;
		}
	}

	// Token: 0x040006AC RID: 1708
	[Header("Keybind")]
	public KeybindAction Action;

	// Token: 0x040006AD RID: 1709
	[SerializeField]
	private TMP_Text _keybindLabel;

	// Token: 0x040006AE RID: 1710
	[SerializeField]
	private GameObject _hideWhenUsingDefaultBind;

	// Token: 0x040006AF RID: 1711
	private InputActionRebindingExtensions.RebindingOperation _rebindOp;

	// Token: 0x040006B0 RID: 1712
	private PlayerInput _playerInput;
}
