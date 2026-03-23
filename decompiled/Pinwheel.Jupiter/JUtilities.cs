using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter;

public static class JUtilities
{
	public static float DELTA_TIME = 0.0167f;

	public static string ListElementsToString<T>(this IEnumerable<T> list, string separator)
	{
		IEnumerator<T> enumerator = list.GetEnumerator();
		StringBuilder stringBuilder = new StringBuilder();
		if (enumerator.MoveNext())
		{
			stringBuilder.Append(enumerator.Current.ToString());
		}
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(separator).Append(enumerator.Current.ToString());
		}
		return stringBuilder.ToString();
	}

	public static void Fill<T>(T[] array, T value)
	{
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = value;
		}
	}

	public static void CopyTo<T>(T[] src, T[] des)
	{
		int num = Mathf.Min(src.Length, des.Length);
		for (int i = 0; i < num; i++)
		{
			des[i] = src[i];
		}
	}

	public static int To1DIndex(int x, int z, int width)
	{
		return z * width + x;
	}

	public static bool IsInRange(float number, float minValue, float maxValue)
	{
		if (number >= minValue)
		{
			return number <= maxValue;
		}
		return false;
	}

	public static bool IsInRangeExclusive(float number, float minValue, float maxValue)
	{
		if (number > minValue)
		{
			return number < maxValue;
		}
		return false;
	}

	public static float GetFraction(float value, float floor, float ceil)
	{
		return (value - floor) / (ceil - floor);
	}

	public static void ClearChildren(Transform t)
	{
		for (int num = t.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(t.GetChild(num).gameObject);
		}
	}

	public static Gradient CreateFullWhiteGradient()
	{
		Gradient gradient = new Gradient();
		GradientColorKey gradientColorKey = new GradientColorKey(Color.white, 1f);
		GradientAlphaKey gradientAlphaKey = new GradientAlphaKey(1f, 1f);
		gradient.SetKeys(new GradientColorKey[1] { gradientColorKey }, new GradientAlphaKey[1] { gradientAlphaKey });
		return gradient;
	}

	public static Gradient CreateFullTransparentGradient()
	{
		Gradient gradient = new Gradient();
		GradientColorKey gradientColorKey = new GradientColorKey(Color.white, 1f);
		GradientAlphaKey gradientAlphaKey = new GradientAlphaKey(0f, 1f);
		gradient.SetKeys(new GradientColorKey[1] { gradientColorKey }, new GradientAlphaKey[1] { gradientAlphaKey });
		return gradient;
	}

	public static void CalculateBarycentricCoord(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector3 bary)
	{
		Vector2 vector = p2 - p1;
		Vector2 vector2 = p3 - p1;
		Vector2 lhs = p - p1;
		float num = Vector2.Dot(vector, vector);
		float num2 = Vector2.Dot(vector, vector2);
		float num3 = Vector2.Dot(vector2, vector2);
		float num4 = Vector2.Dot(lhs, vector);
		float num5 = Vector2.Dot(lhs, vector2);
		float num6 = 1f / (num * num3 - num2 * num2);
		bary.y = (num3 * num4 - num2 * num5) * num6;
		bary.z = (num * num5 - num2 * num4) * num6;
		bary.x = 1f - bary.y - bary.z;
	}

	public static void ExpandTrisUvCoord(Texture2D tex, Vector2[] trisUv)
	{
		Vector2 texelSize = tex.texelSize;
		Vector2 vector = (trisUv[0] + trisUv[1] + trisUv[2]) / 3f;
		trisUv[0] += (trisUv[0] - vector).normalized * texelSize.magnitude;
		trisUv[0].x = Mathf.Clamp01(trisUv[0].x);
		trisUv[0].y = Mathf.Clamp01(trisUv[0].y);
		trisUv[1] += (trisUv[1] - vector).normalized * texelSize.magnitude;
		trisUv[1].x = Mathf.Clamp01(trisUv[1].x);
		trisUv[1].y = Mathf.Clamp01(trisUv[1].y);
		trisUv[2] += (trisUv[2] - vector).normalized * texelSize.magnitude;
		trisUv[2].x = Mathf.Clamp01(trisUv[2].x);
		trisUv[2].y = Mathf.Clamp01(trisUv[2].y);
	}

	public static bool ContainIdenticalElements<T>(T[] arr1, T[] arr2)
	{
		if (arr1 == null && arr2 == null)
		{
			return true;
		}
		if (arr1 == null && arr2 != null)
		{
			return false;
		}
		if (arr1 != null && arr2 == null)
		{
			return false;
		}
		if (arr1.Length != arr2.Length)
		{
			return false;
		}
		for (int i = 0; i < arr1.Length; i++)
		{
			ref readonly T reference = ref arr1[i];
			object obj = arr2[i];
			if (!reference.Equals(obj))
			{
				return false;
			}
		}
		return true;
	}

	public static float GetNearestMultiple(float number, float multipleOf)
	{
		int i;
		for (i = 0; multipleOf * (float)i < number; i++)
		{
		}
		float num = multipleOf * (float)(i - 1);
		float num2 = multipleOf * (float)i;
		float num3 = number - num;
		float num4 = num2 - number;
		if (num3 < num4)
		{
			return num;
		}
		return num2;
	}

	public static Transform GetChildrenWithName(Transform parent, string name)
	{
		Transform transform = parent.Find(name);
		if (transform == null)
		{
			GameObject gameObject = new GameObject(name);
			gameObject.transform.parent = parent;
			ResetTransform(gameObject.transform, parent);
			transform = gameObject.transform;
		}
		return transform;
	}

	public static void ResetTransform(Transform t, Transform parent)
	{
		t.parent = parent;
		t.localPosition = Vector3.zero;
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.one;
	}

	public static void DestroyGameobject(GameObject g)
	{
		UnityEngine.Object.Destroy(g);
	}

	public static void DestroyGameObjectWithUndo(GameObject g)
	{
		DestroyGameobject(g);
	}

	public static void DestroyObject(UnityEngine.Object o)
	{
		UnityEngine.Object.Destroy(o);
	}

	public static string Repeat(char src, int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(src, count);
		return stringBuilder.ToString();
	}

	public static void MarkCurrentSceneDirty()
	{
	}

	public static GameObject[] GetChildrenGameObjects(Transform parent)
	{
		GameObject[] array = new GameObject[parent.childCount];
		for (int i = 0; i < parent.childCount; i++)
		{
			array[i] = parent.GetChild(i).gameObject;
		}
		return array;
	}

	public static Transform[] GetChildrenTransforms(Transform parent)
	{
		Transform[] array = new Transform[parent.childCount];
		for (int i = 0; i < parent.childCount; i++)
		{
			array[i] = parent.GetChild(i);
		}
		return array;
	}

	public static Vector3 RotateVectorAroundNormal(Vector3 vector, Vector3 normal, float angleDegree)
	{
		float num = Mathf.Sin(angleDegree * (MathF.PI / 180f));
		float num2 = Mathf.Cos(angleDegree * (MathF.PI / 180f));
		return vector * num2 + Vector3.Cross(normal, vector) * num + normal * Vector3.Dot(normal, vector) * (1f - num2);
	}

	public static Mesh GetPrimitiveMesh(PrimitiveType type)
	{
		GameObject gameObject = GameObject.CreatePrimitive(type);
		Mesh sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		DestroyGameobject(gameObject);
		return sharedMesh;
	}

	public static void ShuffleList<T>(List<T> list)
	{
		int count = list.Count;
		for (int i = 0; i < count - 1; i++)
		{
			int index = UnityEngine.Random.Range(0, count);
			T value = list[i];
			list[i] = list[index];
			list[index] = value;
		}
	}

	public static int[] GetShuffleIndicesArray(int length)
	{
		int[] indicesArray = GetIndicesArray(length);
		for (int i = 0; i < length - 1; i++)
		{
			int num = UnityEngine.Random.Range(0, length);
			int num2 = indicesArray[i];
			indicesArray[i] = indicesArray[num];
			indicesArray[num] = num2;
		}
		return indicesArray;
	}

	public static int[] GetIndicesArray(int length)
	{
		int[] array = new int[length];
		for (int i = 0; i < length; i++)
		{
			array[i] = i;
		}
		return array;
	}

	public static void ResetArray<T>(ref T[] array, int count, T defaultValue)
	{
		if (array != null && array.Length == count)
		{
			Fill(array, defaultValue);
		}
		else
		{
			array = new T[count];
		}
	}

	public static bool EnsureArrayLength<T>(ref T[] array, int count)
	{
		if (array == null || array.Length != count)
		{
			array = new T[count];
			return false;
		}
		return true;
	}

	public static float GetValueBilinear(float[] data, int width, int height, Vector2 uv)
	{
		Vector2 vector = new Vector2(Mathf.Lerp(0f, width - 1, uv.x), Mathf.Lerp(0f, height - 1, uv.y));
		int num = Mathf.FloorToInt(vector.x);
		int x = Mathf.CeilToInt(vector.x);
		int num2 = Mathf.FloorToInt(vector.y);
		int z = Mathf.CeilToInt(vector.y);
		float num3 = data[To1DIndex(num, num2, width)];
		float num4 = data[To1DIndex(num, z, width)];
		float num5 = data[To1DIndex(x, num2, width)];
		float num6 = data[To1DIndex(x, z, width)];
		Vector2 vector2 = new Vector2(vector.x - (float)num, vector.y - (float)num2);
		return num3 * (1f - vector2.x) * (1f - vector2.y) + num4 * (1f - vector2.x) * vector2.y + num5 * vector2.x * (1f - vector2.y) + num6 * vector2.x * vector2.y;
	}

	public static Color GetColorBilinear(Color[] textureData, int width, int height, Vector2 uv)
	{
		_ = Color.clear;
		Vector2 vector = new Vector2(Mathf.Lerp(0f, width - 1, uv.x), Mathf.Lerp(0f, height - 1, uv.y));
		int num = Mathf.FloorToInt(vector.x);
		int x = Mathf.CeilToInt(vector.x);
		int num2 = Mathf.FloorToInt(vector.y);
		int z = Mathf.CeilToInt(vector.y);
		Color color = textureData[To1DIndex(num, num2, width)];
		Color color2 = textureData[To1DIndex(num, z, width)];
		Color color3 = textureData[To1DIndex(x, num2, width)];
		Color color4 = textureData[To1DIndex(x, z, width)];
		Vector2 vector2 = new Vector2(vector.x - (float)num, vector.y - (float)num2);
		return color * (1f - vector2.x) * (1f - vector2.y) + color2 * (1f - vector2.x) * vector2.y + color3 * vector2.x * (1f - vector2.y) + color4 * vector2.x * vector2.y;
	}

	public static GameObject CreatePreviewGameObject(Mesh m, Material mat, Vector3 position)
	{
		GameObject gameObject = new GameObject("GO");
		gameObject.transform.position = position;
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		gameObject.AddComponent<MeshFilter>().sharedMesh = m;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		meshRenderer.sharedMaterial = mat;
		return gameObject;
	}

	public static Camera CreatePreviewCamera(GameObject target, float distance, float padding)
	{
		GameObject gameObject = new GameObject("CAM");
		gameObject.transform.rotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		Camera camera = gameObject.AddComponent<Camera>();
		camera.orthographic = true;
		camera.clearFlags = CameraClearFlags.Nothing;
		camera.aspect = 1f;
		Bounds bounds = target.GetComponent<MeshRenderer>().bounds;
		camera.transform.position = bounds.center + new Vector3(-1f, 0.5f, -1f) * distance;
		camera.transform.LookAt(bounds.center);
		camera.orthographicSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 2f + padding;
		return camera;
	}

	public static void EnsureDirectoryExists(string dir)
	{
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}
	}

	public static void SetStaticRecursively(GameObject g, bool isStatic)
	{
	}

	public static void EnsureLengthSufficient<T>(List<T> list, int preferredLength) where T : UnityEngine.Object
	{
		if (list == null)
		{
			list = new List<T>();
		}
		while (list.Count < preferredLength)
		{
			list.Add(null);
		}
	}

	public static void EnsureLengthSufficient(List<string> list, int preferredLength)
	{
		if (list == null)
		{
			list = new List<string>();
		}
		while (list.Count < preferredLength)
		{
			list.Add(string.Empty);
		}
	}

	public static void EnsureLengthSufficient(List<bool> list, int preferredLength)
	{
		if (list == null)
		{
			list = new List<bool>();
		}
		while (list.Count < preferredLength)
		{
			list.Add(item: false);
		}
	}

	public static string Ellipsis(string s, int length)
	{
		if (s.Length < length)
		{
			return s;
		}
		return s.Substring(0, length) + "...";
	}

	public static bool IsRectContainPointExclusive(Rect r, Vector2 p)
	{
		if (p.x > r.min.x && p.x < r.max.x && p.y > r.min.y)
		{
			return p.y < r.max.y;
		}
		return false;
	}

	public static Color GetColor(Color baseColor, float alpha)
	{
		return new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
	}

	public static Rect GetRectContainsPoints(params Vector2[] points)
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		for (int i = 0; i < points.Length; i++)
		{
			num = ((points[i].x < num) ? points[i].x : num);
			num2 = ((points[i].x > num2) ? points[i].x : num2);
			num3 = ((points[i].y < num3) ? points[i].y : num3);
			num4 = ((points[i].y > num4) ? points[i].y : num4);
		}
		return Rect.MinMaxRect(num, num3, num2, num4);
	}

	public static float InverseLerpUnclamped(float a, float b, float value)
	{
		if (a != b)
		{
			return (value - a) / (b - a);
		}
		return 0f;
	}

	public static Vector2 PointToNormalizedUnclampled(Rect r, Vector2 point)
	{
		return new Vector2(InverseLerpUnclamped(r.x, r.xMax, point.x), InverseLerpUnclamped(r.y, r.yMax, point.y));
	}

	public static Rect GetUvRect(Vector2 v0, Vector2 v1, Vector2 v2)
	{
		return Rect.MinMaxRect(Mathf.Min(v0.x, v1.x, v2.x), Mathf.Min(v0.y, v1.y, v2.y), Mathf.Max(v0.x, v1.x, v2.x), Mathf.Max(v0.y, v1.y, v2.y));
	}

	public static Gradient Clone(Gradient src)
	{
		if (src == null)
		{
			return null;
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(src.colorKeys, src.alphaKeys);
		return gradient;
	}

	public static AnimationCurve Clone(AnimationCurve src)
	{
		if (src == null)
		{
			return null;
		}
		AnimationCurve animationCurve = new AnimationCurve();
		Keyframe[] keys = src.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			animationCurve.AddKey(keys[i]);
		}
		animationCurve.preWrapMode = src.preWrapMode;
		animationCurve.postWrapMode = src.postWrapMode;
		return animationCurve;
	}

	public static bool IsPointInsideQuadXZ(Vector3 point, Vector3[] quad)
	{
		Vector3 bary = Vector3.zero;
		CalculateBarycentricCoord(new Vector2(point.x, point.z), new Vector2(quad[0].x, quad[0].z), new Vector2(quad[1].x, quad[1].z), new Vector2(quad[2].x, quad[2].z), ref bary);
		if (bary.x >= 0f && bary.y >= 0f && bary.z >= 0f)
		{
			return true;
		}
		CalculateBarycentricCoord(new Vector2(point.x, point.z), new Vector2(quad[0].x, quad[0].z), new Vector2(quad[2].x, quad[2].z), new Vector2(quad[3].x, quad[3].z), ref bary);
		if (bary.x >= 0f && bary.y >= 0f && bary.z >= 0f)
		{
			return true;
		}
		return false;
	}

	public static void DestroyMeshArray(Mesh[] meshes)
	{
		for (int i = 0; i < meshes.Length; i++)
		{
			if (meshes[i] != null)
			{
				UnityEngine.Object.DestroyImmediate(meshes[i], allowDestroyingAssets: true);
			}
		}
	}

	public static Vector2 FlipY(Vector2 v)
	{
		return new Vector2(v.x, 1f - v.y);
	}

	public static bool IsPointInCircumcircle(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 p)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetRow(0, new Vector4(v0.x, v0.y, v0.x * v0.x + v0.y * v0.y, 1f));
		matrix4x.SetRow(1, new Vector4(v2.x, v2.y, v2.x * v2.x + v2.y * v2.y, 1f));
		matrix4x.SetRow(2, new Vector4(v1.x, v1.y, v1.x * v1.x + v1.y * v1.y, 1f));
		matrix4x.SetRow(3, new Vector4(p.x, p.y, p.x * p.x + p.y * p.y, 1f));
		return matrix4x.determinant > 0f;
	}

	public static bool IsPointInCircumcircleXZ(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 p)
	{
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.SetRow(0, new Vector4(v0.x, v0.z, v0.x * v0.x + v0.z * v0.z, 1f));
		matrix4x.SetRow(1, new Vector4(v2.x, v2.z, v2.x * v2.x + v2.z * v2.z, 1f));
		matrix4x.SetRow(2, new Vector4(v1.x, v1.z, v1.x * v1.x + v1.z * v1.z, 1f));
		matrix4x.SetRow(3, new Vector4(p.x, p.z, p.x * p.x + p.z * p.z, 1f));
		return matrix4x.determinant > 0f;
	}

	public static bool AreSetEqual(ushort[] setA, ushort[] setB)
	{
		HashSet<ushort> hashSet = new HashSet<ushort>(setA);
		HashSet<ushort> hashSet2 = new HashSet<ushort>(setB);
		return hashSet.SetEquals(hashSet2);
	}

	public static void Distinct<T>(this List<T> list)
	{
		list.Distinct();
	}

	public static void AddIfNotContains<T>(this IList<T> list, IEnumerable<T> items)
	{
		IEnumerator<T> enumerator = items.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (!list.Contains(current))
			{
				list.Add(current);
			}
		}
	}

	public static void AddIfNotContains<T>(this IList<T> list, T item)
	{
		if (!list.Contains(item))
		{
			list.Add(item);
		}
	}

	public static Vector3 ToX0Y(this Vector2 v)
	{
		return new Vector3(v.x, 0f, v.y);
	}
}
