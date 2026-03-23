using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

// Token: 0x02000074 RID: 116
public class OreManager : global::Singleton<OreManager>
{
	// Token: 0x06000313 RID: 787 RVA: 0x0000F6FC File Offset: 0x0000D8FC
	public ResourceDescription GetResourceDefinition(ResourceType resourceType)
	{
		return this._allResourceDescriptions.FirstOrDefault((ResourceDescription r) => r.ResourceType == resourceType);
	}

	// Token: 0x06000314 RID: 788 RVA: 0x0000F730 File Offset: 0x0000D930
	public string GetColoredResourceTypeString(ResourceType resourceType)
	{
		string text = this.GetResourceColor(resourceType).ToHexString();
		return string.Format("<color=#{0}>{1}</color>", text, resourceType);
	}

	// Token: 0x06000315 RID: 789 RVA: 0x0000F75B File Offset: 0x0000D95B
	public Color GetResourceColor(ResourceType resourceType)
	{
		return this.GetResourceDefinition(resourceType).DisplayColor;
	}

	// Token: 0x06000316 RID: 790 RVA: 0x0000F76C File Offset: 0x0000D96C
	public string GetColoredFormattedResourcePieceString(ResourceType resourceType, PieceType pieceType, bool requirePolished = false)
	{
		string text = string.Format("<color=red>INVALID FORMAT: {0} {1}</color>", resourceType, pieceType);
		string text2 = this.GetResourceColor(resourceType).ToHexString();
		string text3 = pieceType.ToString();
		string text4 = resourceType.ToString();
		if (pieceType == PieceType.DrillBit)
		{
			text3 = "Drill Bit";
		}
		if (pieceType == PieceType.ThreadedRod)
		{
			text3 = "Threaded Rod";
		}
		if (pieceType == PieceType.OreCluster)
		{
			text3 = "Ore Cluster";
		}
		if (pieceType == PieceType.JunkCast)
		{
			text3 = "Junk Cast";
		}
		if (pieceType == PieceType.Pipe && resourceType == ResourceType.Slag)
		{
			text4 = "Junk";
		}
		if (pieceType == PieceType.Crushed)
		{
			if (requirePolished)
			{
				text = string.Concat(new string[] { "<color=#", text2, ">Polished ", text3, " ", text4, "</color>" });
			}
			else
			{
				text = string.Concat(new string[] { "<color=#", text2, ">", text3, " ", text4, "</color>" });
			}
		}
		else if (requirePolished)
		{
			text = string.Concat(new string[] { "<color=#", text2, ">Polished ", text4, " ", text3, "</color>" });
		}
		else
		{
			text = string.Concat(new string[] { "<color=#", text2, ">", text4, " ", text3, "</color>" });
		}
		return text;
	}

	// Token: 0x06000317 RID: 791 RVA: 0x0000F8E8 File Offset: 0x0000DAE8
	public float GetDefaultSellValue(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		if (isPolished)
		{
			OrePiece orePiece = global::Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType && ore.PolishedPercent == 1f).FirstOrDefault<OrePiece>();
			if (orePiece != null)
			{
				return orePiece.GetSellValue();
			}
		}
		return global::Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType).FirstOrDefault<OrePiece>().GetSellValue();
	}

	// Token: 0x06000318 RID: 792 RVA: 0x0000F964 File Offset: 0x0000DB64
	public float GetVolumeInBox(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		return global::Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType).FirstOrDefault<OrePiece>().VolumeInsideBox;
	}

	// Token: 0x06000319 RID: 793 RVA: 0x0000F9AC File Offset: 0x0000DBAC
	private void Update()
	{
		List<OrePiece> allOrePieces = OrePiece.AllOrePieces;
		if (allOrePieces.Count == 0)
		{
			this._currentOreIndex = 0;
			return;
		}
		if (this._currentOreIndex >= allOrePieces.Count)
		{
			this._currentOreIndex = 0;
		}
		OrePiece orePiece = allOrePieces[this._currentOreIndex];
		if (orePiece == null)
		{
			OrePiece.AllOrePieces.Remove(orePiece);
		}
		else if (!Vector3Utils.IsValid(orePiece.transform.position) || orePiece.transform.position.y < -1000f)
		{
			orePiece.Delete();
		}
		this._currentOreIndex++;
	}

	// Token: 0x040002FC RID: 764
	[FormerlySerializedAs("ResourceDescriptions")]
	[SerializeField]
	private List<ResourceDescription> _allResourceDescriptions;

	// Token: 0x040002FD RID: 765
	private int _currentOreIndex;
}
