using System;
using UnityEngine;

// Token: 0x02000024 RID: 36
public class CastingFurnaceMoldArea : MonoBehaviour
{
	// Token: 0x06000116 RID: 278 RVA: 0x00006E80 File Offset: 0x00005080
	public void Initialize(CastingFurnace owner, int slotNumber, CastingMoldType type)
	{
		this._owner = owner;
		this._slotNumber = slotNumber;
		this.CastingMoldType = type;
		this.UpdateRender();
	}

	// Token: 0x06000117 RID: 279 RVA: 0x00006EA0 File Offset: 0x000050A0
	public void UpdateRender()
	{
		if (this.CastingMoldType == CastingMoldType.None)
		{
			this._moldRenderer.gameObject.SetActive(false);
			return;
		}
		CastingMoldRendererInfo castingMoldRendererInfo = this._owner.GetCastingMoldRendererInfo(this.CastingMoldType);
		this._moldRenderer.sharedMaterial = castingMoldRendererInfo.Material;
		this._moldRenderer.GetComponent<MeshFilter>().mesh = castingMoldRendererInfo.Mesh;
		this._moldRenderer.gameObject.SetActive(true);
	}

	// Token: 0x06000118 RID: 280 RVA: 0x00006F11 File Offset: 0x00005111
	public void InsertMoldFromLoading(CastingMoldType type)
	{
		this.CastingMoldType = type;
		this.UpdateRender();
	}

	// Token: 0x06000119 RID: 281 RVA: 0x00006F20 File Offset: 0x00005120
	public void InsertMold(CastingMoldType type)
	{
		this.EjectMold();
		this.CastingMoldType = type;
		this.UpdateRender();
		this._owner.RecalculateMaterialAmountRequired();
	}

	// Token: 0x0600011A RID: 282 RVA: 0x00006F40 File Offset: 0x00005140
	public void EjectMold()
	{
		if (this.CastingMoldType == CastingMoldType.None)
		{
			return;
		}
		GameObject gameObject = Singleton<SavingLoadingManager>.Instance.AllSavableObjectPrefabs.Find(delegate(GameObject go)
		{
			ToolCastingMold toolCastingMold;
			return go && go.TryGetComponent<ToolCastingMold>(out toolCastingMold) && toolCastingMold.CastingMoldType == this.CastingMoldType;
		});
		if (gameObject != null)
		{
			Object.Instantiate<GameObject>(gameObject, this.MoldToolEjectTransform.position, this.MoldToolEjectTransform.transform.rotation);
		}
		else
		{
			Debug.LogError("Unable to spawn casting mold item, can not find prefab for type: " + this.CastingMoldType.ToString());
		}
		this.CastingMoldType = CastingMoldType.None;
		this.UpdateRender();
		this._owner.RecalculateMaterialAmountRequired();
	}

	// Token: 0x0400010D RID: 269
	public Transform MoldToolEjectTransform;

	// Token: 0x0400010E RID: 270
	public Transform ProductEjectTransform;

	// Token: 0x0400010F RID: 271
	public Transform SecondaryProductEjectTransform;

	// Token: 0x04000110 RID: 272
	[HideInInspector]
	public CastingMoldType CastingMoldType;

	// Token: 0x04000111 RID: 273
	[SerializeField]
	private Renderer _moldRenderer;

	// Token: 0x04000112 RID: 274
	private CastingFurnace _owner;

	// Token: 0x04000113 RID: 275
	private int _slotNumber;
}
