using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RoutingConveyor : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
	public string Name = "Routing Conveyor";

	public bool IsClosed;

	[SerializeField]
	private GameObject ClosedObjects;

	[SerializeField]
	private GameObject OpenObjects;

	[SerializeField]
	private Vector3 ClosedRotation;

	[SerializeField]
	private Vector3 OpenRotation;

	[SerializeField]
	private Transform RotatingPart;

	[SerializeField]
	private SoundDefinition ToggleSound;

	[SerializeField]
	private float _rotateDuration = 0.35f;

	[SerializeField]
	private Ease _rotateEase = Ease.OutCubic;

	private Tween _rotTween;

	[SerializeField]
	private List<Interaction> _interactions;

	public void OnEnable()
	{
		SetDirectionFromLoading(IsClosed);
	}

	public void ToggleDirection()
	{
		SetDirection(!IsClosed);
	}

	public void SetDirection(bool closed)
	{
		_rotTween?.Kill();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(ToggleSound, RotatingPart.position);
		IsClosed = closed;
		Vector3 endValue = (closed ? ClosedRotation : OpenRotation);
		if (ClosedObjects != null)
		{
			ClosedObjects.SetActive(closed);
		}
		if (OpenObjects != null)
		{
			OpenObjects?.SetActive(!closed);
		}
		_rotTween = RotatingPart.DOLocalRotate(endValue, _rotateDuration).SetEase(_rotateEase);
	}

	public void SetDirectionFromLoading(bool closed)
	{
		IsClosed = closed;
		Vector3 euler = (closed ? ClosedRotation : OpenRotation);
		if (ClosedObjects != null)
		{
			ClosedObjects.SetActive(closed);
		}
		if (OpenObjects != null)
		{
			OpenObjects?.SetActive(!closed);
		}
		RotatingPart.localRotation = Quaternion.Euler(euler);
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public string GetObjectName()
	{
		return Name;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		string text = selectedInteraction.Name;
		if (!(text == "Switch Direction"))
		{
			if (text == "Toggle")
			{
				ToggleDirection();
			}
		}
		else
		{
			ToggleDirection();
		}
	}

	public virtual void LoadFromSave(string json)
	{
		RoutingConveyorSaveData routingConveyorSaveData = JsonUtility.FromJson<RoutingConveyorSaveData>(json);
		if (routingConveyorSaveData == null)
		{
			routingConveyorSaveData = new RoutingConveyorSaveData();
		}
		SetDirectionFromLoading(routingConveyorSaveData.IsClosed);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new RoutingConveyorSaveData
		{
			IsClosed = IsClosed
		});
	}
}
