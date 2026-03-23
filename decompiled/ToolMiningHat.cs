using UnityEngine;

public class ToolMiningHat : BaseHeldTool
{
	public bool IsOn;

	[SerializeField]
	private GameObject _worldModelLight;

	[SerializeField]
	private GameObject _viewModelLight;

	[SerializeField]
	private SoundDefinition _toggleSoundDefinition;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nToggle Light - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.ToggleFlashlight);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		ToggleLight(IsOn, playSound: false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ToggleLight(IsOn, playSound: false);
	}

	public void ToggleLight(bool enable, bool playSound = true, bool updateOnOwner = true)
	{
		IsOn = enable;
		_worldModelLight.SetActive(IsOn);
		_viewModelLight.SetActive(IsOn);
		if (Owner != null)
		{
			Owner.ToggleMiningLightFromTool(enable);
		}
		if (playSound)
		{
			Singleton<SoundManager>.Instance.PlayUISound(_toggleSoundDefinition);
		}
	}

	public override void PrimaryFire()
	{
		ToggleLight(!IsOn);
	}

	public override void Interact(Interaction selectedInteraction)
	{
		switch (selectedInteraction.Name)
		{
		case "Take":
			TryAddToInventory();
			break;
		case "Destroy":
			Object.Destroy(base.gameObject);
			break;
		case "Toggle":
			ToggleLight(!IsOn);
			break;
		}
	}
}
