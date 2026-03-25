using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-10)]
public class OreManager : Singleton<OreManager>
{
	[FormerlySerializedAs("ResourceDescriptions")]
	[SerializeField]
	private List<ResourceDescription> _allResourceDescriptions;

	private int _currentOreIndex;

	public ResourceDescription GetResourceDefinition(ResourceType resourceType)
	{
		return _allResourceDescriptions.FirstOrDefault((ResourceDescription r) => r.ResourceType == resourceType);
	}

	public string GetColoredResourceTypeString(ResourceType resourceType)
	{
		string arg = GetResourceColor(resourceType).ToHexString();
		return $"<color=#{arg}>{resourceType}</color>";
	}

	public Color GetResourceColor(ResourceType resourceType)
	{
		return GetResourceDefinition(resourceType).DisplayColor;
	}

	public string GetColoredFormattedResourcePieceString(ResourceType resourceType, PieceType pieceType, bool requirePolished = false)
	{
		string text = $"<color=red>INVALID FORMAT: {resourceType} {pieceType}</color>";
		string text2 = GetResourceColor(resourceType).ToHexString();
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
				return "<color=#" + text2 + ">Polished " + text3 + " " + text4 + "</color>";
			}
			return "<color=#" + text2 + ">" + text3 + " " + text4 + "</color>";
		}
		if (requirePolished)
		{
			return "<color=#" + text2 + ">Polished " + text4 + " " + text3 + "</color>";
		}
		return "<color=#" + text2 + ">" + text4 + " " + text3 + "</color>";
	}

	public Sprite GetIconForOre(ResourceType resourceType, PieceType pieceType, bool isPolished = false)
	{
		OrePiece orePiece = Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Find((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType && ore.IsPolished == isPolished);
		if (orePiece != null)
		{
			return orePiece.GetIcon();
		}
		return null;
	}

	public float GetDefaultSellValue(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		if (isPolished)
		{
			OrePiece orePiece = Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType && ore.PolishedPercent == 1f).FirstOrDefault();
			if (orePiece != null)
			{
				return orePiece.GetSellValue();
			}
		}
		return Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType).FirstOrDefault().GetSellValue();
	}

	public float GetVolumeInBox(ResourceType resourceType, PieceType pieceType, bool isPolished)
	{
		return Singleton<SavingLoadingManager>.Instance.AllOrePiecePrefabs.Where((OrePiece ore) => ore.ResourceType == resourceType && ore.PieceType == pieceType).FirstOrDefault().VolumeInsideBox;
	}

	private void Update()
	{
		List<OrePiece> allOrePieces = OrePiece.AllOrePieces;
		if (allOrePieces.Count == 0)
		{
			_currentOreIndex = 0;
			return;
		}
		if (_currentOreIndex >= allOrePieces.Count)
		{
			_currentOreIndex = 0;
		}
		OrePiece orePiece = allOrePieces[_currentOreIndex];
		if (orePiece == null)
		{
			OrePiece.AllOrePieces.Remove(orePiece);
		}
		else if (!Vector3Utils.IsValid(orePiece.transform.position) || orePiece.transform.position.y < -1000f)
		{
			orePiece.Delete();
		}
		_currentOreIndex++;
	}
}
