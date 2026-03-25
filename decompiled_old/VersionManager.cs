using UnityEngine;

[DefaultExecutionOrder(-10000)]
public class VersionManager : Singleton<VersionManager>
{
	public string VersionNumber;

	public string GetFormattedVersionText()
	{
		return "Version: " + VersionNumber;
	}

	public string GetVersionTextWithoutLabel()
	{
		return VersionNumber;
	}
}
