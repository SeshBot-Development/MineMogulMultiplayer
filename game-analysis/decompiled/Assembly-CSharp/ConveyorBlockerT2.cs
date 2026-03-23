using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

// Token: 0x02000038 RID: 56
public class ConveyorBlockerT2 : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	// Token: 0x06000189 RID: 393 RVA: 0x000088CE File Offset: 0x00006ACE
	public void OnEnable()
	{
		this.SetClosedFromLoading(this.IsClosed);
	}

	// Token: 0x0600018A RID: 394 RVA: 0x000088DC File Offset: 0x00006ADC
	public void SetClosedFromLoading(bool isClosed)
	{
		this.IsClosed = isClosed;
		if (this.IsClosed)
		{
			this.MovingPart.localPosition = this.ClosedPosition;
			return;
		}
		this.MovingPart.localPosition = this.OpenPosition;
	}

	// Token: 0x0600018B RID: 395 RVA: 0x00008910 File Offset: 0x00006B10
	public void Toggle()
	{
		this.SetClosed(!this.IsClosed);
	}

	// Token: 0x0600018C RID: 396 RVA: 0x00008924 File Offset: 0x00006B24
	public void SetClosed(bool closed)
	{
		Tween moveTween = this._moveTween;
		if (moveTween != null)
		{
			moveTween.Kill(false);
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(this.ToggleSound, this.MovingPart.position, 1f, 1f, true, false);
		this.IsClosed = closed;
		Vector3 vector = (closed ? this.ClosedPosition : this.OpenPosition);
		this._moveTween = this.MovingPart.DOLocalMove(vector, this._moveDuration, false).SetEase(this._moveEase);
	}

	// Token: 0x0600018D RID: 397 RVA: 0x000089A7 File Offset: 0x00006BA7
	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	// Token: 0x0600018E RID: 398 RVA: 0x000089AA File Offset: 0x00006BAA
	public string GetObjectName()
	{
		return "Conveyor Blocker T2";
	}

	// Token: 0x0600018F RID: 399 RVA: 0x000089B1 File Offset: 0x00006BB1
	public List<Interaction> GetInteractions()
	{
		return this._interactions;
	}

	// Token: 0x06000190 RID: 400 RVA: 0x000089B9 File Offset: 0x00006BB9
	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Toggle")
		{
			this.Toggle();
		}
	}

	// Token: 0x06000191 RID: 401 RVA: 0x000089D4 File Offset: 0x00006BD4
	public virtual void LoadFromSave(string json)
	{
		RoutingConveyorSaveData routingConveyorSaveData = JsonUtility.FromJson<RoutingConveyorSaveData>(json);
		if (routingConveyorSaveData == null)
		{
			routingConveyorSaveData = new RoutingConveyorSaveData();
		}
		this.SetClosedFromLoading(routingConveyorSaveData.IsClosed);
	}

	// Token: 0x06000192 RID: 402 RVA: 0x000089FD File Offset: 0x00006BFD
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new RoutingConveyorSaveData
		{
			IsClosed = this.IsClosed
		});
	}

	// Token: 0x04000170 RID: 368
	public bool IsClosed;

	// Token: 0x04000171 RID: 369
	[SerializeField]
	private Vector3 ClosedPosition;

	// Token: 0x04000172 RID: 370
	[SerializeField]
	private Vector3 OpenPosition;

	// Token: 0x04000173 RID: 371
	[SerializeField]
	private Transform MovingPart;

	// Token: 0x04000174 RID: 372
	[SerializeField]
	private SoundDefinition ToggleSound;

	// Token: 0x04000175 RID: 373
	[SerializeField]
	private float _moveDuration = 0.35f;

	// Token: 0x04000176 RID: 374
	[SerializeField]
	private Ease _moveEase = Ease.OutCubic;

	// Token: 0x04000177 RID: 375
	private Tween _moveTween;

	// Token: 0x04000178 RID: 376
	[SerializeField]
	private List<Interaction> _interactions;
}
