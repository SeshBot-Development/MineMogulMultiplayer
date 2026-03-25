using UnityEngine;

public static class Vector3Utils
{
	public static bool IsValid(Vector3 v)
	{
		if (float.IsFinite(v.x) && float.IsFinite(v.y))
		{
			return float.IsFinite(v.z);
		}
		return false;
	}
}
