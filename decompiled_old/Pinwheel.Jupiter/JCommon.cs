using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Pinwheel.Jupiter;

public static class JCommon
{
	public const string SUPPORT_EMAIL = "support@pinwheel.studio";

	public const string BUSINESS_EMAIL = "hello@pinwheel.studio";

	public const string YOUTUBE_CHANNEL = "https://www.youtube.com/channel/UCebwuk5CfIe5kolBI9nuBTg";

	public const string ONLINE_MANUAL = "https://docs.google.com/document/d/1Wf4CDlD96c6tna1ee0fpquWkdSrGDIO-eKuQDxjJedY/edit?usp=sharing";

	public const string FORUM = "https://forum.unity.com/threads/pre-released-jupiter-procedural-sky-builtin-lwrp-urp.799635/";

	public const string DISCORD = "https://discord.gg/HXNnFpS";

	public const string CONTACT_PAGE = "https://pinwheelstud.io/contact";

	public const string POLARIS_LINK = "https://assetstore.unity.com/packages/tools/terrain/polaris-3-low-poly-terrain-tool-286886?aid=1100l3QbW&pubref=jupiter-editor";

	public const string POSEIDON_LINK = "https://assetstore.unity.com/packages/vfx/shaders/low-poly-water-poseidon-153826?aid=1100l3QbW&pubref=jupiter-editor";

	public const int PREVIEW_TEXTURE_SIZE = 512;

	public const int TEXTURE_SIZE_MIN = 1;

	public const int TEXTURE_SIZE_MAX = 8192;

	private static Vector2[] fullRectUvPoints;

	private static Mesh emptyMesh;

	private static Material[] emptyMaterials;

	public static JRenderPipelineType CurrentRenderPipeline
	{
		get
		{
			string globalRenderPipeline = Shader.globalRenderPipeline;
			if (globalRenderPipeline.Equals("UniversalPipeline,LightweightPipeline"))
			{
				return JRenderPipelineType.Universal;
			}
			if (globalRenderPipeline.Equals("LightweightPipeline"))
			{
				return JRenderPipelineType.Lightweight;
			}
			return JRenderPipelineType.Builtin;
		}
	}

	public static Vector2[] FullRectUvPoints
	{
		get
		{
			if (fullRectUvPoints == null)
			{
				fullRectUvPoints = new Vector2[4]
				{
					Vector2.zero,
					Vector2.up,
					Vector2.one,
					Vector2.right
				};
			}
			return fullRectUvPoints;
		}
	}

	public static Mesh EmptyMesh
	{
		get
		{
			if (emptyMesh == null)
			{
				emptyMesh = new Mesh();
			}
			return emptyMesh;
		}
	}

	public static Material[] EmptyMaterials
	{
		get
		{
			if (emptyMaterials == null)
			{
				emptyMaterials = new Material[0];
			}
			return emptyMaterials;
		}
	}

	public static Rect UnitRect => new Rect(0f, 0f, 1f, 1f);

	public static string GetUniqueID()
	{
		return Reverse(GetTimeTick().ToString());
	}

	public static long GetTimeTick()
	{
		return DateTime.Now.Ticks;
	}

	public static string Reverse(string s)
	{
		char[] array = s.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static void SetDirty(UnityEngine.Object o)
	{
	}

	public static void AddObjectToAsset(UnityEngine.Object objectToAdd, UnityEngine.Object asset)
	{
	}

	public static Texture2D CreateTexture(int resolution, Color fill, TextureFormat format = TextureFormat.ARGB32)
	{
		Texture2D texture2D = new Texture2D(resolution, resolution, format, mipChain: false);
		Color[] array = new Color[resolution * resolution];
		JUtilities.Fill(array, fill);
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static void CopyToRT(Texture t, RenderTexture rt)
	{
		RenderTexture.active = rt;
		Graphics.Blit(t, rt);
		RenderTexture.active = null;
	}

	public static void CopyFromRT(Texture2D t, RenderTexture rt)
	{
		RenderTexture.active = rt;
		t.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0);
		t.Apply();
		RenderTexture.active = null;
	}

	public static void CopyTexture(Texture2D src, Texture2D des)
	{
		RenderTexture renderTexture = new RenderTexture(des.width, des.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default);
		CopyToRT(src, renderTexture);
		CopyFromRT(des, renderTexture);
		renderTexture.Release();
		JUtilities.DestroyObject(renderTexture);
	}

	public static Texture2D CloneTexture(Texture2D t)
	{
		RenderTexture renderTexture = new RenderTexture(t.width, t.height, 0, RenderTextureFormat.ARGB32);
		CopyToRT(t, renderTexture);
		Texture2D obj = new Texture2D(t.width, t.height, TextureFormat.ARGB32, mipChain: false)
		{
			filterMode = t.filterMode,
			wrapMode = t.wrapMode
		};
		CopyFromRT(obj, renderTexture);
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
		return obj;
	}

	public static void FillTexture(Texture2D t, Color c)
	{
		Color[] array = new Color[t.width * t.height];
		JUtilities.Fill(array, c);
		t.SetPixels(array);
		t.Apply();
	}

	public static void FillTexture(RenderTexture rt, Color c)
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
		texture2D.SetPixel(0, 0, c);
		texture2D.Apply();
		CopyToRT(texture2D, rt);
		JUtilities.DestroyObject(texture2D);
	}

	public static Texture2D CloneAndResizeTexture(Texture2D t, int width, int height)
	{
		RenderTexture renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
		CopyToRT(t, renderTexture);
		Texture2D obj = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false)
		{
			filterMode = t.filterMode,
			wrapMode = t.wrapMode
		};
		CopyFromRT(obj, renderTexture);
		renderTexture.Release();
		UnityEngine.Object.DestroyImmediate(renderTexture);
		return obj;
	}

	public static RenderTexture CopyToRT(Texture src, int startX, int startY, int width, int height, Color defaultColor)
	{
		int num = startX + width - 1;
		int num2 = startY + height - 1;
		Vector2 vector = new Vector2(JUtilities.InverseLerpUnclamped(0f, src.width - 1, startX), JUtilities.InverseLerpUnclamped(0f, src.height - 1, startY));
		Vector2 vector2 = new Vector2(JUtilities.InverseLerpUnclamped(0f, src.width - 1, num), JUtilities.InverseLerpUnclamped(0f, src.height - 1, num2));
		Material copyTextureMaterial = JInternalMaterials.CopyTextureMaterial;
		copyTextureMaterial.SetTexture("_MainTex", src);
		copyTextureMaterial.SetVector("_StartUV", vector);
		copyTextureMaterial.SetVector("_EndUV", vector2);
		copyTextureMaterial.SetColor("_DefaultColor", defaultColor);
		copyTextureMaterial.SetPass(0);
		RenderTexture result = (RenderTexture.active = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB));
		Graphics.Blit(src, copyTextureMaterial);
		RenderTexture.active = null;
		return result;
	}

	public static void DrawTexture(RenderTexture rt, Texture texture, Rect uvRect, Material mat, int pass = 0)
	{
		if (mat == null)
		{
			mat = JInternalMaterials.UnlitTextureMaterial;
		}
		RenderTexture.active = rt;
		GL.PushMatrix();
		mat.SetTexture("_MainTex", texture);
		mat.SetPass(pass);
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(0f, 0f, 0f));
		GL.Vertex3(uvRect.min.x, uvRect.min.y, 0f);
		GL.TexCoord(new Vector3(0f, 1f, 0f));
		GL.Vertex3(uvRect.min.x, uvRect.max.y, 0f);
		GL.TexCoord(new Vector3(1f, 1f, 0f));
		GL.Vertex3(uvRect.max.x, uvRect.max.y, 0f);
		GL.TexCoord(new Vector3(1f, 0f, 0f));
		GL.Vertex3(uvRect.max.x, uvRect.min.y, 0f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = null;
	}

	public static void DrawTriangle(RenderTexture rt, Vector2 v0, Vector2 v1, Vector2 v2, Color c)
	{
		Material solidColorMaterial = JInternalMaterials.SolidColorMaterial;
		solidColorMaterial.SetColor("_Color", c);
		RenderTexture.active = rt;
		GL.PushMatrix();
		solidColorMaterial.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(4);
		GL.Vertex3(v0.x, v0.y, 0f);
		GL.Vertex3(v1.x, v1.y, 0f);
		GL.Vertex3(v2.x, v2.y, 0f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = null;
	}

	public static void DrawQuad(RenderTexture rt, Vector2[] quadCorners, Material mat, int pass)
	{
		RenderTexture.active = rt;
		GL.PushMatrix();
		mat.SetPass(pass);
		GL.LoadOrtho();
		GL.Begin(7);
		GL.TexCoord(new Vector3(0f, 0f, 0f));
		GL.Vertex3(quadCorners[0].x, quadCorners[0].y, 0f);
		GL.TexCoord(new Vector3(0f, 1f, 0f));
		GL.Vertex3(quadCorners[1].x, quadCorners[1].y, 0f);
		GL.TexCoord(new Vector3(1f, 1f, 0f));
		GL.Vertex3(quadCorners[2].x, quadCorners[2].y, 0f);
		GL.TexCoord(new Vector3(1f, 0f, 0f));
		GL.Vertex3(quadCorners[3].x, quadCorners[3].y, 0f);
		GL.End();
		GL.PopMatrix();
		RenderTexture.active = null;
	}

	public static List<Type> GetAllLoadedTypes()
	{
		List<Type> list = new List<Type>();
		List<string> list2 = new List<string>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			foreach (Type type in types)
			{
				if (type.IsVisible && !type.IsGenericType)
				{
					list2.Add(type.Name);
					list.Add(type);
				}
			}
		}
		return list;
	}

	public static IEnumerable<Rect> CompareHeightMap(int gridSize, Color[] oldValues, Color[] newValues)
	{
		if (oldValues.LongLength != newValues.LongLength)
		{
			return new Rect[1]
			{
				new Rect(0f, 0f, 1f, 1f)
			};
		}
		Rect[] array = new Rect[gridSize * gridSize];
		for (int i = 0; i < gridSize; i++)
		{
			for (int j = 0; j < gridSize; j++)
			{
				array[JUtilities.To1DIndex(i, j, gridSize)] = GetUvRange(gridSize, i, j);
			}
		}
		HashSet<Rect> hashSet = new HashSet<Rect>();
		int num = 0;
		int num2 = Mathf.RoundToInt(Mathf.Sqrt(newValues.LongLength));
		for (int k = 0; k < array.Length; k++)
		{
			Rect item = array[k];
			int num3 = (int)Mathf.Lerp(0f, num2 - 1, item.min.x);
			int num4 = (int)Mathf.Lerp(0f, num2 - 1, item.min.y);
			int num5 = (int)Mathf.Lerp(0f, num2 - 1, item.max.x);
			int num6 = (int)Mathf.Lerp(0f, num2 - 1, item.max.y);
			for (int l = num3; l <= num5; l++)
			{
				for (int m = num4; m <= num6; m++)
				{
					num = JUtilities.To1DIndex(l, m, num2);
					if (oldValues[num].r != newValues[num].r || oldValues[num].g != newValues[num].g || oldValues[num].b != newValues[num].b || oldValues[num].a != newValues[num].a)
					{
						hashSet.Add(item);
						hashSet.Add(new Rect
						{
							size = new Vector2(item.width * 1.2f, item.height),
							center = item.center
						});
						hashSet.Add(new Rect
						{
							size = new Vector2(item.width, item.height * 1.2f),
							center = item.center
						});
						break;
					}
				}
				if (hashSet.Contains(item))
				{
					break;
				}
			}
		}
		return hashSet;
	}

	public static Rect GetUvRange(int gridSize, int x, int z)
	{
		Vector2 position = new Vector2((float)x * 1f / (float)gridSize, (float)z * 1f / (float)gridSize);
		Vector2 size = Vector2.one / gridSize;
		return new Rect(position, size);
	}

	public static Texture2D CreateTextureFromCurve(AnimationCurve curve, int width, int height)
	{
		Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		texture2D.wrapMode = TextureWrapMode.Clamp;
		Color[] array = new Color[width * height];
		for (int i = 0; i < width; i++)
		{
			float time = Mathf.InverseLerp(0f, width - 1, i);
			float num = curve.Evaluate(time);
			Color color = new Color(num, num, num, num);
			for (int j = 0; j < height; j++)
			{
				array[JUtilities.To1DIndex(i, j, width)] = color;
			}
		}
		texture2D.filterMode = FilterMode.Bilinear;
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public static Vector3[] GetBrushQuadCorners(Vector3 center, float radius, float rotation)
	{
		Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, rotation, 0f));
		return new Vector3[4]
		{
			center + matrix4x.MultiplyPoint(new Vector3(-1f, 0f, -1f) * radius),
			center + matrix4x.MultiplyPoint(new Vector3(-1f, 0f, 1f) * radius),
			center + matrix4x.MultiplyPoint(new Vector3(1f, 0f, 1f) * radius),
			center + matrix4x.MultiplyPoint(new Vector3(1f, 0f, -1f) * radius)
		};
	}

	public static void ClearRT(RenderTexture rt)
	{
		RenderTexture.active = rt;
		GL.Clear(clearDepth: true, clearColor: true, Color.clear);
		RenderTexture.active = null;
	}

	public static void SetMaterialKeywordActive(Material mat, string keyword, bool active)
	{
		if (active)
		{
			mat.EnableKeyword(keyword);
		}
		else
		{
			mat.DisableKeyword(keyword);
		}
	}

	public static void Editor_ProgressBar(string title, string detail, float percent)
	{
	}

	public static void Editor_CancelableProgressBar(string title, string detail, float percent)
	{
	}

	public static void Editor_ClearProgressBar()
	{
	}

	public static Camera CreateCamera()
	{
		return new GameObject().AddComponent<Camera>();
	}
}
