public class BaseSellableItem : BasePhysicsObject
{
	public float BaseSellValue = 1f;

	public virtual float GetSellValue()
	{
		return BaseSellValue;
	}
}
