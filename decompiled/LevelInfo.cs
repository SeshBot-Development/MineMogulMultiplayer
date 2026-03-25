using System;
using UnityEngine;

[Serializable]
public class LevelInfo
{
	public string LevelID;

	public string DisplayName;

	public string SceneName;

	[TextArea]
	public string Description = "The ultimate adventure of the game";

	public Texture Thumbnail;

	public bool ShouldAppearInMapSelect;

	public bool IsLockedInDemo = true;

	public bool ShouldForceSandboxGamemode;
}
