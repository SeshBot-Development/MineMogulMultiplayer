using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x020000AE RID: 174
public class RoutingConveyor : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	// Token: 0x060004D0 RID: 1232 RVA: 0x00019D9D File Offset: 0x00017F9D
	public void OnEnable()
	{
		this.SetDirectionFromLoading(this.IsClosed);
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x00019DAB File Offset: 0x00017FAB
	public void ToggleDirection()
	{
		this.SetDirection(!this.IsClosed);
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x00019DBC File Offset: 0x00017FBC
	public void SetDirection(bool closed)
	{
		Tween rotTween = this._rotTween;
		if (rotTween != null)
		{
			rotTween.Kill(false);
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.ToggleSound, this.RotatingPart.position, 1f, 1f, true, false);
		this.IsClosed = closed;
		Vector3 vector = (closed ? this.ClosedRotation : this.OpenRotation);
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
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x00019E7C File Offset: 0x0001807C
	public void SetDirectionFromLoading(bool closed)
	{
		this.IsClosed = closed;
		Vector3 vector = (closed ? this.ClosedRotation : this.OpenRotation);
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
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x00019EF0 File Offset: 0x000180F0
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x060004D5 RID: 1237 RVA: 0x00019EF3 File Offset: 0x000180F3
	public string GetObjectName()
	{
		return this.Name;
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x00019EFB File Offset: 0x000180FB
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x060004D7 RID: 1239 RVA: 0x00019F04 File Offset: 0x00018104
	public void Interact(Interaction selectedInteraction)
	{
		string name = selectedInteraction.Name;
		if (name == "Switch Direction")
		{
			this.ToggleDirection();
			return;
		}
		if (!(name == "Toggle"))
		{
			return;
		}
		this.ToggleDirection();
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x00019F40 File Offset: 0x00018140
	public virtual void LoadFromSave(string json)
	{
		RoutingConveyorSaveData routingConveyorSaveData = JsonUtility.FromJson<RoutingConveyorSaveData>(json);
		if (routingConveyorSaveData == null)
		{
			routingConveyorSaveData = new RoutingConveyorSaveData();
		}
		this.SetDirectionFromLoading(routingConveyorSaveData.IsClosed);
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x00019F69 File Offset: 0x00018169
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new RoutingConveyorSaveData
		{
			IsClosed = this.IsClosed
		});
	}

	// Token: 0x04000572 RID: 1394
	public string Name = "Routing Conveyor";

	// Token: 0x04000573 RID: 1395
	public bool IsClosed;

	// Token: 0x04000574 RID: 1396
	[SerializeField]
	private GameObject ClosedObjects;

	// Token: 0x04000575 RID: 1397
	[SerializeField]
	private GameObject OpenObjects;

	// Token: 0x04000576 RID: 1398
	[SerializeField]
	private Vector3 ClosedRotation;

	// Token: 0x04000577 RID: 1399
	[SerializeField]
	private Vector3 OpenRotation;

	// Token: 0x04000578 RID: 1400
	[SerializeField]
	private Transform RotatingPart;

	// Token: 0x04000579 RID: 1401
	[SerializeField]
	private SoundDefinition ToggleSound;

	// Token: 0x0400057A RID: 1402
	[SerializeField]
	private float _rotateDuration = 0.35f;

	// Token: 0x0400057B RID: 1403
	[SerializeField]
	private Ease _rotateEase = Ease.OutCubic;

	// Token: 0x0400057C RID: 1404
	private Tween _rotTween;

	// Token: 0x0400057D RID: 1405
	[SerializeField]
	private List<Interaction> _interactions;
}
