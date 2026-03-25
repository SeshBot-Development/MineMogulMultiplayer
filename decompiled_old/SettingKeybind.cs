using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SettingKeybind : BaseSettingOption
{
	[Header("Keybind")]
	public KeybindAction Action;

	[SerializeField]
	private TMP_Text _keybindLabel;

	[SerializeField]
	private GameObject _hideWhenUsingDefaultBind;

	private InputActionRebindingExtensions.RebindingOperation _rebindOp;

	private PlayerInput _playerInput;

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateUI();
	}

	public void StartRebind()
	{
		if (Singleton<KeybindManager>.Instance.TryGetBinding(Action, out var action, out var bindingIndex))
		{
			EventSystem.current?.SetSelectedGameObject(null);
			_keybindLabel.text = "Press a button";
			action.Disable();
			_rebindOp?.Dispose();
			_rebindOp = action.PerformInteractiveRebinding(bindingIndex).WithCancelingThrough("<Keyboard>/escape").WithControlsExcluding("<Mouse>/position")
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
					UpdateUI();
				});
			_rebindOp.Start();
		}
	}

	private void OnDisable()
	{
		_rebindOp?.Dispose();
		_rebindOp = null;
	}

	public void UpdateUI()
	{
		_keybindLabel.text = Singleton<KeybindManager>.Instance.GetBindingText(Action);
		_hideWhenUsingDefaultBind.SetActive(!Singleton<KeybindManager>.Instance.IsUsingDefaultBind(Action));
	}

	public void ResetBind()
	{
		Singleton<KeybindManager>.Instance.ResetBindingToDefault(Action);
		UpdateUI();
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (displayName != "Unnamed Keybind")
		{
			base.name = "Keybind - " + displayName;
		}
	}
}
