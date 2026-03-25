using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ConveyorBlockerT2 : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	public bool IsClosed;

	[SerializeField]
	private Vector3 ClosedPosition;

	[SerializeField]
	private Vector3 OpenPosition;

	[SerializeField]
	private Transform MovingPart;

	[SerializeField]
	private SoundDefinition ToggleSound;

	[SerializeField]
	private float _moveDuration = 0.35f;

	[SerializeField]
	private Ease _moveEase = Ease.OutCubic;

	private Tween _moveTween;

	[SerializeField]
	private List<Interaction> _interactions;

	public void OnEnable()
	{
		SetClosedFromLoading(IsClosed);
	}

	public void SetClosedFromLoading(bool isClosed)
	{
		IsClosed = isClosed;
		if (IsClosed)
		{
			MovingPart.localPosition = ClosedPosition;
		}
		else
		{
			MovingPart.localPosition = OpenPosition;
		}
	}

	public void Toggle()
	{
		SetClosed(!IsClosed);
	}

	public void SetClosed(bool closed)
	{
		_moveTween?.Kill();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(ToggleSound, MovingPart.position);
		IsClosed = closed;
		Vector3 endValue = (closed ? ClosedPosition : OpenPosition);
		_moveTween = MovingPart.DOLocalMove(endValue, _moveDuration).SetEase(_moveEase);
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public string GetObjectName()
	{
		return "Conveyor Blocker T2";
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Toggle")
		{
			Toggle();
		}
	}

	public virtual void LoadFromSave(string json)
	{
		RoutingConveyorSaveData routingConveyorSaveData = JsonUtility.FromJson<RoutingConveyorSaveData>(json);
		if (routingConveyorSaveData == null)
		{
			routingConveyorSaveData = new RoutingConveyorSaveData();
		}
		SetClosedFromLoading(routingConveyorSaveData.IsClosed);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new RoutingConveyorSaveData
		{
			IsClosed = IsClosed
		});
	}
}
