using System;
using UnityEngine;

// Token: 0x02000022 RID: 34
public class CastingFurnaceCoalInput : MonoBehaviour
{
	// Token: 0x0600010F RID: 271 RVA: 0x00006D60 File Offset: 0x00004F60
	private void OnTriggerEnter(Collider other)
	{
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			if (componentInParent.ResourceType == ResourceType.Coal)
			{
				if (componentInParent.PieceType == PieceType.Ore)
				{
					this.ParentFurnace.AddCoal(2);
				}
				else if (componentInParent.PieceType == PieceType.OreCluster)
				{
					this.ParentFurnace.AddCoal(4);
				}
				else
				{
					this.ParentFurnace.AddCoal(1);
				}
			}
			else if (componentInParent.ResourceType == ResourceType.Slag)
			{
				this.ParentFurnace.AddCoal(3);
			}
			componentInParent.Delete();
		}
	}

	// Token: 0x0400010A RID: 266
	public CastingFurnace ParentFurnace;
}
