using UnityEngine;

public class ToolHammer : BaseHeldTool
{
	public float UseRange = 3f;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nPickup Building - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack) + "\nPack Building - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack);
	}

	public override void PrimaryFire()
	{
		if (Owner == null)
		{
			return;
		}
		Camera playerCamera = Owner.PlayerCamera;
		if (playerCamera == null || !Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hitInfo, UseRange))
		{
			return;
		}
		BuildingObject componentInParent = hitInfo.collider.GetComponentInParent<BuildingObject>();
		if (componentInParent != null)
		{
			componentInParent.TryTakeOrPack();
			return;
		}
		BuildingCrate componentInParent2 = hitInfo.collider.GetComponentInParent<BuildingCrate>();
		if (componentInParent2 != null)
		{
			componentInParent2.TryAddToInventory();
		}
	}

	public override void SecondaryFire()
	{
		if (Owner == null)
		{
			return;
		}
		Camera playerCamera = Owner.PlayerCamera;
		if (!(playerCamera == null) && Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hitInfo, UseRange))
		{
			BuildingObject componentInParent = hitInfo.collider.GetComponentInParent<BuildingObject>();
			if (componentInParent != null)
			{
				componentInParent.Pack();
			}
		}
	}
}
