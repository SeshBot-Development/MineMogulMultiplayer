using System;
using UnityEngine;

// Token: 0x02000028 RID: 40
public class CoalGaugeNeedle : MonoBehaviour
{
	// Token: 0x06000132 RID: 306 RVA: 0x00007408 File Offset: 0x00005608
	public void SetCoal(float coalAmount)
	{
		if (coalAmount == 0f)
		{
			this._targetAngle = this._angleAtZero;
			return;
		}
		float num = 100f;
		float requiredCoalForSteel = this._castingFurnace.GetRequiredCoalForSteel();
		coalAmount = Mathf.Clamp(coalAmount, 0f, num);
		if (coalAmount <= requiredCoalForSteel)
		{
			float num2 = coalAmount / requiredCoalForSteel;
			this._targetAngle = Mathf.Lerp(this._angleAtZero, this._redZoneEndAngle, num2);
			return;
		}
		float num3 = Mathf.InverseLerp(requiredCoalForSteel, num, coalAmount);
		this._targetAngle = Mathf.Lerp(this._redZoneEndAngle, this._angleAtMax, num3);
	}

	// Token: 0x06000133 RID: 307 RVA: 0x0000748C File Offset: 0x0000568C
	private void Update()
	{
		this.SetCoal(this._castingFurnace.CoalAmount);
		Quaternion quaternion = Quaternion.AngleAxis(this._targetAngle, this._localAxis);
		if (this._smoothSpeed <= 0f)
		{
			base.transform.localRotation = quaternion;
			return;
		}
		base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, quaternion, Time.deltaTime * this._smoothSpeed);
	}

	// Token: 0x0400012A RID: 298
	[Header("Needle angles (degrees)")]
	[SerializeField]
	private float _angleAtZero = 90f;

	// Token: 0x0400012B RID: 299
	[SerializeField]
	private float _angleAtMax = -90f;

	// Token: 0x0400012C RID: 300
	[Tooltip("Needle angle at the end of the red zone")]
	[SerializeField]
	private float _redZoneEndAngle = 50f;

	// Token: 0x0400012D RID: 301
	[Header("Rotation axis")]
	[SerializeField]
	private Vector3 _localAxis = Vector3.left;

	// Token: 0x0400012E RID: 302
	[Header("Smoothing")]
	[SerializeField]
	private float _smoothSpeed = 10f;

	// Token: 0x0400012F RID: 303
	[SerializeField]
	private CastingFurnace _castingFurnace;

	// Token: 0x04000130 RID: 304
	private float _targetAngle;
}
