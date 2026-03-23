using System;
using TMPro;
using UnityEngine;

// Token: 0x020000F6 RID: 246
public class ToolResourceScanner : BaseHeldTool
{
	// Token: 0x06000688 RID: 1672 RVA: 0x000222D1 File Offset: 0x000204D1
	public override string GetControlsText()
	{
		return "Drop - " + Singleton<KeybindManager>.Instance.GetBindingText(KeybindAction.DropTool);
	}

	// Token: 0x06000689 RID: 1673 RVA: 0x000222EC File Offset: 0x000204EC
	public override void PrimaryFire()
	{
	}

	// Token: 0x0600068A RID: 1674 RVA: 0x000222FC File Offset: 0x000204FC
	private void Update()
	{
		if (this.Owner == null)
		{
			return;
		}
		if (this.Owner.Inventory.ActiveTool != this)
		{
			return;
		}
		Camera componentInChildren = this.Owner.GetComponentInChildren<Camera>();
		if (componentInChildren == null)
		{
			return;
		}
		string text = "No Target";
		RaycastHit raycastHit;
		if (Physics.Raycast(componentInChildren.transform.position, componentInChildren.transform.forward, out raycastHit, this.UseRange, this.Owner.InteractLayerMask))
		{
			text = this.GetThingNameText(raycastHit.collider.gameObject);
		}
		this.ThingNameText.text = text;
	}

	// Token: 0x0600068B RID: 1675 RVA: 0x000223A4 File Offset: 0x000205A4
	private string GetThingNameText(GameObject thing)
	{
		OreNode oreNode;
		if (this.TryGetComponentInParent<OreNode>(thing, out oreNode))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(oreNode.ResourceType, PieceType.Ore, false) + " Node";
		}
		if (thing.GetComponentInChildren<SellerMachine>())
		{
			return "Deposit Hopper";
		}
		BuildingPlacementNode buildingPlacementNode;
		if (this.TryGetComponentInParent<BuildingPlacementNode>(thing, out buildingPlacementNode))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(buildingPlacementNode.GetPrimaryResourceType(), PieceType.Ore, false) + " Auto-Miner Node";
		}
		if (thing.isStatic)
		{
			return "No Target";
		}
		OrePiece orePiece;
		if (this.TryGetComponentInParent<OrePiece>(thing, out orePiece))
		{
			return Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(orePiece.ResourceType, orePiece.PieceType, orePiece.PolishedPercent == 1f);
		}
		BuildingObject buildingObject;
		if (this.TryGetComponentInParent<BuildingObject>(thing, out buildingObject))
		{
			return buildingObject.Definition.Name;
		}
		BuildingCrate buildingCrate;
		if (this.TryGetComponentInParent<BuildingCrate>(thing, out buildingCrate))
		{
			return buildingCrate.Definition.Name;
		}
		BaseHeldTool baseHeldTool;
		if (this.TryGetComponentInParent<BaseHeldTool>(thing, out baseHeldTool))
		{
			return baseHeldTool.Name;
		}
		if (base.GetComponentInParent<ComputerTerminal>())
		{
			return "Computer Terminal";
		}
		return "Unknown";
	}

	// Token: 0x0600068C RID: 1676 RVA: 0x000224AB File Offset: 0x000206AB
	private bool TryGetComponentInParent<T>(GameObject obj, out T component) where T : Component
	{
		component = obj.GetComponentInParent<T>();
		return component != null;
	}

	// Token: 0x040007CC RID: 1996
	public float UseRange = 3f;

	// Token: 0x040007CD RID: 1997
	public TMP_Text ThingNameText;
}
