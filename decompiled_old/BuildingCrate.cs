using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingCrate : BaseSellableItem, IInteractable, ISaveLoadableObject
{
	public SavableObjectID SavableObjectID;

	public int Quantity = 1;

	public BuildingInventoryDefinition Definition;

	[SerializeField]
	private List<Interaction> _interactions;

	public bool HasBeenSaved { get; set; }

	private void Start()
	{
		if (Definition != null && Definition.GetIcon() != null)
		{
			Image[] componentsInChildren = GetComponentsInChildren<Image>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].sprite = Definition.GetIcon();
			}
			if (Quantity > 1)
			{
				TMP_Text[] componentsInChildren2 = GetComponentsInChildren<TMP_Text>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].text = "x" + Quantity;
				}
			}
			else
			{
				TMP_Text[] componentsInChildren2 = GetComponentsInChildren<TMP_Text>();
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					componentsInChildren2[i].enabled = false;
				}
			}
		}
		else
		{
			Canvas[] componentsInChildren3 = GetComponentsInChildren<Canvas>();
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				Object.Destroy(componentsInChildren3[i].gameObject);
			}
		}
	}

	public void TryAddToInventory()
	{
		if (Definition == null)
		{
			Debug.LogWarning("Tried to pickup Crate with missing Building Definition!");
			return;
		}
		ToolBuilder toolBuilder = Object.Instantiate(Singleton<BuildingManager>.Instance.BuildingToolPrefab);
		toolBuilder.Definition = Definition;
		toolBuilder.Quantity = Quantity;
		toolBuilder.Setup();
		if (Object.FindObjectOfType<PlayerInventory>().TryAddToInventory(toolBuilder))
		{
			Object.Destroy(base.gameObject);
		}
	}

	public override float GetSellValue()
	{
		return Singleton<EconomyManager>.Instance.GetPriceOfBuildingDefinition(Definition) * 0.9f * (float)Quantity;
	}

	public bool ShouldUseInteractionWheel()
	{
		return false;
	}

	public List<Interaction> GetInteractions()
	{
		return _interactions;
	}

	public string GetObjectName()
	{
		return Definition.Name;
	}

	public void Interact(Interaction selectedInteraction)
	{
		string text = selectedInteraction.Name;
		if (!(text == "Take"))
		{
			if (text == "Destroy")
			{
				Object.Destroy(base.gameObject);
			}
		}
		else
		{
			TryAddToInventory();
		}
	}

	public bool ShouldBeSaved()
	{
		return true;
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	public SavableObjectID GetSavableObjectID()
	{
		return SavableObjectID;
	}

	public virtual void LoadFromSave(string json)
	{
		BuildingCrateSaveData buildingCrateSaveData = JsonUtility.FromJson<BuildingCrateSaveData>(json);
		if (buildingCrateSaveData == null)
		{
			buildingCrateSaveData = new BuildingCrateSaveData();
		}
		Definition = Singleton<SavingLoadingManager>.Instance.GetBuildingInventoryDefinition(buildingCrateSaveData.BuildObjectID);
		Quantity = buildingCrateSaveData.Quantity;
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(new BuildingCrateSaveData
		{
			Quantity = Quantity,
			BuildObjectID = Definition.GetMainPrefab().SavableObjectID
		});
	}
}
