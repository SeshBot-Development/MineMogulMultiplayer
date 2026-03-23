using System;
using UnityEngine;

// Token: 0x020000F4 RID: 244
public class ToolMiningHat : BaseHeldTool
{
	// Token: 0x0600067A RID: 1658 RVA: 0x00022066 File Offset: 0x00020266
	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nToggle Light - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.ToggleFlashlight);
	}

	// Token: 0x0600067B RID: 1659 RVA: 0x0002208F File Offset: 0x0002028F
	protected override void OnEnable()
	{
		base.OnEnable();
		this.ToggleLight(this.IsOn, false, true);
	}

	// Token: 0x0600067C RID: 1660 RVA: 0x000220A5 File Offset: 0x000202A5
	protected override void OnDisable()
	{
		base.OnDisable();
		this.ToggleLight(this.IsOn, false, true);
	}

	// Token: 0x0600067D RID: 1661 RVA: 0x000220BC File Offset: 0x000202BC
	public void ToggleLight(bool enable, bool playSound = true, bool updateOnOwner = true)
	{
		this.IsOn = enable;
		this._worldModelLight.SetActive(this.IsOn);
		this._viewModelLight.SetActive(this.IsOn);
		if (this.Owner != null)
		{
			this.Owner.ToggleMiningLightFromTool(enable);
		}
		if (playSound)
		{
			Singleton<SoundManager>.Instance.PlayUISound(this._toggleSoundDefinition, 1f);
		}
	}

	// Token: 0x0600067E RID: 1662 RVA: 0x00022124 File Offset: 0x00020324
	public override void PrimaryFire()
	{
		this.ToggleLight(!this.IsOn, true, true);
	}

	// Token: 0x0600067F RID: 1663 RVA: 0x00022138 File Offset: 0x00020338
	public override void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Take")
		{
			this.TryAddToInventory(-1);
			return;
		}
		if (name == "Destroy")
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (!(name == "Toggle"))
		{
			return;
		}
		this.ToggleLight(!this.IsOn, true, true);
	}

	// Token: 0x040007BE RID: 1982
	public bool IsOn;

	// Token: 0x040007BF RID: 1983
	[SerializeField]
	private GameObject _worldModelLight;

	// Token: 0x040007C0 RID: 1984
	[SerializeField]
	private GameObject _viewModelLight;

	// Token: 0x040007C1 RID: 1985
	[SerializeField]
	private SoundDefinition _toggleSoundDefinition;
}
