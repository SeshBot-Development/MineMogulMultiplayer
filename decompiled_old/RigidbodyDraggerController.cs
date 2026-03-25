using UnityEngine;

public class RigidbodyDraggerController : MonoBehaviour
{
	public PlayerController playerController;

	private void OnJointBreak(float breakForce)
	{
		if (playerController != null)
		{
			playerController.ReleaseObject();
		}
	}
}
