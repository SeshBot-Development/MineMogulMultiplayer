using System;
using UnityEngine;

// Token: 0x020000F7 RID: 247
public class ToolSupportsWrench : BaseHeldTool
{
	// Token: 0x0600068E RID: 1678 RVA: 0x000224E0 File Offset: 0x000206E0
	public override string GetControlsText()
	{
		return string.Concat(new string[]
		{
			"Drop - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool),
			"\nDisable Building Supports - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack),
			"\nEnable Building Supports - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack)
		});
	}

	// Token: 0x0600068F RID: 1679 RVA: 0x0002253C File Offset: 0x0002073C
	public override void PrimaryFire()
	{
		if (this.Owner == null)
		{
			return;
		}
		Camera playerCamera = this.Owner.PlayerCamera;
		if (playerCamera == null)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, this.UseRange))
		{
			BuildingObject componentInParent = raycastHit.collider.GetComponentInParent<BuildingObject>();
			if (componentInParent != null)
			{
				componentInParent.EnableBuildingSupports(false);
				return;
			}
		}
	}

	// Token: 0x06000690 RID: 1680 RVA: 0x000225B4 File Offset: 0x000207B4
	public override void SecondaryFire()
	{
		if (this.Owner == null)
		{
			return;
		}
		Camera playerCamera = this.Owner.PlayerCamera;
		if (playerCamera == null)
		{
			return;
		}
		RaycastHit raycastHit;
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out raycastHit, this.UseRange))
		{
			BuildingObject componentInParent = raycastHit.collider.GetComponentInParent<BuildingObject>();
			if (componentInParent != null)
			{
				componentInParent.EnableBuildingSupports(true);
			}
		}
	}

	// Token: 0x040007CE RID: 1998
	public float UseRange = 3f;
}
