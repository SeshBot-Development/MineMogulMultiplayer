using UnityEngine;

public class CoalGaugeNeedle : MonoBehaviour
{
	[Header("Needle angles (degrees)")]
	[SerializeField]
	private float _angleAtZero = 90f;

	[SerializeField]
	private float _angleAtMax = -90f;

	[Tooltip("Needle angle at the end of the red zone")]
	[SerializeField]
	private float _redZoneEndAngle = 50f;

	[Header("Rotation axis")]
	[SerializeField]
	private Vector3 _localAxis = Vector3.left;

	[Header("Smoothing")]
	[SerializeField]
	private float _smoothSpeed = 10f;

	[SerializeField]
	private CastingFurnace _castingFurnace;

	private float _targetAngle;

	public void SetCoal(float coalAmount)
	{
		if (coalAmount == 0f)
		{
			_targetAngle = _angleAtZero;
			return;
		}
		float num = 100f;
		float requiredCoalForSteel = _castingFurnace.GetRequiredCoalForSteel();
		coalAmount = Mathf.Clamp(coalAmount, 0f, num);
		if (coalAmount <= requiredCoalForSteel)
		{
			float t = coalAmount / requiredCoalForSteel;
			_targetAngle = Mathf.Lerp(_angleAtZero, _redZoneEndAngle, t);
		}
		else
		{
			float t2 = Mathf.InverseLerp(requiredCoalForSteel, num, coalAmount);
			_targetAngle = Mathf.Lerp(_redZoneEndAngle, _angleAtMax, t2);
		}
	}

	private void Update()
	{
		SetCoal(_castingFurnace.CoalAmount);
		Quaternion quaternion = Quaternion.AngleAxis(_targetAngle, _localAxis);
		if (_smoothSpeed <= 0f)
		{
			base.transform.localRotation = quaternion;
		}
		else
		{
			base.transform.localRotation = Quaternion.Slerp(base.transform.localRotation, quaternion, Time.deltaTime * _smoothSpeed);
		}
	}
}
