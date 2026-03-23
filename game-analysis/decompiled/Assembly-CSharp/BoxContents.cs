using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x02000010 RID: 16
[Serializable]
public class BoxContents
{
	// Token: 0x06000080 RID: 128 RVA: 0x00003A53 File Offset: 0x00001C53
	public void AddOrePiece(OrePiece orePiece)
	{
		this.AddOrePiece(orePiece.ResourceType, orePiece.PieceType, orePiece.PolishedPercent >= 1f, 1);
	}

	// Token: 0x06000081 RID: 129 RVA: 0x00003A78 File Offset: 0x00001C78
	public void AddOrePiece(ResourceType resourceType, PieceType pieceType, bool isPolished, int count)
	{
		BoxContentEntry boxContentEntry = this.Contents.Find((BoxContentEntry e) => e.ResourceType == resourceType && e.PieceType == pieceType && e.IsPolished == isPolished);
		if (boxContentEntry != null)
		{
			boxContentEntry.Count += count;
			return;
		}
		this.Contents.Add(new BoxContentEntry
		{
			ResourceType = resourceType,
			PieceType = pieceType,
			IsPolished = isPolished,
			Count = count
		});
	}

	// Token: 0x06000082 RID: 130 RVA: 0x00003B08 File Offset: 0x00001D08
	public float GetBaseSellValue()
	{
		float num = 0f;
		foreach (BoxContentEntry boxContentEntry in this.Contents)
		{
			num += Singleton<OreManager>.Instance.GetDefaultSellValue(boxContentEntry.ResourceType, boxContentEntry.PieceType, boxContentEntry.IsPolished) * (float)boxContentEntry.Count;
		}
		return num;
	}

	// Token: 0x06000083 RID: 131 RVA: 0x00003B84 File Offset: 0x00001D84
	public float GetTotalSellValue()
	{
		return this.GetBaseSellValue() * 1.05f;
	}

	// Token: 0x06000084 RID: 132 RVA: 0x00003B94 File Offset: 0x00001D94
	public string GetManifestText()
	{
		float num = this.GetCurrentVolume() / this.MaxVolume * 100f;
		num = Mathf.Clamp(num, 0f, 100f);
		string text = string.Format("Box Contents ({0:0.##}%):\n", num);
		foreach (BoxContentEntry boxContentEntry in this.Contents)
		{
			text += string.Format("{0}x {1}\n", boxContentEntry.Count, Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(boxContentEntry.ResourceType, boxContentEntry.PieceType, boxContentEntry.IsPolished));
		}
		float baseSellValue = this.GetBaseSellValue();
		float num2 = baseSellValue * 1.05f;
		text += string.Format("\nBase Value: ${0:0.00}", baseSellValue);
		text += string.Format("\nPackaged Value: ${0:0.00}", num2);
		return text;
	}

	// Token: 0x06000085 RID: 133 RVA: 0x00003C94 File Offset: 0x00001E94
	public float GetCurrentVolume()
	{
		float num = 0f;
		foreach (BoxContentEntry boxContentEntry in this.Contents)
		{
			num += Singleton<OreManager>.Instance.GetVolumeInBox(boxContentEntry.ResourceType, boxContentEntry.PieceType, boxContentEntry.IsPolished) * (float)boxContentEntry.Count;
		}
		return num;
	}

	// Token: 0x04000070 RID: 112
	public float MaxVolume = 1f;

	// Token: 0x04000071 RID: 113
	public List<BoxContentEntry> Contents = new List<BoxContentEntry>();

	// Token: 0x04000072 RID: 114
	public const float SellPriceMultiplier = 1.05f;
}
