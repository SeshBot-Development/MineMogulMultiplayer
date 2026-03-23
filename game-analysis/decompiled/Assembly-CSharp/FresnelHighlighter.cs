using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Token: 0x02000050 RID: 80
[RequireComponent(typeof(Camera))]
public class FresnelHighlighter : MonoBehaviour
{
	// Token: 0x06000217 RID: 535 RVA: 0x0000A824 File Offset: 0x00008A24
	private void OnEnable()
	{
		this._cam = base.GetComponent<Camera>();
		if (this.fresnelShader == null)
		{
			this.fresnelShader = Shader.Find("Hidden/Focus/FresnelAdd");
		}
		this._mat = new Material(this.fresnelShader)
		{
			hideFlags = HideFlags.DontSave
		};
		this._evt = ((this._cam.actualRenderingPath == RenderingPath.DeferredShading) ? CameraEvent.AfterLighting : CameraEvent.BeforeImageEffects);
		this._cb = new CommandBuffer
		{
			name = "Fresnel Highlighter"
		};
		this._cam.AddCommandBuffer(this._evt, this._cb);
	}

	// Token: 0x06000218 RID: 536 RVA: 0x0000A8BC File Offset: 0x00008ABC
	private void OnDisable()
	{
		if (this._cam != null && this._cb != null)
		{
			this._cam.RemoveCommandBuffer(this._evt, this._cb);
		}
		if (this._cb != null)
		{
			this._cb.Release();
		}
		if (this._mat != null)
		{
			Object.DestroyImmediate(this._mat);
		}
		this._cb = null;
		this._mat = null;
	}

	// Token: 0x06000219 RID: 537 RVA: 0x0000A930 File Offset: 0x00008B30
	public void Highlight(Renderer r, Color color)
	{
		HighlightStyle highlightStyle = new HighlightStyle(color, this.GenericGrabbablePreset.RimPower, this.GenericGrabbablePreset.Intensity, this.GenericGrabbablePreset.XrayThroughWalls);
		this.Highlight(r, in highlightStyle);
	}

	// Token: 0x0600021A RID: 538 RVA: 0x0000A970 File Offset: 0x00008B70
	public void Highlight(Renderer r, in HighlightStyle style)
	{
		if (!r)
		{
			return;
		}
		List<Renderer> list;
		if (!this._styleBuckets.TryGetValue(style, out list))
		{
			list = new List<Renderer>(8);
			this._styleBuckets[style] = list;
		}
		list.Add(r);
	}

	// Token: 0x0600021B RID: 539 RVA: 0x0000A9BC File Offset: 0x00008BBC
	public void HighlightObject(GameObject obj, HighlightStyle highlightStyle)
	{
		foreach (Renderer renderer in obj.GetComponentsInChildren<Renderer>(false))
		{
			if (!(renderer is ParticleSystemRenderer) && renderer && renderer.enabled)
			{
				this.Highlight(renderer, in highlightStyle);
			}
		}
	}

	// Token: 0x0600021C RID: 540 RVA: 0x0000AA04 File Offset: 0x00008C04
	public void ClearAll()
	{
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> keyValuePair in this._styleBuckets)
		{
			keyValuePair.Value.Clear();
		}
		CommandBuffer cb = this._cb;
		if (cb == null)
		{
			return;
		}
		cb.Clear();
	}

	// Token: 0x0600021D RID: 541 RVA: 0x0000AA6C File Offset: 0x00008C6C
	private void LateUpdate()
	{
		if (this._cb == null || this._mat == null)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> keyValuePair in this._styleBuckets)
		{
			num += keyValuePair.Value.Count;
		}
		if (num == 0)
		{
			return;
		}
		this._cb.Clear();
		foreach (KeyValuePair<HighlightStyle, List<Renderer>> keyValuePair2 in this._styleBuckets)
		{
			HighlightStyle key = keyValuePair2.Key;
			List<Renderer> value = keyValuePair2.Value;
			if (value != null && value.Count != 0)
			{
				this._mat.SetColor("_Color", key.Color);
				this._mat.SetFloat("_Power", key.RimPower);
				this._mat.SetFloat("_Intensity", key.Intensity);
				this._mat.SetFloat("_ZTest", key.XrayThroughWalls ? 8f : 4f);
				for (int i = 0; i < value.Count; i++)
				{
					Renderer renderer = value[i];
					if (renderer && renderer.enabled)
					{
						int num2 = ((renderer.sharedMaterials != null) ? renderer.sharedMaterials.Length : 1);
						for (int j = 0; j < num2; j++)
						{
							this._cb.DrawRenderer(renderer, this._mat, j, 0);
						}
					}
				}
			}
		}
	}

	// Token: 0x040001F7 RID: 503
	[Header("Presets")]
	public HighlightStyle ToolPreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);

	// Token: 0x040001F8 RID: 504
	public HighlightStyle GenericGrabbablePreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);

	// Token: 0x040001F9 RID: 505
	public HighlightStyle BuildingPreset = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);

	// Token: 0x040001FA RID: 506
	public HighlightStyle WrenchEnableSupports = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);

	// Token: 0x040001FB RID: 507
	public HighlightStyle WrenchDisableSupports = new HighlightStyle(new Color(0.25f, 0.85f, 1f, 1f), 2f, 1.2f, false);

	// Token: 0x040001FC RID: 508
	[Header("Shader")]
	public Shader fresnelShader;

	// Token: 0x040001FD RID: 509
	private Material _mat;

	// Token: 0x040001FE RID: 510
	private Camera _cam;

	// Token: 0x040001FF RID: 511
	private CommandBuffer _cb;

	// Token: 0x04000200 RID: 512
	private CameraEvent _evt = CameraEvent.AfterLighting;

	// Token: 0x04000201 RID: 513
	private readonly Dictionary<HighlightStyle, List<Renderer>> _styleBuckets = new Dictionary<HighlightStyle, List<Renderer>>();
}
