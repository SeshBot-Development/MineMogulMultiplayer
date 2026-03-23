using System;
using System.Collections.Generic;

// Token: 0x02000072 RID: 114
public class OreLimitManager : Singleton<OreLimitManager>
{
	// Token: 0x0600030C RID: 780 RVA: 0x0000F55C File Offset: 0x0000D75C
	private void OnEnable()
	{
		this._timeSinceLastLimitCheck = -30f;
	}

	// Token: 0x0600030D RID: 781 RVA: 0x0000F56E File Offset: 0x0000D76E
	public bool ShouldBlockOreSpawning()
	{
		return this.OreLimitState == OreLimitState.Blocked;
	}

	// Token: 0x0600030E RID: 782 RVA: 0x0000F57C File Offset: 0x0000D77C
	public float GetAutoMinerSpawnTimeMultiplier()
	{
		switch (this.OreLimitState)
		{
		case OreLimitState.SlightlyLimited:
			return 1.25f;
		case OreLimitState.HighlyLimited:
			return 1.5f;
		case OreLimitState.Blocked:
			return 2f;
		default:
			return 1f;
		}
	}

	// Token: 0x0600030F RID: 783 RVA: 0x0000F5BD File Offset: 0x0000D7BD
	public void OnObjectLimitChanged()
	{
		this._timeSinceLastLimitCheck = 10f;
	}

	// Token: 0x06000310 RID: 784 RVA: 0x0000F5D0 File Offset: 0x0000D7D0
	private void Update()
	{
		if ((in this._timeSinceLastLimitCheck) < 15)
		{
			return;
		}
		this._timeSinceLastLimitCheck = 0f;
		int movingPhysicsObjectLimit = Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit;
		if (movingPhysicsObjectLimit >= 10000)
		{
			this.OreLimitState = OreLimitState.Regular;
			PhysicsLimitUIWarning.SwitchState(this.OreLimitState);
			return;
		}
		int num = movingPhysicsObjectLimit + 100;
		int num2 = movingPhysicsObjectLimit + 200;
		int num3 = 0;
		List<OrePiece> allOrePieces = OrePiece.AllOrePieces;
		for (int i = 0; i < allOrePieces.Count; i++)
		{
			if (!allOrePieces[i].Rb.IsSleeping())
			{
				num3++;
				if (num3 > num2)
				{
					break;
				}
			}
		}
		if (num3 > num2)
		{
			this.OreLimitState = OreLimitState.Blocked;
			this.TryShowWarningPopup();
		}
		else if (num3 > num)
		{
			this.OreLimitState = OreLimitState.HighlyLimited;
			this.TryShowWarningPopup();
		}
		else if (num3 > movingPhysicsObjectLimit)
		{
			this.OreLimitState = OreLimitState.SlightlyLimited;
			this.TryShowWarningPopup();
		}
		else
		{
			this.OreLimitState = OreLimitState.Regular;
		}
		PhysicsLimitUIWarning.SwitchState(this.OreLimitState);
	}

	// Token: 0x06000311 RID: 785 RVA: 0x0000F6B7 File Offset: 0x0000D8B7
	private void TryShowWarningPopup()
	{
		if (this.HasShownOreLimitPopup)
		{
			return;
		}
		this.HasShownOreLimitPopup = true;
		Singleton<UIManager>.Instance.ShowInfoMessagePopup("Ore Limit", string.Format("Congratulations! Your factory has reached a very large scale, and you've reached the moving physics object limit.\nAuto-Miner production rate has been slowed to keep FPS high.\n \nIt's a good time to check if your factory has any spills or leaks, as that can increase moving object count. \n \nYou can raise/lower this limit in the settings menu.\n(current limit: {0} moving objects)", Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit));
	}

	// Token: 0x040002F4 RID: 756
	public OreLimitState OreLimitState;

	// Token: 0x040002F5 RID: 757
	public bool HasShownOreLimitPopup;

	// Token: 0x040002F6 RID: 758
	private TimeSince _timeSinceLastLimitCheck;
}
