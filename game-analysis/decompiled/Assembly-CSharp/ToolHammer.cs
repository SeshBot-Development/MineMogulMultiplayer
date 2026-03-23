using System;
using UnityEngine;

// Token: 0x020000EF RID: 239
public class ToolHammer : BaseHeldTool
{
	// Token: 0x06000661 RID: 1633 RVA: 0x000214F0 File Offset: 0x0001F6F0
	public override string GetControlsText()
	{
		return string.Concat(new string[]
		{
			"Drop - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool),
			"\nPickup Building - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack),
			"\nPack Building - ",
			Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack)
		});
	}

	// Token: 0x06000662 RID: 1634 RVA: 0x0002154C File Offset: 0x0001F74C
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
				componentInParent.TryTakeOrPack();
				return;
			}
			BuildingCrate componentInParent2 = raycastHit.collider.GetComponentInParent<BuildingCrate>();
			if (componentInParent2 != null)
			{
				componentInParent2.TryAddToInventory();
			}
		}
	}

	// Token: 0x06000663 RID: 1635 RVA: 0x000215DC File Offset: 0x0001F7DC
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
				componentInParent.Pack();
			}
		}
	}

	// Token: 0x040007A6 RID: 1958
	public float UseRange = 3f;
}
