using UnityEngine;

namespace AzureNature;

public class MoveController : MonoBehaviour
{
	public float movementSpeed;

	public float jumpSpeed;

	public float runMultiplier;

	public float gravity = -9.81f;

	private Vector3 velocity;

	private CharacterController characterController;

	private void Awake()
	{
		characterController = GetComponent<CharacterController>();
	}

	private void Update()
	{
		if (characterController.isGrounded && velocity.y < 0f)
		{
			velocity.y = -2f;
		}
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 vector = base.transform.right * axis + base.transform.forward * axis2;
		characterController.Move(vector * movementSpeed * Time.deltaTime);
		velocity.y += gravity * Time.deltaTime;
		characterController.Move(velocity * Time.deltaTime);
		if (Input.GetButton("Jump") && characterController.isGrounded)
		{
			velocity.y = Mathf.Sqrt(jumpSpeed * -2f * gravity);
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			characterController.Move(vector * Time.deltaTime * runMultiplier);
		}
	}
}
