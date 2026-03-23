using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PolishingMachine : MonoBehaviour
{
	public Animator PolishingAnimator;

	public float PolishingTime = 1.75f;

	[SerializeField]
	private float _timeToStayDirty = 10f;

	[SerializeField]
	private float _standardConveyorSpeed;

	[SerializeField]
	private float _dirtyConveyorSpeed;

	[SerializeField]
	private ConveyorBelt _conveyor;

	[FormerlySerializedAs("_renderersToMakeDirty")]
	[SerializeField]
	private List<Renderer> _brushRenderersToMakeDirty;

	[SerializeField]
	private Material _cleanMachineMaterial;

	[SerializeField]
	private Material _dirtyMachineMaterial;

	[SerializeField]
	private Material _cleanBrushMaterial;

	[SerializeField]
	private Material _dirtyBrushMaterial;

	[SerializeField]
	private Renderer _lightMeshRenderer;

	[SerializeField]
	private LoopingSoundPlayer DirtySoundPlayer;

	private bool _isDirty;

	private List<OrePiece> _polishingList = new List<OrePiece>();

	private TimeUntil _timeUntilClean;

	private void OnEnable()
	{
		MakeClean();
	}

	private void Update()
	{
		_polishingList.RemoveAll((OrePiece item) => item == null || !item.isActiveAndEnabled);
		foreach (OrePiece polishing in _polishingList)
		{
			if (polishing.MakesPolishingMachineDirty)
			{
				MakeDirty();
				break;
			}
			if (!_isDirty)
			{
				polishing.AddPolish(Time.deltaTime / PolishingTime);
			}
		}
		if (_isDirty)
		{
			PolishingAnimator.SetBool("IsPolishing", value: false);
			PolishingAnimator.SetBool("IsDirty", value: true);
			if (_timeUntilClean <= 0f)
			{
				MakeClean();
			}
		}
		else
		{
			PolishingAnimator.SetBool("IsPolishing", _polishingList.Count > 0);
		}
	}

	private void MakeDirty()
	{
		_timeUntilClean = _timeToStayDirty;
		if (_isDirty)
		{
			return;
		}
		_isDirty = true;
		foreach (Renderer item in _brushRenderersToMakeDirty)
		{
			Material[] sharedMaterials = item.sharedMaterials;
			sharedMaterials[0] = _dirtyBrushMaterial;
			sharedMaterials[1] = _dirtyMachineMaterial;
			item.sharedMaterials = sharedMaterials;
		}
		ChangeMachineMaterial(_dirtyMachineMaterial);
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.OrangeLightMaterial);
		_conveyor.ChangeSpeed(_dirtyConveyorSpeed);
		DirtySoundPlayer.Play();
	}

	private void MakeClean()
	{
		_isDirty = false;
		foreach (Renderer item in _brushRenderersToMakeDirty)
		{
			Material[] sharedMaterials = item.sharedMaterials;
			sharedMaterials[0] = _cleanBrushMaterial;
			sharedMaterials[1] = _cleanMachineMaterial;
			item.sharedMaterials = sharedMaterials;
		}
		ChangeMachineMaterial(_cleanMachineMaterial);
		ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
		_conveyor.ChangeSpeed(_standardConveyorSpeed);
		PolishingAnimator.SetBool("IsDirty", value: false);
		DirtySoundPlayer.Stop();
	}

	protected void ChangeMachineMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[0] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	protected void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[2] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			_polishingList.Add(componentInParent);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			_polishingList.Remove(componentInParent);
		}
	}
}
