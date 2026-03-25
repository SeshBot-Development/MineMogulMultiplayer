using UnityEngine;

public class SellerMachine : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("MarkedForDestruction") || other.attachedRigidbody == null)
		{
			return;
		}
		OrePiece componentInParent = other.GetComponentInParent<OrePiece>();
		if (componentInParent != null)
		{
			componentInParent.SellAfterDelay();
			return;
		}
		BoxObject componentInParent2 = other.GetComponentInParent<BoxObject>();
		if (componentInParent2 != null)
		{
			SellBox(componentInParent2);
			return;
		}
		BaseSellableItem componentInParent3 = other.GetComponentInParent<BaseSellableItem>();
		if (componentInParent3 != null)
		{
			componentInParent3.SellItem();
		}
	}

	private void SellBox(BoxObject box)
	{
		Singleton<EconomyManager>.Instance.AddMoney(box.GetSellValue());
		foreach (BoxContentEntry content in box.BoxContents.Contents)
		{
			Singleton<QuestManager>.Instance?.OnResourceDeposited(content.ResourceType, content.PieceType, content.IsPolished ? 1 : 0, content.Count);
			Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
		}
		box.Delete();
	}
}
