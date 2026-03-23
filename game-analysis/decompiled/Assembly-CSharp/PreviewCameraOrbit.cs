using System;
using UnityEngine;

// Token: 0x0200008A RID: 138
public class PreviewCameraOrbit : MonoBehaviour
{
	// Token: 0x060003D2 RID: 978 RVA: 0x00014D0C File Offset: 0x00012F0C
	public void SetTarget(Transform newTarget, float initialDistance, bool isGeneratingIcons)
	{
		this.target = newTarget;
		this.distance = Mathf.Clamp(initialDistance, this.minDistance, this.maxDistance);
		if (isGeneratingIcons)
		{
			this.yaw = -30f;
			this.pitch = 30f;
			this.lastManualRotateTime = float.PositiveInfinity;
		}
		else
		{
			this.yaw = 0f;
			this.pitch = 20f;
			this.lastManualRotateTime = float.NegativeInfinity;
		}
		this.autoRotateWeight = 1f;
		this.StopRotation();
	}

	// Token: 0x060003D3 RID: 979 RVA: 0x00014D90 File Offset: 0x00012F90
	private void LateUpdate()
	{
		if (this.target == null)
		{
			return;
		}
		if (this.IsHovering)
		{
			float y = Input.mouseScrollDelta.y;
			this.distance -= y * this.zoomSpeed;
			this.distance = Mathf.Clamp(this.distance, this.minDistance, this.maxDistance);
		}
		if (this.IsHovering && Input.GetMouseButtonDown(0))
		{
			this.StartRotation();
		}
		if (this.isRotating && Input.GetMouseButtonUp(0))
		{
			this.StopRotation();
		}
		if (this.isRotating)
		{
			this.yaw += Input.GetAxis("Mouse X") * this.rotationSpeed;
			this.pitch -= Input.GetAxis("Mouse Y") * this.rotationSpeed;
			this.pitch = Mathf.Clamp(this.pitch, -85f, 85f);
			this.lastManualRotateTime = Time.time;
		}
		float num = ((Time.time - this.lastManualRotateTime >= this.autoRotateDelay) ? 1f : 0f);
		this.autoRotateWeight = Mathf.MoveTowards(this.autoRotateWeight, num, this.autoRotateEaseSpeed * Time.deltaTime);
		this.yaw += this.autoRotateSpeed * Time.deltaTime * this.autoRotateWeight;
		Vector3 vector = Quaternion.Euler(this.pitch, this.yaw, 0f) * Vector3.back * this.distance;
		base.transform.position = this.target.position + vector;
		base.transform.LookAt(this.target.position);
	}

	// Token: 0x060003D4 RID: 980 RVA: 0x00014F44 File Offset: 0x00013144
	private void StartRotation()
	{
		if (this.isRotating)
		{
			return;
		}
		this.isRotating = true;
	}

	// Token: 0x060003D5 RID: 981 RVA: 0x00014F56 File Offset: 0x00013156
	private void StopRotation()
	{
		if (!this.isRotating)
		{
			return;
		}
		this.isRotating = false;
	}

	// Token: 0x04000416 RID: 1046
	public Transform target;

	// Token: 0x04000417 RID: 1047
	public bool IsHovering;

	// Token: 0x04000418 RID: 1048
	public float distance = 2f;

	// Token: 0x04000419 RID: 1049
	public float zoomSpeed = 1.5f;

	// Token: 0x0400041A RID: 1050
	public float rotationSpeed = 3f;

	// Token: 0x0400041B RID: 1051
	public float autoRotateSpeed = 10f;

	// Token: 0x0400041C RID: 1052
	public float autoRotateDelay = 3f;

	// Token: 0x0400041D RID: 1053
	public float autoRotateEaseSpeed = 2f;

	// Token: 0x0400041E RID: 1054
	public float minDistance = 0.5f;

	// Token: 0x0400041F RID: 1055
	public float maxDistance = 5f;

	// Token: 0x04000420 RID: 1056
	private float yaw;

	// Token: 0x04000421 RID: 1057
	private float pitch = 20f;

	// Token: 0x04000422 RID: 1058
	private float lastManualRotateTime = float.NegativeInfinity;

	// Token: 0x04000423 RID: 1059
	private float autoRotateWeight = 1f;

	// Token: 0x04000424 RID: 1060
	private bool isRotating;
}
