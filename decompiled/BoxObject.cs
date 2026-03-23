using TMPro;
using UnityEngine;

public class BoxObject : BaseSellableItem, ISaveLoadableObject
{
	public SavableObjectID SavableObjectID;

	public BoxContents BoxContents;

	[SerializeField]
	private TMP_Text _manifestText;

	public bool HasBeenSaved { get; set; }

	public void Initialize(BoxContents boxContents)
	{
		BoxContents = boxContents;
		UpdateManifestText();
	}

	public void UpdateManifestText()
	{
		_manifestText.text = BoxContents.GetManifestText();
	}

	public override float GetSellValue()
	{
		return BoxContents.GetTotalSellValue();
	}

	public void Delete()
	{
		Object.Destroy(base.gameObject);
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
		BoxContents boxContents = JsonUtility.FromJson<BoxContents>(json);
		if (boxContents == null)
		{
			Debug.Log("Failed to load BoxObject's BoxContent data from save!");
			boxContents = new BoxContents();
		}
		Initialize(boxContents);
	}

	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(BoxContents);
	}
}
