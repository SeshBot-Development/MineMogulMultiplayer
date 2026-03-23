using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryItemPreview : Singleton<InventoryItemPreview>
{
	public Transform ObjectPreviewRoot;

	public Camera PreviewCamera;

	public BaseHeldTool PreviouslyShownTool;

	private bool IsEnabled;

	public PreviewCameraOrbit PreviewCameraOrbit;

	private void OnEnable()
	{
		PreviewCamera.enabled = false;
		StopPreview();
	}

	public void StopPreview()
	{
		PreviouslyShownTool = null;
		PreviewCamera.enabled = false;
		foreach (Transform item in ObjectPreviewRoot)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		PreviewCameraOrbit.enabled = false;
		IsEnabled = false;
	}

	public void StartPreview(BaseHeldTool toolPrefab, bool isGeneratingIcons)
	{
		if (toolPrefab == null)
		{
			StopPreview();
		}
		else
		{
			if (PreviouslyShownTool == toolPrefab)
			{
				return;
			}
			PreviouslyShownTool = toolPrefab;
			PreviewCamera.enabled = false;
			foreach (Transform item in ObjectPreviewRoot)
			{
				UnityEngine.Object.Destroy(item.gameObject);
			}
			if (toolPrefab is ToolBuilder toolBuilder)
			{
				BuildingObject buildingObject = UnityEngine.Object.Instantiate(toolBuilder.Definition.GetMainPrefab(), ObjectPreviewRoot);
				if (!isGeneratingIcons && buildingObject.ExtraGhostRenderers != null)
				{
					buildingObject.ExtraGhostRenderers.SetActive(value: true);
				}
				DisableNonRendererComponents(buildingObject.gameObject);
				SetLayerRecursively(buildingObject.gameObject);
			}
			else
			{
				BaseHeldTool baseHeldTool = UnityEngine.Object.Instantiate(toolPrefab, ObjectPreviewRoot);
				baseHeldTool.transform.localPosition = Vector3.zero;
				baseHeldTool.transform.localRotation = Quaternion.identity;
				baseHeldTool.HideWorldModel(hide: false);
				baseHeldTool.HideViewModel();
				if (baseHeldTool.WorldModel != null)
				{
					baseHeldTool.WorldModel.transform.localPosition = Vector3.zero;
					baseHeldTool.WorldModel.transform.localRotation = Quaternion.identity;
				}
				DisableNonRendererComponents(baseHeldTool.gameObject);
				SetLayerRecursively(baseHeldTool.gameObject);
			}
			StartCoroutine(WaitThenEnable3dPreview(isGeneratingIcons));
			IsEnabled = true;
		}
	}

	public void StartOrePiecePreview(OrePiece orePiecePrefab, bool isGeneratingIcons)
	{
		if (orePiecePrefab == null)
		{
			StopPreview();
			return;
		}
		PreviewCamera.enabled = false;
		foreach (Transform item in ObjectPreviewRoot)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		OrePiece orePiece = UnityEngine.Object.Instantiate(orePiecePrefab, ObjectPreviewRoot);
		orePiece.transform.localPosition = Vector3.zero;
		orePiece.transform.localRotation = Quaternion.identity;
		orePiece.UseRandomMesh = false;
		orePiece.UseRandomScale = false;
		orePiece.GetComponent<Rigidbody>().isKinematic = true;
		orePiece.enabled = false;
		DisableNonRendererComponents(orePiece.gameObject);
		SetLayerRecursively(orePiece.gameObject);
		StartCoroutine(WaitThenEnable3dPreview(isGeneratingIcons));
		IsEnabled = true;
	}

	private IEnumerator WaitThenEnable3dPreview(bool isGeneratingIcons)
	{
		yield return new WaitForFixedUpdate();
		if (IsEnabled)
		{
			PreviewCameraOrbit.enabled = true;
			FrameObjectInPreviewCamera(ObjectPreviewRoot, isGeneratingIcons);
			PreviewCamera.enabled = true;
		}
	}

	private void DisableNonRendererComponents(GameObject root)
	{
		HashSet<Transform> hashSet = new HashSet<Transform>();
		SkinnedMeshRenderer[] componentsInChildren = root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
		Transform[] bones;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren)
		{
			if (skinnedMeshRenderer.rootBone != null)
			{
				hashSet.Add(skinnedMeshRenderer.rootBone);
			}
			if (skinnedMeshRenderer.bones == null)
			{
				continue;
			}
			bones = skinnedMeshRenderer.bones;
			foreach (Transform transform in bones)
			{
				if (transform != null)
				{
					hashSet.Add(transform);
				}
			}
		}
		bones = root.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform2 in bones)
		{
			Component[] components = transform2.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (!(component is Transform) && !(component is Renderer) && !(component is MeshFilter) && !(component is SkinnedMeshRenderer))
				{
					string text = component.GetType().Namespace;
					if ((string.IsNullOrEmpty(text) || !text.StartsWith("UnityEngine")) && !hashSet.Contains(transform2.transform))
					{
						UnityEngine.Object.Destroy(component);
					}
				}
			}
		}
		root.SetActive(value: true);
		Rigidbody[] componentsInChildren2 = root.GetComponentsInChildren<Rigidbody>();
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].isKinematic = true;
		}
		ParticleSystem[] componentsInChildren3 = root.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem obj in componentsInChildren3)
		{
			obj.enableEmission = false;
			obj.gameObject.SetActive(value: false);
		}
		AudioSource[] componentsInChildren4 = root.GetComponentsInChildren<AudioSource>(includeInactive: true);
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].enabled = false;
		}
	}

	private void SetLayerRecursively(GameObject obj)
	{
		int layer = LayerMask.NameToLayer("ItemPreviewUI");
		Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

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

	private void FrameObjectInPreviewCamera(Transform root, bool isGeneratingIcons)
	{
		Camera previewCamera = PreviewCamera;
		previewCamera.orthographic = isGeneratingIcons;
		Bounds bounds = CalculateRenderersBounds(root);
		Vector3 center = bounds.center;
		Transform transform = new GameObject("PreviewCenterPivot").transform;
		transform.SetParent(root, worldPositionStays: false);
		transform.position = center;
		float magnitude = bounds.extents.magnitude;
		float num = (isGeneratingIcons ? 1.05f : 1.2f);
		float num2 = magnitude * num / Mathf.Sin(previewCamera.fieldOfView * 0.5f * (MathF.PI / 180f));
		Vector3 vector = previewCamera.transform.forward * (0f - num2);
		previewCamera.transform.position = center + vector;
		previewCamera.transform.LookAt(center);
		if (isGeneratingIcons)
		{
			Vector3 extents = bounds.extents;
			Vector3 center2 = bounds.center;
			Vector3[] array = new Vector3[8]
			{
				center2 + new Vector3(0f - extents.x, 0f - extents.y, 0f - extents.z),
				center2 + new Vector3(0f - extents.x, 0f - extents.y, extents.z),
				center2 + new Vector3(0f - extents.x, extents.y, 0f - extents.z),
				center2 + new Vector3(0f - extents.x, extents.y, extents.z),
				center2 + new Vector3(extents.x, 0f - extents.y, 0f - extents.z),
				center2 + new Vector3(extents.x, 0f - extents.y, extents.z),
				center2 + new Vector3(extents.x, extents.y, 0f - extents.z),
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
			float a = (num6 - num5) * 0.5f;
			float num9 = (num4 - num3) * 0.5f;
			float b = Mathf.Max(a, num9 / previewCamera.aspect) * num;
			previewCamera.orthographicSize = Mathf.Max(0.001f, b);
			float num10 = Mathf.Max(0.1f, magnitude * 2f);
			previewCamera.nearClipPlane = Mathf.Max(0.01f, num7 - num10);
			previewCamera.farClipPlane = Mathf.Max(previewCamera.nearClipPlane + 0.1f, num8 + num10);
		}
		PreviewCameraOrbit.SetTarget(transform, num2, isGeneratingIcons);
	}
}
