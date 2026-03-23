using System;
using UnityEngine;

namespace AzureNature
{
	// Token: 0x0200010E RID: 270
	public class MoveController : MonoBehaviour
	{
		// Token: 0x06000713 RID: 1811 RVA: 0x00023F05 File Offset: 0x00022105
		private void Awake()
		{
			this.characterController = base.GetComponent<CharacterController>();
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x00023F14 File Offset: 0x00022114
		private void Update()
		{
			if (this.characterController.isGrounded && this.velocity.y < 0f)
			{
				this.velocity.y = -2f;
			}
			float axis = Input.GetAxis("Horizontal");
			float axis2 = Input.GetAxis("Vertical");
			Vector3 vector = base.transform.right * axis + base.transform.forward * axis2;
			this.characterController.Move(vector * this.movementSpeed * Time.deltaTime);
			this.velocity.y = this.velocity.y + this.gravity * Time.deltaTime;
			this.characterController.Move(this.velocity * Time.deltaTime);
			if (Input.GetButton("Jump") && this.characterController.isGrounded)
			{
				this.velocity.y = Mathf.Sqrt(this.jumpSpeed * -2f * this.gravity);
			}
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.characterController.Move(vector * Time.deltaTime * this.runMultiplier);
			}
		}

		// Token: 0x04000826 RID: 2086
		public float movementSpeed;

		// Token: 0x04000827 RID: 2087
		public float jumpSpeed;

		// Token: 0x04000828 RID: 2088
		public float runMultiplier;

		// Token: 0x04000829 RID: 2089
		public float gravity = -9.81f;

		// Token: 0x0400082A RID: 2090
		private Vector3 velocity;

		// Token: 0x0400082B RID: 2091
		private CharacterController characterController;
	}
}
