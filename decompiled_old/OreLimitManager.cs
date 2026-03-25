using System.Collections.Generic;

public class OreLimitManager : Singleton<OreLimitManager>
{
	public OreLimitState OreLimitState;

	public bool HasShownOreLimitPopup;

	private TimeSince _timeSinceLastLimitCheck;

	private void OnEnable()
	{
		_timeSinceLastLimitCheck = -30f;
	}

	public bool ShouldBlockOreSpawning()
	{
		return OreLimitState == OreLimitState.Blocked;
	}

	public float GetAutoMinerSpawnTimeMultiplier()
	{
		return OreLimitState switch
		{
			OreLimitState.SlightlyLimited => 1.25f, 
			OreLimitState.HighlyLimited => 1.5f, 
			OreLimitState.Blocked => 2f, 
			_ => 1f, 
		};
	}

	public void OnObjectLimitChanged()
	{
		_timeSinceLastLimitCheck = 10f;
	}

	private void Update()
	{
		if (_timeSinceLastLimitCheck < 15)
		{
			return;
		}
		_timeSinceLastLimitCheck = 0f;
		int movingPhysicsObjectLimit = Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit;
		if (movingPhysicsObjectLimit >= 10000)
		{
			OreLimitState = OreLimitState.Regular;
			PhysicsLimitUIWarning.SwitchState(OreLimitState);
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
			OreLimitState = OreLimitState.Blocked;
			TryShowWarningPopup();
		}
		else if (num3 > num)
		{
			OreLimitState = OreLimitState.HighlyLimited;
			TryShowWarningPopup();
		}
		else if (num3 > movingPhysicsObjectLimit)
		{
			OreLimitState = OreLimitState.SlightlyLimited;
			TryShowWarningPopup();
		}
		else
		{
			OreLimitState = OreLimitState.Regular;
		}
		PhysicsLimitUIWarning.SwitchState(OreLimitState);
	}

	private void TryShowWarningPopup()
	{
		if (!HasShownOreLimitPopup)
		{
			HasShownOreLimitPopup = true;
			Singleton<UIManager>.Instance.ShowInfoMessagePopup("Ore Limit", $"Congratulations! Your factory has reached a very large scale, and you've reached the moving physics object limit.\nAuto-Miner production rate has been slowed to keep FPS high.\n \nIt's a good time to check if your factory has any spills or leaks, as that can increase moving object count. \n \nYou can raise/lower this limit in the settings menu.\n(current limit: {Singleton<SettingsManager>.Instance.MovingPhysicsObjectLimit} moving objects)");
		}
	}
}
