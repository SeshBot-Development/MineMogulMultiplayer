using UnityEngine;

public class CastingFurnaceCoalInput : MonoBehaviour
{
	public CastingFurnace ParentFurnace;

	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (!(componentInParent != null))
		{
			return;
		}
		if (componentInParent.ResourceType == ResourceType.Coal)
		{
			if (componentInParent.PieceType == PieceType.Ore)
			{
				ParentFurnace.AddCoal(2);
			}
			else if (componentInParent.PieceType == PieceType.OreCluster)
			{
				ParentFurnace.AddCoal(4);
			}
			else
			{
				ParentFurnace.AddCoal(1);
			}
		}
		else if (componentInParent.ResourceType == ResourceType.Slag)
		{
			ParentFurnace.AddCoal(3);
		}
		componentInParent.Delete();
	}
}
