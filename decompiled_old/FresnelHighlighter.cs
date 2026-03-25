using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class FresnelHighlighter : MonoBehaviour
{
	[Header("Presets")]
	public HighlightStyle ToolPreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	public HighlightStyle GenericGrabbablePreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	public HighlightStyle BuildingPreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	public HighlightStyle WrenchEnableSupports = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	public HighlightStyle WrenchDisableSupports = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, xray: false);

	[Header("Shader")]
	public Shader fresnelShader;

	private Material _mat;

	private Camera _cam;

	private CommandBuffer _cb;

	private CameraEvent _evt = CameraEvent.AfterLighting;

	private readonly Dictionary<HighlightStyle, List<Renderer>> _styleBuckets = new Dictionary<HighlightStyle, List<Renderer>>();

	private void OnEnable()
	{
		_cam = GetComponent<Camera>();
		if (fresnelShader == null)
		{
			fresnelShader = Shader.Find("Hidden/Focus/FresnelAdd");
		}
		_mat = new Material(fresnelShader)
		{
			hideFlags = HideFlags.DontSave
		};
		_evt = ((_cam.actualRenderingPath == RenderingPath.DeferredShading) ? CameraEvent.AfterLighting : CameraEvent.BeforeImageEffects);
		_cb = new CommandBuffer
		{
			name = "Fresnel Highlighter"
		};
		_cam.AddCommandBuffer(_evt, _cb);
	}

	private void OnDisable()
	{
		if (_cam != null && _cb != null)
		{
			_cam.RemoveCommandBuffer(_evt, _cb);
		}
		if (_cb != null)
		{
			_cb.Release();
		}
		if (_mat != null)
		{
			Object.DestroyImmediate(_mat);
		}
		_cb = null;
		_mat = null;
	}

	public void Highlight(Renderer r, Color color)
	{
		Highlight(r, new HighlightStyle(color, GenericGrabbablePreset.RimPower, GenericGrabbablePreset.Intensity, GenericGrabbablePreset.XrayThroughWalls));
	}

	public void Highlight(Renderer r, in HighlightStyle style)
	{
		if ((bool)r)
		{
			if (!_styleBuckets.TryGetValue(style, out var value))
			{
				value = new List<Renderer>(8);
				_styleBuckets[style] = value;
			}
			value.Add(r);
		}
	}

	public void HighlightObject(GameObject obj, HighlightStyle highlightStyle)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(includeInactive: false);
		foreach (Renderer renderer in componentsInChildren)
		{
			if (!(renderer is ParticleSystemRenderer) && (bool)renderer && renderer.enabled)
			{
				Highlight(renderer, in highlightStyle);
			}
		}
	}

	public void ClearAll()
	{
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> styleBucket in _styleBuckets)
		{
			styleBucket.Value.Clear();
		}
		_cb?.Clear();
	}

	private void LateUpdate()
	{
		if (_cb == null || _mat == null)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> styleBucket in _styleBuckets)
		{
			num += styleBucket.Value.Count;
		}
		if (num == 0)
		{
			return;
		}
		_cb.Clear();
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> styleBucket2 in _styleBuckets)
		{
			HighlightStyle key = styleBucket2.Key;
			List<Renderer> value = styleBucket2.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			_mat.SetColor("_Color", key.Color);
			_mat.SetFloat("_Power", key.RimPower);
			_mat.SetFloat("_Intensity", key.Intensity);
			_mat.SetFloat("_ZTest", key.XrayThroughWalls ? 8f : 4f);
			for (int i = 0; i < value.Count; i++)
			{
				Renderer renderer = value[i];
				if ((bool)renderer && renderer.enabled)
				{
					int num2 = ((renderer.sharedMaterials == null) ? 1 : renderer.sharedMaterials.Length);
					for (int j = 0; j < num2; j++)
					{
						_cb.DrawRenderer(renderer, _mat, j, 0);
					}
				}
			}
		}
	}
}
