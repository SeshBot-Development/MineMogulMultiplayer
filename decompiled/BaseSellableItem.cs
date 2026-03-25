using UnityEngine;

public class BaseSellableItem : BasePhysicsObject
{
	public float BaseSellValue = 1f;

	public virtual float GetSellValue()
	{
		return BaseSellValue;
	}

	public virtual void SellItem()
	{
		Singleton<EconomyManager>.Instance.AddMoney(GetSellValue());
		Singleton<EconomyManager>.Instance.DispatchOnItemSoldEvent();
		Object.Destroy(base.gameObject);
	}
}
