using UnityEngine;

public class PreviewCameraOrbit : MonoBehaviour
{
	public Transform target;

	public bool IsHovering;

	public float distance = 2f;

	public float zoomSpeed = 1.5f;

	public float rotationSpeed = 3f;

	public float autoRotateSpeed = 10f;

	public float autoRotateDelay = 3f;

	public float autoRotateEaseSpeed = 2f;

	public float minDistance = 0.5f;

	public float maxDistance = 5f;

	private float yaw;

	private float pitch = 20f;

	private float lastManualRotateTime = float.NegativeInfinity;

	private float autoRotateWeight = 1f;

	private bool isRotating;

	public void SetTarget(Transform newTarget, float initialDistance, bool isGeneratingIcons)
	{
		target = newTarget;
		distance = Mathf.Clamp(initialDistance, minDistance, maxDistance);
		if (isGeneratingIcons)
		{
			yaw = -30f;
			pitch = 30f;
			lastManualRotateTime = float.PositiveInfinity;
		}
		else
		{
			yaw = 0f;
			pitch = 20f;
			lastManualRotateTime = float.NegativeInfinity;
		}
		autoRotateWeight = 1f;
		StopRotation();
	}

	private void LateUpdate()
	{
		if (!(target == null))
		{
			if (IsHovering)
			{
				float y = Input.mouseScrollDelta.y;
				distance -= y * zoomSpeed;
				distance = Mathf.Clamp(distance, minDistance, maxDistance);
			}
			if (IsHovering && Input.GetMouseButtonDown(0))
			{
				StartRotation();
			}
			if (isRotating && Input.GetMouseButtonUp(0))
			{
				StopRotation();
			}
			if (isRotating)
			{
				yaw += Input.GetAxis("Mouse X") * rotationSpeed;
				pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
				pitch = Mathf.Clamp(pitch, -85f, 85f);
				lastManualRotateTime = Time.time;
			}
			float num = ((Time.time - lastManualRotateTime >= autoRotateDelay) ? 1f : 0f);
			autoRotateWeight = Mathf.MoveTowards(autoRotateWeight, num, autoRotateEaseSpeed * Time.deltaTime);
			yaw += autoRotateSpeed * Time.deltaTime * autoRotateWeight;
			Vector3 vector = Quaternion.Euler(pitch, yaw, 0f) * Vector3.back * distance;
			base.transform.position = target.position + vector;
			base.transform.LookAt(target.position);
		}
	}

	private void StartRotation()
	{
		if (!isRotating)
		{
			isRotating = true;
		}
	}

	private void StopRotation()
	{
		if (isRotating)
		{
			isRotating = false;
		}
	}
}
