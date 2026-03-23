using System;
using UnityEngine;

// Token: 0x02000091 RID: 145
[Serializable]
public class TimedQuestRequirement : QuestRequirement
{
	// Token: 0x060003F5 RID: 1013 RVA: 0x000157C8 File Offset: 0x000139C8
	public override string GetRequirementText()
	{
		if (this._timeStarted < 0f)
		{
			return string.Format("Wait {0:0} seconds...", this.DurationSeconds);
		}
		float num = Mathf.Max(0f, this.DurationSeconds - (Time.time - this._timeStarted));
		if (!this._completed)
		{
			return string.Format("Waiting... ({0:0.0}s left)", num);
		}
		return "Wait complete!";
	}

	// Token: 0x060003F6 RID: 1014 RVA: 0x00015834 File Offset: 0x00013A34
	public override bool IsCompleted()
	{
		if (this._completed)
		{
			return true;
		}
		if (this._timeStarted < 0f)
		{
			this._timeStarted = Time.time;
			return false;
		}
		if (Time.time - this._timeStarted >= this.DurationSeconds)
		{
			this._completed = true;
		}
		return this._completed;
	}

	// Token: 0x060003F7 RID: 1015 RVA: 0x00015886 File Offset: 0x00013A86
	public override QuestRequirement Clone()
	{
		return new TimedQuestRequirement
		{
			DurationSeconds = this.DurationSeconds,
			IsHidden = this.IsHidden,
			UnlocksHiddenQuest = this.UnlocksHiddenQuest
		};
	}

	// Token: 0x04000443 RID: 1091
	public float DurationSeconds = 10f;

	// Token: 0x04000444 RID: 1092
	[NonSerialized]
	private float _timeStarted = -1f;

	// Token: 0x04000445 RID: 1093
	[NonSerialized]
	private bool _completed;
}
