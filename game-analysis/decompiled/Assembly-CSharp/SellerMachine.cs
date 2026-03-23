using System;
using UnityEngine;

// Token: 0x020000CE RID: 206
public class SellerMachine : MonoBehaviour
{
	// Token: 0x06000566 RID: 1382 RVA: 0x0001C93C File Offset: 0x0001AB3C
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("MarkedForDestruction"))
		{
			return;
		}
		if (other.attachedRigidbody == null)
		{
			return;
		}
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			componentInParent.SellAfterDelay(2f);
			return;
		}
		BoxObject componentInParent2 = other.GetComponentInParent<BoxObject>();
		if (componentInParent2 != null)
		{
			this.SellBox(componentInParent2);
			return;
		}
		BaseSellableItem componentInParent3 = other.GetComponentInParent<BaseSellableItem>();
		if (componentInParent3 != null)
		{
			Singleton<EconomyManager>.Instance.AddMoney(componentInParent3.GetSellValue());
			Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
			Object.Destroy(componentInParent3.gameObject);
		}
	}

	// Token: 0x06000567 RID: 1383 RVA: 0x0001C9D0 File Offset: 0x0001ABD0
	private void SellBox(BoxObject box)
	{
		Singleton<EconomyManager>.Instance.AddMoney(box.GetSellValue());
		foreach (BoxContentEntry boxContentEntry in box.BoxContents.Contents)
		{
			QuestManager instance = Singleton<QuestManager>.Instance;
			if (instance != null)
			{
				instance.OnResourceDeposited(boxContentEntry.ResourceType, boxContentEntry.PieceType, (float)(boxContentEntry.IsPolished ? 1 : 0), boxContentEntry.Count);
			}
			Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
		}
		box.Delete();
	}
}
