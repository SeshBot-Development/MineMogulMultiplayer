using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x02000025 RID: 37
public class ChuteHatch : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	// Token: 0x0600011D RID: 285 RVA: 0x00007011 File Offset: 0x00005211
	public void OnEnable()
	{
		this.SetDirectionFromLoading(this.IsClosed);
	}

	// Token: 0x0600011E RID: 286 RVA: 0x0000701F File Offset: 0x0000521F
	public void ToggleDirection()
	{
		this.SetDirection(!this.IsClosed);
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00007030 File Offset: 0x00005230
	public void SetDirection(bool closed)
	{
		Tween rotTween = this._rotTween;
		if (rotTween != null)
		{
			rotTween.Kill(false);
		}
		Tween rotTween2 = this._rotTween2;
		if (rotTween2 != null)
		{
			rotTween2.Kill(false);
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.ToggleSound, this.RotatingPart.position, 1f, 1f, true, false);
		this.ChangeLightMaterial(closed ? Singleton<BuildingManager>.Instance.RedLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
		this.IsClosed = closed;
		Vector3 vector = (closed ? this.ClosedRotation : this.OpenRotation);
		Vector3 vector2 = (closed ? this.ClosedRotation2 : this.OpenRotation2);
		if (this.ClosedObjects != null)
		{
			this.ClosedObjects.SetActive(closed);
		}
		if (this.OpenObjects != null)
		{
			GameObject openObjects = this.OpenObjects;
			if (openObjects != null)
			{
				openObjects.SetActive(!closed);
			}
		}
		this._rotTween = this.RotatingPart.DOLocalRotate(vector, this._rotateDuration, RotateMode.Fast).SetEase(this._rotateEase);
		this._rotTween2 = this.RotatingPart2.DOLocalRotate(vector2, this._rotateDuration, RotateMode.Fast).SetEase(this._rotateEase);
	}

	// Token: 0x06000120 RID: 288 RVA: 0x00007158 File Offset: 0x00005358
	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = this._lightRenderer.sharedMaterials;
		sharedMaterials[2] = material;
		this._lightRenderer.sharedMaterials = sharedMaterials;
	}

	// Token: 0x06000121 RID: 289 RVA: 0x00007184 File Offset: 0x00005384
	public void SetDirectionFromLoading(bool closed)
	{
		this.IsClosed = closed;
		this.ChangeLightMaterial(closed ? Singleton<BuildingManager>.Instance.RedLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
		Vector3 vector = (closed ? this.ClosedRotation : this.OpenRotation);
		Vector3 vector2 = (closed ? this.ClosedRotation2 : this.OpenRotation2);
		if (this.ClosedObjects != null)
		{
			this.ClosedObjects.SetActive(closed);
		}
		if (this.OpenObjects != null)
		{
			GameObject openObjects = this.OpenObjects;
			if (openObjects != null)
			{
				openObjects.SetActive(!closed);
			}
		}
		this.RotatingPart.localRotation = Quaternion.Euler(vector);
		this.RotatingPart2.localRotation = Quaternion.Euler(vector2);
	}

	// Token: 0x06000122 RID: 290 RVA: 0x0000723A File Offset: 0x0000543A
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x06000123 RID: 291 RVA: 0x0000723D File Offset: 0x0000543D
	public string GetObjectName()
	{
		return "Chute Hatch";
	}

	// Token: 0x06000124 RID: 292 RVA: 0x00007244 File Offset: 0x00005444
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000125 RID: 293 RVA: 0x0000724C File Offset: 0x0000544C
	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Toggle")
		{
			this.ToggleDirection();
		}
	}

	// Token: 0x06000126 RID: 294 RVA: 0x00007268 File Offset: 0x00005468
	public virtual void LoadFromSave(string json)
	{
		RoutingConveyorSaveData routingConveyorSaveData = JsonUtility.FromJson<RoutingConveyorSaveData>(json);
		if (routingConveyorSaveData == null)
		{
			routingConveyorSaveData = new RoutingConveyorSaveData();
		}
		this.SetDirectionFromLoading(routingConveyorSaveData.IsClosed);
	}

	// Token: 0x06000127 RID: 295 RVA: 0x00007291 File Offset: 0x00005491
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new RoutingConveyorSaveData
		{
			IsClosed = this.IsClosed
		});
	}

	// Token: 0x04000114 RID: 276
	public bool IsClosed;

	// Token: 0x04000115 RID: 277
	[SerializeField]
	private GameObject ClosedObjects;

	// Token: 0x04000116 RID: 278
	[SerializeField]
	private GameObject OpenObjects;

	// Token: 0x04000117 RID: 279
	[SerializeField]
	private Vector3 ClosedRotation;

	// Token: 0x04000118 RID: 280
	[SerializeField]
	private Vector3 OpenRotation;

	// Token: 0x04000119 RID: 281
	[SerializeField]
	private Vector3 ClosedRotation2;

	// Token: 0x0400011A RID: 282
	[SerializeField]
	private Vector3 OpenRotation2;

	// Token: 0x0400011B RID: 283
	[SerializeField]
	private Transform RotatingPart;

	// Token: 0x0400011C RID: 284
	[SerializeField]
	private Transform RotatingPart2;

	// Token: 0x0400011D RID: 285
	[SerializeField]
	private SoundDefinition ToggleSound;

	// Token: 0x0400011E RID: 286
	[SerializeField]
	private Renderer _lightRenderer;

	// Token: 0x0400011F RID: 287
	[SerializeField]
	private float _rotateDuration = 0.35f;

	// Token: 0x04000120 RID: 288
	[SerializeField]
	private Ease _rotateEase = Ease.OutCubic;

	// Token: 0x04000121 RID: 289
	private Tween _rotTween;

	// Token: 0x04000122 RID: 290
	private Tween _rotTween2;

	// Token: 0x04000123 RID: 291
	[SerializeField]
	private List<Interaction> _interactions;
}
