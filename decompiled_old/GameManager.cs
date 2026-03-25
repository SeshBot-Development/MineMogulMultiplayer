using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : Singleton<GameManager>
{
	public event Action GamePaused;

	public event Action GameUnpaused;

	public void OnGamePauseToggled(bool isPaused)
	{
		if (isPaused)
		{
			this.GamePaused?.Invoke();
		}
		else
		{
			this.GameUnpaused?.Invoke();
		}
	}
}
