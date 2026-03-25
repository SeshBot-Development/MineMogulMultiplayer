using UnityEngine;

public class MainMenuElevator : MonoBehaviour
{
	[Header("Idle Sway Settings")]
	public float swayAmplitude = 0.05f;

	public float swayFrequency = 0.1f;

	public float rotationAmplitude = 1.5f;

	public float rotationFrequency = 0.2f;

	[Header("Drop Settings")]
	public float dropSpeed = 20f;

	public float dropAcceleration = 60f;

	public float dropTargetY = -50f;

	private Vector3 initialPosition;

	private Quaternion initialRotation;

	private float timeOffset;

	private bool isDropping;

	private float currentDropSpeed;

	private void Start()
	{
		initialPosition = base.transform.localPosition;
		initialRotation = base.transform.localRotation;
		timeOffset = Random.value * 100f;
	}

	private void Update()
	{
		Input.GetKeyDown(KeyCode.Space);
		if (isDropping)
		{
			currentDropSpeed += dropAcceleration * Time.deltaTime;
			Vector3 localPosition = base.transform.localPosition;
			localPosition.y -= currentDropSpeed * Time.deltaTime;
			if (localPosition.y <= dropTargetY)
			{
				localPosition.y = dropTargetY;
				currentDropSpeed = 0f;
			}
			base.transform.localPosition = localPosition;
		}
		else
		{
			float num = Time.time + timeOffset;
			Vector3 vector = new Vector3((Mathf.PerlinNoise(num * swayFrequency, 0f) - 0.5f) * 2f, 0f, (Mathf.PerlinNoise(num * swayFrequency, 1f) - 0.5f) * 2f) * swayAmplitude;
			Vector3 euler = new Vector3((Mathf.PerlinNoise(num * rotationFrequency, 2f) - 0.5f) * 2f, 0f, (Mathf.PerlinNoise(num * rotationFrequency, 3f) - 0.5f) * 2f) * rotationAmplitude;
			base.transform.localPosition = initialPosition + vector;
			base.transform.localRotation = initialRotation * Quaternion.Euler(euler);
		}
	}

	public void DropElevator()
	{
		isDropping = true;
		currentDropSpeed = dropSpeed;
	}
}
