using UnityEngine;

public interface ISaveLoadableStaticBreakable
{
	Vector3 GetPosition();

	void MarkStaticPositionAsBroken();

	void DestroyFromLoading();
}
