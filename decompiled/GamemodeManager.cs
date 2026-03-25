using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GamemodeManager : Singleton<GamemodeManager>
{
	public GameModeType GameModeType;

	public bool ShouldUseFreeShop()
	{
		return GameModeType == GameModeType.Sandbox;
	}

	public bool ShouldAllowNoclip()
	{
		if (GameModeType != GameModeType.Sandbox)
		{
			return Singleton<DebugManager>.Instance.DevModeEnabled;
		}
		return true;
	}

	public bool ShouldShowSandboxShopTab()
	{
		return GameModeType == GameModeType.Sandbox;
	}

	public bool ShouldDisableQuests()
	{
		return GameModeType == GameModeType.Sandbox;
	}

	public string GetFormattedGamemodeName(GameModeType gameMode)
	{
		return gameMode switch
		{
			GameModeType.Standard => "Standard", 
			GameModeType.Sandbox => "Sandbox", 
			_ => "Unknown", 
		};
	}

	public string GetColoredFormattedGamemodeName(GameModeType gameMode)
	{
		string formattedGamemodeName = GetFormattedGamemodeName(gameMode);
		if (gameMode == GameModeType.Sandbox)
		{
			return "<color=#75aaff>" + formattedGamemodeName + "</color>";
		}
		return formattedGamemodeName;
	}
}
