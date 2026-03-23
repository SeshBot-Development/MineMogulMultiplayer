using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020000DF RID: 223
public class SorterFilterBasket : BaseBasket
{
	// Token: 0x060005F9 RID: 1529 RVA: 0x0001F310 File Offset: 0x0001D510
	protected override void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this.AddToFilter(componentInParent);
		}
	}

	// Token: 0x060005FA RID: 1530 RVA: 0x0001F334 File Offset: 0x0001D534
	protected override void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this.RemoveFromFilter(componentInParent);
		}
	}

	// Token: 0x060005FB RID: 1531 RVA: 0x0001F358 File Offset: 0x0001D558
	public bool OreMatchesFilter(OrePiece ore)
	{
		return this._filterCriteria.Contains(new ValueTuple<ResourceType, PieceType>(ore.ResourceType, ore.PieceType));
	}

	// Token: 0x060005FC RID: 1532 RVA: 0x0001F378 File Offset: 0x0001D578
	public void AddToFilter(OrePiece ore)
	{
		if (!this._basketOreList.Contains(ore))
		{
			this._basketOreList.Add(ore);
			ore.BasketsThisIsInside.Add(this);
			this.UpdateFilter();
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.SetFilterSound, base.transform.position, 1.4f, 1f, true, false);
	}

	// Token: 0x060005FD RID: 1533 RVA: 0x0001F3DC File Offset: 0x0001D5DC
	public void RemoveFromFilter(OrePiece ore)
	{
		this._basketOreList.Remove(ore);
		ore.BasketsThisIsInside.Remove(this);
		this.UpdateFilter();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.SetFilterSound, base.transform.position, 0.6f, 1f, true, false);
	}

	// Token: 0x060005FE RID: 1534 RVA: 0x0001F430 File Offset: 0x0001D630
	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[this._lightMaterialIndex] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x060005FF RID: 1535 RVA: 0x0001F45E File Offset: 0x0001D65E
	protected override void OnDisable()
	{
		base.OnDisable();
		this._filterCriteria.Clear();
	}

	// Token: 0x06000600 RID: 1536 RVA: 0x0001F474 File Offset: 0x0001D674
	private void UpdateFilter()
	{
		this._filterCriteria.Clear();
		foreach (OrePiece orePiece in this._basketOreList)
		{
			ValueTuple<ResourceType, PieceType> valueTuple = new ValueTuple<ResourceType, PieceType>(orePiece.ResourceType, orePiece.PieceType);
			if (!this._filterCriteria.Contains(valueTuple))
			{
				this._filterCriteria.Add(valueTuple);
			}
		}
		if (this._filterCriteria.Count > 0)
		{
			this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
			return;
		}
		this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.RedLightMaterial);
	}

	// Token: 0x04000731 RID: 1841
	private List<ValueTuple<ResourceType, PieceType>> _filterCriteria = new List<ValueTuple<ResourceType, PieceType>>();

	// Token: 0x04000732 RID: 1842
	[SerializeField]
	private MeshRenderer _lightMeshRenderer;

	// Token: 0x04000733 RID: 1843
	[SerializeField]
	private int _lightMaterialIndex;

	// Token: 0x04000734 RID: 1844
	public SoundDefinition SetFilterSound;
}
