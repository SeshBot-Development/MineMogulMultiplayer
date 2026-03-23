using TMPro;
using UnityEngine;

public class ToolResourceScanner : BaseHeldTool
{
	public float UseRange = 3f;

	public TMP_Text ThingNameText;

	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool);
	}

	public override void PrimaryFire()
	{
	}

	private void Update()
	{
		if (Owner == null || Owner.Inventory.ActiveTool != this)
		{
			return;
		}
		Camera componentInChildren = Owner.GetComponentInChildren<Camera>();
		if (!(componentInChildren == null))
		{
			string text = "No Target";
			if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out var hitInfo, UseRange, Owner.InteractLayerMask))
			{
				text = GetThingNameText(hitInfo.collider.gameObject);
			}
			ThingNameText.text = text;
		}
	}

	private string GetThingNameText(GameObject thing)
	{
		if (TryGetComponentInParent<OreNode>(thing, out var component))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(component.ResourceType, PieceType.Ore) + " Node";
		}
		if ((bool)thing.GetComponentInChildren<SellerMachine>())
		{
			return "Deposit Hopper";
		}
		if (TryGetComponentInParent<BuildingPlacementNode>(thing, out var component2))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(component2.GetPrimaryResourceType(), PieceType.Ore) + " Auto-Miner Node";
		}
		if (thing.isStatic)
		{
			return "No Target";
		}
		if (TryGetComponentInParent<OrePiece>(thing, out var component3))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(component3.ResourceType, component3.PieceType, component3.PolishedPercent == 1f);
		}
		if (TryGetComponentInParent<BuildingObject>(thing, out var component4))
		{
			return component4.Definition.Name;
		}
		if (TryGetComponentInParent<BuildingCrate>(thing, out var component5))
		{
			return component5.Definition.Name;
		}
		if (TryGetComponentInParent<BaseHeldTool>(thing, out var component6))
		{
			return component6.Name;
		}
		if ((bool)GetComponentInParent<ComputerTerminal>())
		{
			return "Computer Terminal";
		}
		return "Unknown";
	}

	private bool TryGetComponentInParent<T>(GameObject obj, out T component) where T : Component
	{
		component = obj.GetComponentInParent<T>();
		return component != null;
	}
}
