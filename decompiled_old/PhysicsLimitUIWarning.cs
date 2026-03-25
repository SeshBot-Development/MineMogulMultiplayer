using UnityEngine;

public class PhysicsLimitUIWarning : MonoBehaviour
{
	[SerializeField]
	private GameObject _softLimitObject;

	[SerializeField]
	private GameObject _hardLimitObject;

	private static PhysicsLimitUIWarning _instance;

	protected virtual void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else if (_instance != this)
		{
			Debug.Log("PhysicsLimitUIWarning already exists, destroying duplicate: " + base.gameObject.name);
			Object.Destroy(base.gameObject);
			return;
		}
		SwitchState(OreLimitState.Regular);
	}

	public static void SwitchState(OreLimitState state)
	{
		if (_instance != null)
		{
			_instance.SwitchStateInternal(state);
		}
	}

	private void SwitchStateInternal(OreLimitState state)
	{
		switch (state)
		{
		case OreLimitState.Regular:
			base.gameObject.SetActive(value: false);
			break;
		case OreLimitState.SlightlyLimited:
		case OreLimitState.HighlyLimited:
			base.gameObject.SetActive(value: true);
			_softLimitObject.SetActive(value: true);
			_hardLimitObject.SetActive(value: false);
			break;
		case OreLimitState.Blocked:
			base.gameObject.SetActive(value: true);
			_softLimitObject.SetActive(value: false);
			_hardLimitObject.SetActive(value: true);
			break;
		}
	}
}
