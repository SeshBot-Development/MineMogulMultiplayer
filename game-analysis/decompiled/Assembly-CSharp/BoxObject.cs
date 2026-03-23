using System;
using TMPro;
using UnityEngine;

// Token: 0x0200000F RID: 15
public class BoxObject : BaseSellableItem, ISaveLoadableObject
{
	// Token: 0x06000073 RID: 115 RVA: 0x00003983 File Offset: 0x00001B83
	public void Initialize(BoxContents boxContents)
	{
		this.BoxContents = boxContents;
		this.UpdateManifestText();
	}

	// Token: 0x06000074 RID: 116 RVA: 0x00003992 File Offset: 0x00001B92
	public void UpdateManifestText()
	{
		this._manifestText.text = this.BoxContents.GetManifestText();
	}

	// Token: 0x06000075 RID: 117 RVA: 0x000039AA File Offset: 0x00001BAA
	public override float GetSellValue()
	{
		return this.BoxContents.GetTotalSellValue();
	}

	// Token: 0x06000076 RID: 118 RVA: 0x000039B7 File Offset: 0x00001BB7
	public void Delete()
	{
		Object.Destroy(base.gameObject);
	}

	// Token: 0x06000077 RID: 119 RVA: 0x000039C4 File Offset: 0x00001BC4
	public bool ShouldBeSaved()
	{
		return true;
	}

	// Token: 0x06000078 RID: 120 RVA: 0x000039C7 File Offset: 0x00001BC7
	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	// Token: 0x06000079 RID: 121 RVA: 0x000039D4 File Offset: 0x00001BD4
	public Vector3 GetRotation()
	{
		return base.transform.rotation.eulerAngles;
	}

	// Token: 0x0600007A RID: 122 RVA: 0x000039F4 File Offset: 0x00001BF4
	public SavableObjectID GetSavableObjectID()
	{
		return this.SavableObjectID;
	}

	// Token: 0x1700000A RID: 10
	// (get) Token: 0x0600007B RID: 123 RVA: 0x000039FC File Offset: 0x00001BFC
	// (set) Token: 0x0600007C RID: 124 RVA: 0x00003A04 File Offset: 0x00001C04
	public bool HasBeenSaved { get; set; }

	// Token: 0x0600007D RID: 125 RVA: 0x00003A10 File Offset: 0x00001C10
	public virtual void LoadFromSave(string json)
	{
		BoxContents boxContents = JsonUtility.FromJson<BoxContents>(json);
		if (boxContents == null)
		{
			Debug.Log("Failed to load BoxObject's BoxContent data from save!");
			boxContents = new BoxContents();
		}
		this.Initialize(boxContents);
	}

	// Token: 0x0600007E RID: 126 RVA: 0x00003A3E File Offset: 0x00001C3E
	public virtual string GetCustomSaveData()
	{
		return JsonUtility.ToJson(this.BoxContents);
	}

	// Token: 0x0400006C RID: 108
	public SavableObjectID SavableObjectID;

	// Token: 0x0400006D RID: 109
	public BoxContents BoxContents;

	// Token: 0x0400006E RID: 110
	[SerializeField]
	private TMP_Text _manifestText;
}
