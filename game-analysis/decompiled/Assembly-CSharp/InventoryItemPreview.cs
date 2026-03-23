using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200005C RID: 92
public class InventoryItemPreview : Singleton<InventoryItemPreview>
{
	// Token: 0x06000240 RID: 576 RVA: 0x0000B243 File Offset: 0x00009443
	private void OnEnable()
	{
		this.PreviewCamera.enabled = false;
		this.StopPreview();
	}

	// Token: 0x06000241 RID: 577 RVA: 0x0000B258 File Offset: 0x00009458
	public void StopPreview()
	{
		this.PreviouslyShownTool = null;
		this.PreviewCamera.enabled = false;
		foreach (object obj in this.ObjectPreviewRoot)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		this.PreviewCameraOrbit.enabled = false;
		this.IsEnabled = false;
	}

	// Token: 0x06000242 RID: 578 RVA: 0x0000B2DC File Offset: 0x000094DC
	public void StartPreview(BaseHeldTool toolPrefab, bool isGeneratingIcons)
	{
		if (toolPrefab == null)
		{
			this.StopPreview();
			return;
		}
		if (this.PreviouslyShownTool == toolPrefab)
		{
			return;
		}
		this.PreviouslyShownTool = toolPrefab;
		this.PreviewCamera.enabled = false;
		foreach (object obj in this.ObjectPreviewRoot)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		ToolBuilder toolBuilder = toolPrefab as ToolBuilder;
		if (toolBuilder != null)
		{
			BuildingObject buildingObject = Object.Instantiate<BuildingObject>(toolBuilder.Definition.GetMainPrefab(), this.ObjectPreviewRoot);
			if (!isGeneratingIcons && buildingObject.ExtraGhostRenderers != null)
			{
				buildingObject.ExtraGhostRenderers.SetActive(true);
			}
			this.DisableNonRendererComponents(buildingObject.gameObject);
			this.SetLayerRecursively(buildingObject.gameObject);
		}
		else
		{
			BaseHeldTool baseHeldTool = Object.Instantiate<BaseHeldTool>(toolPrefab, this.ObjectPreviewRoot);
			baseHeldTool.transform.localPosition = Vector3.zero;
			baseHeldTool.transform.localRotation = Quaternion.identity;
			baseHeldTool.HideWorldModel(false);
			baseHeldTool.HideViewModel(true);
			if (baseHeldTool.WorldModel != null)
			{
				baseHeldTool.WorldModel.transform.localPosition = Vector3.zero;
				baseHeldTool.WorldModel.transform.localRotation = Quaternion.identity;
			}
			this.DisableNonRendererComponents(baseHeldTool.gameObject);
			this.SetLayerRecursively(baseHeldTool.gameObject);
		}
		base.StartCoroutine(this.WaitThenEnable3dPreview(isGeneratingIcons));
		this.IsEnabled = true;
	}

	// Token: 0x06000243 RID: 579 RVA: 0x0000B470 File Offset: 0x00009670
	public void StartOrePiecePreview(OrePiece orePiecePrefab, bool isGeneratingIcons)
	{
		if (orePiecePrefab == null)
		{
			this.StopPreview();
			return;
		}
		this.PreviewCamera.enabled = false;
		foreach (object obj in this.ObjectPreviewRoot)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
		OrePiece orePiece = Object.Instantiate<OrePiece>(orePiecePrefab, this.ObjectPreviewRoot);
		orePiece.transform.localPosition = Vector3.zero;
		orePiece.transform.localRotation = Quaternion.identity;
		orePiece.UseRandomMesh = false;
		orePiece.UseRandomScale = false;
		orePiece.GetComponent<Rigidbody>().isKinematic = true;
		orePiece.enabled = false;
		this.DisableNonRendererComponents(orePiece.gameObject);
		this.SetLayerRecursively(orePiece.gameObject);
		base.StartCoroutine(this.WaitThenEnable3dPreview(isGeneratingIcons));
		this.IsEnabled = true;
	}

	// Token: 0x06000244 RID: 580 RVA: 0x0000B564 File Offset: 0x00009764
	private IEnumerator WaitThenEnable3dPreview(bool isGeneratingIcons)
	{
		yield return new WaitForFixedUpdate();
		if (this.IsEnabled)
		{
			this.PreviewCameraOrbit.enabled = true;
			this.FrameObjectInPreviewCamera(this.ObjectPreviewRoot, isGeneratingIcons);
			this.PreviewCamera.enabled = true;
		}
		yield break;
	}

	// Token: 0x06000245 RID: 581 RVA: 0x0000B57C File Offset: 0x0000977C
	private void DisableNonRendererComponents(GameObject root)
	{
		HashSet<Transform> hashSet = new HashSet<Transform>();
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
		{
			if (skinnedMeshRenderer.rootBone != null)
			{
				hashSet.Add(skinnedMeshRenderer.rootBone);
			}
			if (skinnedMeshRenderer.bones != null)
			{
				foreach (Transform transform in skinnedMeshRenderer.bones)
				{
					if (transform != null)
					{
						hashSet.Add(transform);
					}
				}
			}
		}
		foreach (Transform transform2 in root.GetComponentsInChildren<Transform>(true))
		{
			foreach (Component component in transform2.GetComponents<Component>())
			{
				if (!(component is Transform) && !(component is Renderer) && !(component is MeshFilter) && !(component is SkinnedMeshRenderer))
				{
					string @namespace = component.GetType().Namespace;
					if ((string.IsNullOrEmpty(@namespace) || !@namespace.StartsWith("UnityEngine")) && !hashSet.Contains(transform2.transform))
					{
						Object.Destroy(component);
					}
				}
			}
		}
		root.SetActive(true);
		Rigidbody[] componentsInChildren2 = root.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].isKinematic = true;
		}
		foreach (ParticleSystem particleSystem in root.GetComponentsInChildren<ParticleSystem>())
		{
			particleSystem.enableEmission = false;
			particleSystem.gameObject.SetActive(false);
		}
		AudioSource[] componentsInChildren4 = root.GetComponentsInChildren<AudioSource>(true);
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].enabled = false;
		}
	}

	// Token: 0x06000246 RID: 582 RVA: 0x0000B720 File Offset: 0x00009920
	private void SetLayerRecursively(GameObject obj)
	{
		int num = LayerMask.NameToLayer("ItemPreviewUI");
		Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = num;
		}
	}

	// Token: 0x06000247 RID: 583 RVA: 0x0000B75C File Offset: 0x0000995C
	private Bounds CalculateRenderersBounds(Transform root)
	{
		Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(root.position, Vector3.one * 0.1f);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			bounds.Encapsulate(componentsInChildren[i].bounds);
		}
		return bounds;
	}

	// Token: 0x06000248 RID: 584 RVA: 0x0000B7B8 File Offset: 0x000099B8
	private void FrameObjectInPreviewCamera(Transform root, bool isGeneratingIcons)
	{
		Camera previewCamera = this.PreviewCamera;
		previewCamera.orthographic = isGeneratingIcons;
		Bounds bounds = this.CalculateRenderersBounds(root);
		Vector3 center = bounds.center;
		Transform transform = new GameObject("PreviewCenterPivot").transform;
		transform.SetParent(root, false);
		transform.position = center;
		float magnitude = bounds.extents.magnitude;
		float num = (isGeneratingIcons ? 1.05f : 1.2f);
		float num2 = magnitude * num / Mathf.Sin(previewCamera.fieldOfView * 0.5f * 0.017453292f);
		Vector3 vector = previewCamera.transform.forward * -num2;
		previewCamera.transform.position = center + vector;
		previewCamera.transform.LookAt(center);
		if (isGeneratingIcons)
		{
			Vector3 extents = bounds.extents;
			Vector3 center2 = bounds.center;
			Vector3[] array = new Vector3[]
			{
				center2 + new Vector3(-extents.x, -extents.y, -extents.z),
				center2 + new Vector3(-extents.x, -extents.y, extents.z),
				center2 + new Vector3(-extents.x, extents.y, -extents.z),
				center2 + new Vector3(-extents.x, extents.y, extents.z),
				center2 + new Vector3(extents.x, -extents.y, -extents.z),
				center2 + new Vector3(extents.x, -extents.y, extents.z),
				center2 + new Vector3(extents.x, extents.y, -extents.z),
				center2 + new Vector3(extents.x, extents.y, extents.z)
			};
			float num3 = float.PositiveInfinity;
			float num4 = float.NegativeInfinity;
			float num5 = float.PositiveInfinity;
			float num6 = float.NegativeInfinity;
			float num7 = float.PositiveInfinity;
			float num8 = float.NegativeInfinity;
			for (int i = 0; i < 8; i++)
			{
				Vector3 vector2 = previewCamera.transform.InverseTransformPoint(array[i]);
				if (vector2.x < num3)
				{
					num3 = vector2.x;
				}
				if (vector2.x > num4)
				{
					num4 = vector2.x;
				}
				if (vector2.y < num5)
				{
					num5 = vector2.y;
				}
				if (vector2.y > num6)
				{
					num6 = vector2.y;
				}
				if (vector2.z < num7)
				{
					num7 = vector2.z;
				}
				if (vector2.z > num8)
				{
					num8 = vector2.z;
				}
			}
			float num9 = (num6 - num5) * 0.5f;
			float num10 = (num4 - num3) * 0.5f;
			float num11 = Mathf.Max(num9, num10 / previewCamera.aspect) * num;
			previewCamera.orthographicSize = Mathf.Max(0.001f, num11);
			float num12 = Mathf.Max(0.1f, magnitude * 2f);
			previewCamera.nearClipPlane = Mathf.Max(0.01f, num7 - num12);
			previewCamera.farClipPlane = Mathf.Max(previewCamera.nearClipPlane + 0.1f, num8 + num12);
		}
		this.PreviewCameraOrbit.SetTarget(transform, num2, isGeneratingIcons);
	}

	// Token: 0x04000218 RID: 536
	public Transform ObjectPreviewRoot;

	// Token: 0x04000219 RID: 537
	public Camera PreviewCamera;

	// Token: 0x0400021A RID: 538
	public BaseHeldTool PreviouslyShownTool;

	// Token: 0x0400021B RID: 539
	private bool IsEnabled;

	// Token: 0x0400021C RID: 540
	public PreviewCameraOrbit PreviewCameraOrbit;
}
