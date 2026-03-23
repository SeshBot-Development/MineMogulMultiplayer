using ScreenSpaceCavityCurvature;
using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(SSCC))]
public class SaveFileScreenshotCamera : MonoBehaviour
{
	public Camera Camera { get; private set; }

	public SSCC SSCC { get; private set; }

	private void Awake()
	{
		Camera = GetComponent<Camera>();
		Camera.enabled = false;
		SSCC = GetComponent<SSCC>();
		SSCC.enabled = false;
	}
}
