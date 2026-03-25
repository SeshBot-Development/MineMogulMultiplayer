using UnityEngine;

public class CastingFurnaceMoldArea : MonoBehaviour
{
	public Transform MoldToolEjectTransform;

	public Transform ProductEjectTransform;

	public Transform SecondaryProductEjectTransform;

	[HideInInspector]
	public CastingMoldType CastingMoldType;

	[SerializeField]
	private Renderer _moldRenderer;

	private CastingFurnace _owner;

	private int _slotNumber;

	public void Initialize(CastingFurnace owner, int slotNumber, CastingMoldType type)
	{
		_owner = owner;
		_slotNumber = slotNumber;
		CastingMoldType = type;
		UpdateRender();
	}

	public void UpdateRender()
	{
		if (CastingMoldType == CastingMoldType.None)
		{
			_moldRenderer.gameObject.SetActive(value: false);
			return;
		}
		CastingMoldRendererInfo castingMoldRendererInfo = _owner.GetCastingMoldRendererInfo(CastingMoldType);
		_moldRenderer.sharedMaterial = castingMoldRendererInfo.Material;
		_moldRenderer.GetComponent<MeshFilter>().mesh = castingMoldRendererInfo.Mesh;
		_moldRenderer.gameObject.SetActive(value: true);
	}

	public void InsertMoldFromLoading(CastingMoldType type)
	{
		CastingMoldType = type;
		UpdateRender();
	}

	public void InsertMold(CastingMoldType type)
	{
		EjectMold();
		CastingMoldType = type;
		UpdateRender();
		_owner.RecalculateMaterialAmountRequired();
	}

	public void EjectMold()
	{
		if (CastingMoldType == CastingMoldType.None)
		{
			return;
		}
		GameObject gameObject = Singleton<SavingLoadingManager>.Instance.AllSavableObjectPrefabs.Find(delegate(GameObject go)
		{
			if (!go)
			{
				return false;
			}
			ToolCastingMold component;
			return go.TryGetComponent<ToolCastingMold>(out component) && component.CastingMoldType == CastingMoldType;
		});
		if (gameObject != null)
		{
			Object.Instantiate(gameObject, MoldToolEjectTransform.position, MoldToolEjectTransform.transform.rotation);
		}
		else
		{
			Debug.LogError("Unable to spawn casting mold item, can not find prefab for type: " + CastingMoldType);
		}
		CastingMoldType = CastingMoldType.None;
		UpdateRender();
		_owner.RecalculateMaterialAmountRequired();
	}
}
