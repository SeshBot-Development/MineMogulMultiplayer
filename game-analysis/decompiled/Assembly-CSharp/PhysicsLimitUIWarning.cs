using System;
using UnityEngine;

// Token: 0x02000082 RID: 130
public class PhysicsLimitUIWarning : MonoBehaviour
{
	// Token: 0x06000382 RID: 898 RVA: 0x000115C0 File Offset: 0x0000F7C0
	protected virtual void Awake()
	{
		if (PhysicsLimitUIWarning._instance == null)
		{
			PhysicsLimitUIWarning._instance = this;
		}
		else if (PhysicsLimitUIWarning._instance != this)
		{
			Debug.Log("PhysicsLimitUIWarning already exists, destroying duplicate: " + base.gameObject.name);
			Object.Destroy(base.gameObject);
			return;
		}
		PhysicsLimitUIWarning.SwitchState(OreLimitState.Regular);
	}

	// Token: 0x06000383 RID: 899 RVA: 0x0001161B File Offset: 0x0000F81B
	public static void SwitchState(OreLimitState state)
	{
		if (PhysicsLimitUIWarning._instance != null)
		{
			PhysicsLimitUIWarning._instance.SwitchStateInternal(state);
		}
	}

	// Token: 0x06000384 RID: 900 RVA: 0x00011638 File Offset: 0x0000F838
	private void SwitchStateInternal(OreLimitState state)
	{
		switch (state)
		{
		case OreLimitState.Regular:
			base.gameObject.SetActive(false);
			return;
		case OreLimitState.SlightlyLimited:
		case OreLimitState.HighlyLimited:
			base.gameObject.SetActive(true);
			this._softLimitObject.SetActive(true);
			this._hardLimitObject.SetActive(false);
			return;
		case OreLimitState.Blocked:
			base.gameObject.SetActive(true);
			this._softLimitObject.SetActive(false);
			this._hardLimitObject.SetActive(true);
			return;
		default:
			return;
		}
	}

	// Token: 0x0400036F RID: 879
	[SerializeField]
	private GameObject _softLimitObject;

	// Token: 0x04000370 RID: 880
	[SerializeField]
	private GameObject _hardLimitObject;

	// Token: 0x04000371 RID: 881
	private static PhysicsLimitUIWarning _instance;
}
