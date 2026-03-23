using UnityEngine;

public class MainMenuCameraShaker : MonoBehaviour
{
	public float positionAmplitude = 0.05f;

	public float rotationAmplitude = 0.2f;

	public float positionFrequency = 0.2f;

	public float rotationFrequency = 0.1f;

	private Vector3 initialPosition;

	private Quaternion initialRotation;

	private float timeOffset;

	private Vector3 currentPunchRotation = Vector3.zero;

	private Vector3 targetPunchRotation = Vector3.zero;

	private Vector3 punchVelocity = Vector3.zero;

	private float punchSmoothTime = 0.2f;

	private float punchRecoverSpeed = 4f;

	private void Start()
	{
		initialPosition = base.transform.localPosition;
		initialRotation = base.transform.localRotation;
		timeOffset = Random.value * 100f;
	}

	private void Update()
	{
		float num = Time.time + timeOffset;
		Vector3 vector = new Vector3((Mathf.PerlinNoise(num * positionFrequency, 0f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * positionFrequency, 1f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * positionFrequency, 2f) - 0.5f) * 2f) * positionAmplitude;
		Vector3 vector2 = new Vector3((Mathf.PerlinNoise(num * rotationFrequency, 3f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * rotationFrequency, 4f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * rotationFrequency, 5f) - 0.5f) * 2f) * rotationAmplitude;
		currentPunchRotation = Vector3.SmoothDamp(currentPunchRotation, targetPunchRotation, ref punchVelocity, punchSmoothTime);
		targetPunchRotation = Vector3.Lerp(targetPunchRotation, Vector3.zero, Time.deltaTime * punchRecoverSpeed);
		base.transform.localPosition = initialPosition + vector;
		base.transform.localRotation = initialRotation * Quaternion.Euler(vector2 + currentPunchRotation);
	}

	public void ApplyViewPunch(Vector3 punch)
	{
		targetPunchRotation += punch;
	}
}
