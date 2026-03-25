using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoxContents
{
	public float MaxVolume = 1f;

	public List<BoxContentEntry> Contents = new List<BoxContentEntry>();

	public const float SellPriceMultiplier = 1.05f;

	public void AddOrePiece(OrePiece orePiece)
	{
		AddOrePiece(orePiece.ResourceType, orePiece.PieceType, orePiece.PolishedPercent >= 1f, 1);
	}

	public void AddOrePiece(ResourceType resourceType, PieceType pieceType, bool isPolished, int count)
	{
		BoxContentEntry boxContentEntry = Contents.Find((BoxContentEntry e) => e.ResourceType == resourceType && e.PieceType == pieceType && e.IsPolished == isPolished);
		if (boxContentEntry != null)
		{
			boxContentEntry.Count += count;
			return;
		}
		Contents.Add(new BoxContentEntry
		{
			ResourceType = resourceType,
			PieceType = pieceType,
			IsPolished = isPolished,
			Count = count
		});
	}

	public float GetBaseSellValue()
	{
		float num = 0f;
		foreach (BoxContentEntry content in Contents)
		{
			num += Singleton<OreManager>.Instance.GetDefaultSellValue(content.ResourceType, content.PieceType, content.IsPolished) * (float)content.Count;
		}
		return num;
	}

	public float GetTotalSellValue()
	{
		return GetBaseSellValue() * 1.05f;
	}

	public string GetManifestText()
	{
		float value = GetCurrentVolume() / MaxVolume * 100f;
		value = Mathf.Clamp(value, 0f, 100f);
		string text = $"Box Contents ({value:0.##}%):\n";
		foreach (BoxContentEntry content in Contents)
		{
			text += $"{content.Count}x {Singleton<OreManager>.Instance.GetColoredFormattedResourcePieceString(content.ResourceType, content.PieceType, content.IsPolished)}\n";
		}
		float baseSellValue = GetBaseSellValue();
		float num = baseSellValue * 1.05f;
		text += $"\nBase Value: ${baseSellValue:0.00}";
		return text + $"\nPackaged Value: ${num:0.00}";
	}

	public float GetCurrentVolume()
	{
		float num = 0f;
		foreach (BoxContentEntry content in Contents)
		{
			num += Singleton<OreManager>.Instance.GetVolumeInBox(content.ResourceType, content.PieceType, content.IsPolished) * (float)content.Count;
		}
		return num;
	}
}
