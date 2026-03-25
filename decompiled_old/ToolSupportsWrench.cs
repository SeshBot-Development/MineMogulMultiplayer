using UnityEngine;

public class ToolSupportsWrench : BaseHeldTool
{
	public float UseRange = 3f;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool) + "\nDisable Building Supports - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.PrimaryAttack) + "\nEnable Building Supports - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.SecondaryAttack);
	}

	public override void PrimaryFire()
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
				componentInParent.EnableBuildingSupports(enabled: false);
			}
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
				componentInParent.EnableBuildingSupports(enabled: true);
			}
		}
	}
}
