using UnityEngine;

namespace AzureNature;

public class MouseLook : MonoBehaviour
{
	public float mouseSensitivity;

	public Transform playerBody;

	private float xAxisClamp;

	private void Awake()
	{
		LockCursor();
		xAxisClamp = 0f;
	}

	private void LockCursor()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		float num = Input.GetAxis("Mouse X") * mouseSensitivity;
		float num2 = Input.GetAxis("Mouse Y") * mouseSensitivity;
		xAxisClamp += num2;
		if (xAxisClamp > 90f)
		{
			xAxisClamp = 90f;
			num2 = 0f;
			ClampXAxisRotationToValue(270f);
		}
		else if (xAxisClamp < -90f)
		{
			xAxisClamp = -90f;
			num2 = 0f;
			ClampXAxisRotationToValue(90f);
		}
		base.transform.Rotate(Vector3.left * num2);
		playerBody.Rotate(Vector3.up * num);
	}

	private void ClampXAxisRotationToValue(float value)
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.x = value;
		base.transform.eulerAngles = eulerAngles;
	}
}
