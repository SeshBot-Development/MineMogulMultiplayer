using System;
using UnityEngine;

// Token: 0x02000069 RID: 105
public class MainMenuElevator : MonoBehaviour
{
	// Token: 0x060002C0 RID: 704 RVA: 0x0000D4A6 File Offset: 0x0000B6A6
	private void Start()
	{
		this.initialPosition = base.transform.localPosition;
		this.initialRotation = base.transform.localRotation;
		this.timeOffset = Random.value * 100f;
	}

	// Token: 0x060002C1 RID: 705 RVA: 0x0000D4DC File Offset: 0x0000B6DC
	private void Update()
	{
		Input.GetKeyDown(KeyCode.Space);
		if (this.isDropping)
		{
			this.currentDropSpeed += this.dropAcceleration * Time.deltaTime;
			Vector3 localPosition = base.transform.localPosition;
			localPosition.y -= this.currentDropSpeed * Time.deltaTime;
			if (localPosition.y <= this.dropTargetY)
			{
				localPosition.y = this.dropTargetY;
				this.currentDropSpeed = 0f;
			}
			base.transform.localPosition = localPosition;
			return;
		}
		float num = Time.time + this.timeOffset;
		Vector3 vector = new Vector3((Mathf.PerlinNoise(num * this.swayFrequency, 0f) - 0.5f) * 2f, 0f, (Mathf.PerlinNoise(num * this.swayFrequency, 1f) - 0.5f) * 2f) * this.swayAmplitude;
		Vector3 vector2 = new Vector3((Mathf.PerlinNoise(num * this.rotationFrequency, 2f) - 0.5f) * 2f, 0f, (Mathf.PerlinNoise(num * this.rotationFrequency, 3f) - 0.5f) * 2f) * this.rotationAmplitude;
		base.transform.localPosition = this.initialPosition + vector;
		base.transform.localRotation = this.initialRotation * Quaternion.Euler(vector2);
	}

	// Token: 0x060002C2 RID: 706 RVA: 0x0000D64C File Offset: 0x0000B84C
	public void DropElevator()
	{
		this.isDropping = true;
		this.currentDropSpeed = this.dropSpeed;
	}

	// Token: 0x04000295 RID: 661
	[Header("Idle Sway Settings")]
	public float swayAmplitude = 0.05f;

	// Token: 0x04000296 RID: 662
	public float swayFrequency = 0.1f;

	// Token: 0x04000297 RID: 663
	public float rotationAmplitude = 1.5f;

	// Token: 0x04000298 RID: 664
	public float rotationFrequency = 0.2f;

	// Token: 0x04000299 RID: 665
	[Header("Drop Settings")]
	public float dropSpeed = 20f;

	// Token: 0x0400029A RID: 666
	public float dropAcceleration = 60f;

	// Token: 0x0400029B RID: 667
	public float dropTargetY = -50f;

	// Token: 0x0400029C RID: 668
	private Vector3 initialPosition;

	// Token: 0x0400029D RID: 669
	private Quaternion initialRotation;

	// Token: 0x0400029E RID: 670
	private float timeOffset;

	// Token: 0x0400029F RID: 671
	private bool isDropping;

	// Token: 0x040002A0 RID: 672
	private float currentDropSpeed;
}
