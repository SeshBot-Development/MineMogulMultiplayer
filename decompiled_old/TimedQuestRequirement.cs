using System;
using UnityEngine;

[Serializable]
public class TimedQuestRequirement : QuestRequirement
{
	public float DurationSeconds = 10f;

	[NonSerialized]
	private float _timeStarted = -1f;

	[NonSerialized]
	private bool _completed;

	public override string GetRequirementText()
	{
		if (_timeStarted < 0f)
		{
			return $"Wait {DurationSeconds:0} seconds...";
		}
		float num = Mathf.Max(0f, DurationSeconds - (Time.time - _timeStarted));
		if (!_completed)
		{
			return $"Waiting... ({num:0.0}s left)";
		}
		return "Wait complete!";
	}

	public override bool IsCompleted()
	{
		if (_completed)
		{
			return true;
		}
		if (_timeStarted < 0f)
		{
			_timeStarted = Time.time;
			return false;
		}
		if (Time.time - _timeStarted >= DurationSeconds)
		{
			_completed = true;
		}
		return _completed;
	}

	public override QuestRequirement Clone()
	{
		return new TimedQuestRequirement
		{
			DurationSeconds = DurationSeconds,
			IsHidden = IsHidden,
			UnlocksHiddenQuest = UnlocksHiddenQuest
		};
	}
}
