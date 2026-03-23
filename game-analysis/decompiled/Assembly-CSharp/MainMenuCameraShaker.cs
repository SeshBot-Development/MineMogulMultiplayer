using System;
using UnityEngine;

// Token: 0x02000068 RID: 104
public class MainMenuCameraShaker : MonoBehaviour
{
	// Token: 0x060002BC RID: 700 RVA: 0x0000D272 File Offset: 0x0000B472
	private void Start()
	{
		this.initialPosition = base.transform.localPosition;
		this.initialRotation = base.transform.localRotation;
		this.timeOffset = Random.value * 100f;
	}

	// Token: 0x060002BD RID: 701 RVA: 0x0000D2A8 File Offset: 0x0000B4A8
	private void Update()
	{
		float num = Time.time + this.timeOffset;
		Vector3 vector = new Vector3((Mathf.PerlinNoise(num * this.positionFrequency, 0f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * this.positionFrequency, 1f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * this.positionFrequency, 2f) - 0.5f) * 2f) * this.positionAmplitude;
		Vector3 vector2 = new Vector3((Mathf.PerlinNoise(num * this.rotationFrequency, 3f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * this.rotationFrequency, 4f) - 0.5f) * 2f, (Mathf.PerlinNoise(num * this.rotationFrequency, 5f) - 0.5f) * 2f) * this.rotationAmplitude;
		this.currentPunchRotation = Vector3.SmoothDamp(this.currentPunchRotation, this.targetPunchRotation, ref this.punchVelocity, this.punchSmoothTime);
		this.targetPunchRotation = Vector3.Lerp(this.targetPunchRotation, Vector3.zero, Time.deltaTime * this.punchRecoverSpeed);
		base.transform.localPosition = this.initialPosition + vector;
		base.transform.localRotation = this.initialRotation * Quaternion.Euler(vector2 + this.currentPunchRotation);
	}

	// Token: 0x060002BE RID: 702 RVA: 0x0000D41B File Offset: 0x0000B61B
	public void ApplyViewPunch(Vector3 punch)
	{
		this.targetPunchRotation += punch;
	}

	// Token: 0x04000289 RID: 649
	public float positionAmplitude = 0.05f;

	// Token: 0x0400028A RID: 650
	public float rotationAmplitude = 0.2f;

	// Token: 0x0400028B RID: 651
	public float positionFrequency = 0.2f;

	// Token: 0x0400028C RID: 652
	public float rotationFrequency = 0.1f;

	// Token: 0x0400028D RID: 653
	private Vector3 initialPosition;

	// Token: 0x0400028E RID: 654
	private Quaternion initialRotation;

	// Token: 0x0400028F RID: 655
	private float timeOffset;

	// Token: 0x04000290 RID: 656
	private Vector3 currentPunchRotation = Vector3.zero;

	// Token: 0x04000291 RID: 657
	private Vector3 targetPunchRotation = Vector3.zero;

	// Token: 0x04000292 RID: 658
	private Vector3 punchVelocity = Vector3.zero;

	// Token: 0x04000293 RID: 659
	private float punchSmoothTime = 0.2f;

	// Token: 0x04000294 RID: 660
	private float punchRecoverSpeed = 4f;
}
