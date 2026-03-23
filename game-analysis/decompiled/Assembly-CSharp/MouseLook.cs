using System;
using UnityEngine;

namespace AzureNature
{
	// Token: 0x0200010D RID: 269
	public class MouseLook : MonoBehaviour
	{
		// Token: 0x0600070D RID: 1805 RVA: 0x00023DEA File Offset: 0x00021FEA
		private void Awake()
		{
			this.LockCursor();
			this.xAxisClamp = 0f;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x00023DFD File Offset: 0x00021FFD
		private void LockCursor()
		{
			Cursor.lockState = CursorLockMode.Locked;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x00023E05 File Offset: 0x00022005
		private void Update()
		{
			this.CameraRotation();
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x00023E10 File Offset: 0x00022010
		private void CameraRotation()
		{
			float num = Input.GetAxis("Mouse X") * this.mouseSensitivity;
			float num2 = Input.GetAxis("Mouse Y") * this.mouseSensitivity;
			this.xAxisClamp += num2;
			if (this.xAxisClamp > 90f)
			{
				this.xAxisClamp = 90f;
				num2 = 0f;
				this.ClampXAxisRotationToValue(270f);
			}
			else if (this.xAxisClamp < -90f)
			{
				this.xAxisClamp = -90f;
				num2 = 0f;
				this.ClampXAxisRotationToValue(90f);
			}
			base.transform.Rotate(Vector3.left * num2);
			this.playerBody.Rotate(Vector3.up * num);
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x00023ED0 File Offset: 0x000220D0
		private void ClampXAxisRotationToValue(float value)
		{
			Vector3 eulerAngles = base.transform.eulerAngles;
			eulerAngles.x = value;
			base.transform.eulerAngles = eulerAngles;
		}

		// Token: 0x04000823 RID: 2083
		public float mouseSensitivity;

		// Token: 0x04000824 RID: 2084
		public Transform playerBody;

		// Token: 0x04000825 RID: 2085
		private float xAxisClamp;
	}
}
