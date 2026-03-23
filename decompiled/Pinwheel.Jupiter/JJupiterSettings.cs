using UnityEngine;

namespace Pinwheel.Jupiter;

public class JJupiterSettings : ScriptableObject
{
	private static JJupiterSettings instance;

	[SerializeField]
	private Material defaultSkybox;

	[SerializeField]
	private Texture2D noiseTexture;

	[SerializeField]
	private Texture2D cloudTexture;

	[SerializeField]
	private JSkyProfile defaultProfileSunnyDay;

	[SerializeField]
	private JSkyProfile defaultProfileStarryNight;

	[SerializeField]
	private JDayNightCycleProfile defaultDayNightCycleProfile;

	[SerializeField]
	private JInternalShaderSettings internalShaders;

	public static JJupiterSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load<JJupiterSettings>("JupiterSettings");
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<JJupiterSettings>();
				}
			}
			return instance;
		}
	}

	public Material DefaultSkybox
	{
		get
		{
			return defaultSkybox;
		}
		set
		{
			defaultSkybox = value;
		}
	}

	public Texture2D NoiseTexture
	{
		get
		{
			return noiseTexture;
		}
		set
		{
			noiseTexture = value;
		}
	}

	public Texture2D CloudTexture
	{
		get
		{
			return cloudTexture;
		}
		set
		{
			cloudTexture = value;
		}
	}

	public JSkyProfile DefaultProfileSunnyDay
	{
		get
		{
			return defaultProfileSunnyDay;
		}
		set
		{
			defaultProfileSunnyDay = value;
		}
	}

	public JSkyProfile DefaultProfileStarryNight
	{
		get
		{
			return defaultProfileStarryNight;
		}
		set
		{
			defaultProfileStarryNight = value;
		}
	}

	public JDayNightCycleProfile DefaultDayNightCycleProfile
	{
		get
		{
			return defaultDayNightCycleProfile;
		}
		set
		{
			defaultDayNightCycleProfile = value;
		}
	}

	public JInternalShaderSettings InternalShaders
	{
		get
		{
			return internalShaders;
		}
		set
		{
			internalShaders = value;
		}
	}
}
