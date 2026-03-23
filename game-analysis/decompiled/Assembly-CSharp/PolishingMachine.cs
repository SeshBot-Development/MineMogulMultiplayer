using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000089 RID: 137
public class PolishingMachine : MonoBehaviour
{
	// Token: 0x060003C9 RID: 969 RVA: 0x000149A8 File Offset: 0x00012BA8
	private void OnEnable()
	{
		this.MakeClean();
	}

	// Token: 0x060003CA RID: 970 RVA: 0x000149B0 File Offset: 0x00012BB0
	private void Update()
	{
		this._polishingList.RemoveAll((OrePiece item) => item == null || !item.isActiveAndEnabled);
		foreach (OrePiece orePiece in this._polishingList)
		{
			if (orePiece.MakesPolishingMachineDirty)
			{
				this.MakeDirty();
				break;
			}
			if (!this._isDirty)
			{
				orePiece.AddPolish(Time.deltaTime / this.PolishingTime);
			}
		}
		if (this._isDirty)
		{
			this.PolishingAnimator.SetBool("IsPolishing", false);
			this.PolishingAnimator.SetBool("IsDirty", true);
			if ((in this._timeUntilClean) <= 0f)
			{
				this.MakeClean();
				return;
			}
		}
		else
		{
			this.PolishingAnimator.SetBool("IsPolishing", this._polishingList.Count > 0);
		}
	}

	// Token: 0x060003CB RID: 971 RVA: 0x00014AB4 File Offset: 0x00012CB4
	private void MakeDirty()
	{
		this._timeUntilClean = this._timeToStayDirty;
		if (this._isDirty)
		{
			return;
		}
		this._isDirty = true;
		foreach (Renderer renderer in this._brushRenderersToMakeDirty)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			sharedMaterials[0] = this._dirtyBrushMaterial;
			sharedMaterials[1] = this._dirtyMachineMaterial;
			renderer.sharedMaterials = sharedMaterials;
		}
		this.ChangeMachineMaterial(this._dirtyMachineMaterial);
		this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
		this._conveyor.ChangeSpeed(this._dirtyConveyorSpeed);
		this.DirtySoundPlayer.Play();
	}

	// Token: 0x060003CC RID: 972 RVA: 0x00014B78 File Offset: 0x00012D78
	private void MakeClean()
	{
		this._isDirty = false;
		foreach (Renderer renderer in this._brushRenderersToMakeDirty)
		{
			Material[] sharedMaterials = renderer.sharedMaterials;
			sharedMaterials[0] = this._cleanBrushMaterial;
			sharedMaterials[1] = this._cleanMachineMaterial;
			renderer.sharedMaterials = sharedMaterials;
		}
		this.ChangeMachineMaterial(this._cleanMachineMaterial);
		this.ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
		this._conveyor.ChangeSpeed(this._standardConveyorSpeed);
		this.PolishingAnimator.SetBool("IsDirty", false);
		this.DirtySoundPlayer.Stop();
	}

	// Token: 0x060003CD RID: 973 RVA: 0x00014C34 File Offset: 0x00012E34
	protected void ChangeMachineMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[0] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x060003CE RID: 974 RVA: 0x00014C60 File Offset: 0x00012E60
	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightMeshRenderer.sharedMaterials;
		sharedMaterials[2] = material;
		this._lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x060003CF RID: 975 RVA: 0x00014C8C File Offset: 0x00012E8C
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this._polishingList.Add(componentInParent);
		}
	}

	// Token: 0x060003D0 RID: 976 RVA: 0x00014CB8 File Offset: 0x00012EB8
	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			this._polishingList.Remove(componentInParent);
		}
	}

	// Token: 0x04000406 RID: 1030
	public Animator PolishingAnimator;

	// Token: 0x04000407 RID: 1031
	public float PolishingTime = 1.75f;

	// Token: 0x04000408 RID: 1032
	[SerializeField]
	private float _timeToStayDirty = 10f;

	// Token: 0x04000409 RID: 1033
	[SerializeField]
	private float _standardConveyorSpeed;

	// Token: 0x0400040A RID: 1034
	[SerializeField]
	private float _dirtyConveyorSpeed;

	// Token: 0x0400040B RID: 1035
	[SerializeField]
	private ConveyorBelt _conveyor;

	// Token: 0x0400040C RID: 1036
	[FormerlySerializedAs("_renderersToMakeDirty")]
	[SerializeField]
	private List<Renderer> _brushRenderersToMakeDirty;

	// Token: 0x0400040D RID: 1037
	[SerializeField]
	private Material _cleanMachineMaterial;

	// Token: 0x0400040E RID: 1038
	[SerializeField]
	private Material _dirtyMachineMaterial;

	// Token: 0x0400040F RID: 1039
	[SerializeField]
	private Material _cleanBrushMaterial;

	// Token: 0x04000410 RID: 1040
	[SerializeField]
	private Material _dirtyBrushMaterial;

	// Token: 0x04000411 RID: 1041
	[SerializeField]
	private Renderer _lightMeshRenderer;

	// Token: 0x04000412 RID: 1042
	[SerializeField]
	private LoopingSoundPlayer DirtySoundPlayer;

	// Token: 0x04000413 RID: 1043
	private bool _isDirty;

	// Token: 0x04000414 RID: 1044
	private List<OrePiece> _polishingList = new List<OrePiece>();

	// Token: 0x04000415 RID: 1045
	private TimeUntil _timeUntilClean;
}
