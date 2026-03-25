using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ChuteHatch : MonoBehaviour, IInteractable, ICustomSaveDataProvider
{
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
	private Vector3 ClosedRotation2;

	[SerializeField]
	private Vector3 OpenRotation2;

	[SerializeField]
	private Transform RotatingPart;

	[SerializeField]
	private Transform RotatingPart2;

	[SerializeField]
	private SoundDefinition ToggleSound;

	[SerializeField]
	private Renderer _lightRenderer;

	[SerializeField]
	private float _rotateDuration = 0.35f;

	[SerializeField]
	private Ease _rotateEase = Ease.OutCubic;

	private Tween _rotTween;

	private Tween _rotTween2;

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
		_rotTween2?.Kill();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(ToggleSound, RotatingPart.position);
		ChangeLightMaterial(closed ? Singleton<BuildingManager>.Instance.RedLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
		IsClosed = closed;
		Vector3 endValue = (closed ? ClosedRotation : OpenRotation);
		Vector3 endValue2 = (closed ? ClosedRotation2 : OpenRotation2);
		if (ClosedObjects != null)
		{
			ClosedObjects.SetActive(closed);
		}
		if (OpenObjects != null)
		{
			OpenObjects?.SetActive(!closed);
		}
		_rotTween = RotatingPart.DOLocalRotate(endValue, _rotateDuration).SetEase(_rotateEase);
		_rotTween2 = RotatingPart2.DOLocalRotate(endValue2, _rotateDuration).SetEase(_rotateEase);
	}

	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightRenderer.sharedMaterials;
		sharedMaterials[2] = material;
		_lightRenderer.sharedMaterials = sharedMaterials;
	}

	public void SetDirectionFromLoading(bool closed)
	{
		IsClosed = closed;
		ChangeLightMaterial(closed ? Singleton<BuildingManager>.Instance.RedLightMaterial : Singleton<BuildingManager>.Instance.GreenLightMaterial);
		Vector3 euler = (closed ? ClosedRotation : OpenRotation);
		Vector3 euler2 = (closed ? ClosedRotation2 : OpenRotation2);
		if (ClosedObjects != null)
		{
			ClosedObjects.SetActive(closed);
		}
		if (OpenObjects != null)
		{
			OpenObjects?.SetActive(!closed);
		}
		RotatingPart.localRotation = Quaternion.Euler(euler);
		RotatingPart2.localRotation = Quaternion.Euler(euler2);
	}

	public bool ShouldUseInteractionWheel()
	{
		return true;
	}

	public string GetObjectName()
	{
		return "Chute Hatch";
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public void Interact(Interaction selectedInteraction)
	{
		if (selectedInteraction.Name == "Toggle")
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
