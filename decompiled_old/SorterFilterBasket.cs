using System.Collections.Generic;
using UnityEngine;

public class SorterFilterBasket : BaseBasket
{
	private List<(ResourceType, PieceType)> _filterCriteria = new List<(ResourceType, PieceType)>();

	[SerializeField]
	private MeshRenderer _lightMeshRenderer;

	[SerializeField]
	private int _lightMaterialIndex;

	public SoundDefinition SetFilterSound;

	protected override void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			AddToFilter(componentInParent);
		}
	}

	protected override void OnTriggerExit(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			RemoveFromFilter(componentInParent);
		}
	}

	public bool OreMatchesFilter(OrePiece ore)
	{
		return _filterCriteria.Contains((ore.ResourceType, ore.PieceType));
	}

	public void AddToFilter(OrePiece ore)
	{
		if (!_basketOreList.Contains(ore))
		{
			_basketOreList.Add(ore);
			ore.BasketsThisIsInside.Add(this);
			UpdateFilter();
		}
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(SetFilterSound, base.transform.position, 1.4f);
	}

	public void RemoveFromFilter(OrePiece ore)
	{
		_basketOreList.Remove(ore);
		ore.BasketsThisIsInside.Remove(this);
		UpdateFilter();
		Singleton<SoundManager>.Instance.PlaySoundAtLocation(SetFilterSound, base.transform.position, 0.6f);
	}

	private void ChangeLightMaterial(Material material)
	{
		Material[] sharedMaterials = _lightMeshRenderer.sharedMaterials;
		sharedMaterials[_lightMaterialIndex] = material;
		_lightMeshRenderer.sharedMaterials = sharedMaterials;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_filterCriteria.Clear();
	}

	private void UpdateFilter()
	{
		_filterCriteria.Clear();
		foreach (OrePiece basketOre in _basketOreList)
		{
			(ResourceType, PieceType) item = (basketOre.ResourceType, basketOre.PieceType);
			if (!_filterCriteria.Contains(item))
			{
				_filterCriteria.Add(item);
			}
		}
		if (_filterCriteria.Count > 0)
		{
			ChangeLightMaterial(Singleton<BuildingManager>.Instance.GreenLightMaterial);
		}
		else
		{
			ChangeLightMaterial(Singleton<BuildingManager>.Instance.RedLightMaterial);
		}
	}
}
