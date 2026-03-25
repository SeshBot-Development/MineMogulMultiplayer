using System;
using UnityEngine;

public static class MathExtensions
{
	public static Vector3 RoundVector3(Vector3 v, int decimals = 2)
	{
		return new Vector3((float)Math.Round(v.x, decimals), (float)Math.Round(v.y, decimals), (float)Math.Round(v.z, decimals));
	}

	public static Vector3 TruncateVector3(Vector3 position)
	{
		return new Vector3(Truncate(position.x), Truncate(position.y), Truncate(position.z));
		static float Truncate(float value)
		{
			return Mathf.Floor(value * 100f) / 100f;
		}
	}
}
