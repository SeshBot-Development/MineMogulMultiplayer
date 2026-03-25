using UnityEngine;

namespace Pinwheel.Jupiter;

[CreateAssetMenu(menuName = "Jupiter/Sky Profile")]
public class JSkyProfile : ScriptableObject
{
	[SerializeField]
	private Color skyColor;

	[SerializeField]
	private Color horizonColor;

	[SerializeField]
	private Color groundColor;

	[SerializeField]
	private float horizonThickness;

	[SerializeField]
	private float horizonExponent;

	[SerializeField]
	private int horizonStep;

	[SerializeField]
	private JFogSyncOption fogSyncOption;

	[SerializeField]
	private Color fogColor;

	[SerializeField]
	private bool enableStars;

	[SerializeField]
	private float starsStartPosition;

	[SerializeField]
	private float starsEndPosition;

	[SerializeField]
	private float starsOpacity;

	[SerializeField]
	private int starsLayerCount;

	[SerializeField]
	private Color starsColor0;

	[SerializeField]
	private Color starsColor1;

	[SerializeField]
	private Color starsColor2;

	[SerializeField]
	private float starsDensity0;

	[SerializeField]
	private float starsDensity1;

	[SerializeField]
	private float starsDensity2;

	[SerializeField]
	private float starsSize0;

	[SerializeField]
	private float starsSize1;

	[SerializeField]
	private float starsSize2;

	[SerializeField]
	private float starsGlow0;

	[SerializeField]
	private float starsGlow1;

	[SerializeField]
	private float starsGlow2;

	[SerializeField]
	private float starsTwinkle0;

	[SerializeField]
	private float starsTwinkle1;

	[SerializeField]
	private float starsTwinkle2;

	[SerializeField]
	private bool useBakedStars;

	[SerializeField]
	private Cubemap starsCubemap;

	[SerializeField]
	private Texture2D starsTwinkleMap;

	[SerializeField]
	private bool enableSun;

	[SerializeField]
	private Texture2D sunTexture;

	[SerializeField]
	private Color sunColor;

	[SerializeField]
	private float sunSize;

	[SerializeField]
	private float sunSoftEdge;

	[SerializeField]
	private float sunGlow;

	[SerializeField]
	private bool useBakedSun;

	[SerializeField]
	private Cubemap sunCubemap;

	[SerializeField]
	private Color sunLightColor;

	[SerializeField]
	private float sunLightIntensity;

	[SerializeField]
	private bool enableMoon;

	[SerializeField]
	private Texture2D moonTexture;

	[SerializeField]
	private Color moonColor;

	[SerializeField]
	private float moonSize;

	[SerializeField]
	private float moonSoftEdge;

	[SerializeField]
	private float moonGlow;

	[SerializeField]
	private bool useBakedMoon;

	[SerializeField]
	private Cubemap moonCubemap;

	[SerializeField]
	private Color moonLightColor;

	[SerializeField]
	private float moonLightIntensity;

	[SerializeField]
	private Texture2D customCloudTexture;

	[SerializeField]
	private bool enableHorizonCloud;

	[SerializeField]
	private Color horizonCloudColor;

	[SerializeField]
	private float horizonCloudStartPosition;

	[SerializeField]
	private float horizonCloudEndPosition;

	[SerializeField]
	private float horizonCloudSize;

	[SerializeField]
	private int horizonCloudStep;

	[SerializeField]
	private float horizonCloudAnimationSpeed;

	[SerializeField]
	private bool enableOverheadCloud;

	[SerializeField]
	private Color overheadCloudColor;

	[SerializeField]
	private float overheadCloudAltitude;

	[SerializeField]
	private float overheadCloudSize;

	[SerializeField]
	private int overheadCloudStep;

	[SerializeField]
	private float overheadCloudAnimationSpeed;

	[SerializeField]
	private float overheadCloudFlowSpeed;

	[SerializeField]
	private float overheadCloudFlowDirectionX;

	[SerializeField]
	private float overheadCloudFlowDirectionZ;

	[SerializeField]
	private float overheadCloudRemapMin;

	[SerializeField]
	private float overheadCloudRemapMax;

	[SerializeField]
	private bool overheadCloudCastShadow;

	[SerializeField]
	private float overheadCloudShadowClipMask;

	[SerializeField]
	private bool enableDetailOverlay;

	[SerializeField]
	private Color detailOverlayTintColor;

	[SerializeField]
	private Cubemap detailOverlayCubeMap;

	[SerializeField]
	private JDetailOverlayLayer detailOverlayLayer;

	[SerializeField]
	private float detailOverlayRotationSpeed;

	[SerializeField]
	private bool allowStepEffect;

	[SerializeField]
	private Material material;

	[SerializeField]
	private Material shadowMaterial;

	[JAnimatable("Sky/Sky Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color SkyColor
	{
		get
		{
			return skyColor;
		}
		set
		{
			skyColor = value;
		}
	}

	[JAnimatable("Sky/Horizon Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color HorizonColor
	{
		get
		{
			return horizonColor;
		}
		set
		{
			horizonColor = value;
		}
	}

	[JAnimatable("Sky/Ground Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color GroundColor
	{
		get
		{
			return groundColor;
		}
		set
		{
			groundColor = value;
		}
	}

	[JAnimatable("Sky/Horizon Thickness", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonThickness
	{
		get
		{
			return horizonThickness;
		}
		set
		{
			horizonThickness = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Sky/Horizon Exponent", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonExponent
	{
		get
		{
			return horizonExponent;
		}
		set
		{
			horizonExponent = Mathf.Max(0.01f, value);
		}
	}

	public int HorizonStep
	{
		get
		{
			return horizonStep;
		}
		set
		{
			horizonStep = Mathf.Max(1, value);
		}
	}

	public JFogSyncOption FogSyncOption
	{
		get
		{
			return fogSyncOption;
		}
		set
		{
			fogSyncOption = value;
		}
	}

	[JAnimatable("Sky/Fog Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color FogColor
	{
		get
		{
			return fogColor;
		}
		set
		{
			fogColor = value;
		}
	}

	[JAnimatable("Ambient/Ambient Light", JCurveOrGradient.Gradient, JAnimateTarget.SkyProfile)]
	public Color AmbientLight
	{
		get
		{
			return RenderSettings.ambientLight;
		}
		set
		{
			Debug.Log("A");
			RenderSettings.ambientLight = value;
		}
	}

	[JAnimatable("Ambient/Ambient Sky Color", JCurveOrGradient.Gradient, JAnimateTarget.SkyProfile)]
	public Color AmbientSkyColor
	{
		get
		{
			return RenderSettings.ambientSkyColor;
		}
		set
		{
			RenderSettings.ambientSkyColor = value;
		}
	}

	[JAnimatable("Ambient/Ambient Equator Color", JCurveOrGradient.Gradient, JAnimateTarget.SkyProfile)]
	public Color AmbientEquatorColor
	{
		get
		{
			return RenderSettings.ambientEquatorColor;
		}
		set
		{
			RenderSettings.ambientEquatorColor = value;
		}
	}

	[JAnimatable("Ambient/Ambient Ground Color", JCurveOrGradient.Gradient, JAnimateTarget.SkyProfile)]
	public Color AmbientGroundColor
	{
		get
		{
			return RenderSettings.ambientGroundColor;
		}
		set
		{
			Debug.Log("A");
			RenderSettings.ambientGroundColor = value;
		}
	}

	[JAnimatable("Ambient/Ambient Skybox Intensity", JCurveOrGradient.Curve, JAnimateTarget.SkyProfile)]
	public float AmbientSkyboxIntensity
	{
		get
		{
			return RenderSettings.ambientIntensity;
		}
		set
		{
			RenderSettings.ambientIntensity = value;
		}
	}

	public bool EnableStars
	{
		get
		{
			return enableStars;
		}
		set
		{
			enableStars = value;
		}
	}

	[JAnimatable("Stars/Start Position", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsStartPosition
	{
		get
		{
			return starsStartPosition;
		}
		set
		{
			starsStartPosition = Mathf.Min(value, starsEndPosition - 0.01f);
		}
	}

	[JAnimatable("Stars/End Position", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsEndPosition
	{
		get
		{
			return starsEndPosition;
		}
		set
		{
			starsEndPosition = Mathf.Max(value, starsStartPosition + 0.01f);
		}
	}

	[JAnimatable("Stars/Opacity", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsOpacity
	{
		get
		{
			return starsOpacity;
		}
		set
		{
			starsOpacity = Mathf.Clamp01(value);
		}
	}

	public int StarsLayerCount
	{
		get
		{
			return starsLayerCount;
		}
		set
		{
			starsLayerCount = Mathf.Clamp(value, 1, 3);
		}
	}

	[JAnimatable("Stars/Color 0", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color StarsColor0
	{
		get
		{
			return starsColor0;
		}
		set
		{
			starsColor0 = value;
		}
	}

	[JAnimatable("Stars/Color 1", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color StarsColor1
	{
		get
		{
			return starsColor1;
		}
		set
		{
			starsColor1 = value;
		}
	}

	[JAnimatable("Stars/Color 2", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color StarsColor2
	{
		get
		{
			return starsColor2;
		}
		set
		{
			starsColor2 = value;
		}
	}

	[JAnimatable("Stars/Density 0", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsDensity0
	{
		get
		{
			return starsDensity0;
		}
		set
		{
			starsDensity0 = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Stars/Density 1", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsDensity1
	{
		get
		{
			return starsDensity1;
		}
		set
		{
			starsDensity1 = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Stars/Density 2", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsDensity2
	{
		get
		{
			return starsDensity2;
		}
		set
		{
			starsDensity2 = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Stars/Size 0", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsSize0
	{
		get
		{
			return starsSize0;
		}
		set
		{
			starsSize0 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Size 1", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsSize1
	{
		get
		{
			return starsSize1;
		}
		set
		{
			starsSize1 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Size 2", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsSize2
	{
		get
		{
			return starsSize2;
		}
		set
		{
			starsSize2 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Glow 0", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsGlow0
	{
		get
		{
			return starsGlow0;
		}
		set
		{
			starsGlow0 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Glow 1", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsGlow1
	{
		get
		{
			return starsGlow1;
		}
		set
		{
			starsGlow1 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Glow 2", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsGlow2
	{
		get
		{
			return starsGlow2;
		}
		set
		{
			starsGlow2 = Mathf.Max(0.01f, value);
		}
	}

	[JAnimatable("Stars/Twinkle 0", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsTwinkle0
	{
		get
		{
			return starsTwinkle0;
		}
		set
		{
			starsTwinkle0 = value;
		}
	}

	[JAnimatable("Stars/Twinkle 1", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsTwinkle1
	{
		get
		{
			return starsTwinkle1;
		}
		set
		{
			starsTwinkle1 = value;
		}
	}

	[JAnimatable("Stars/Twinkle 2", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float StarsTwinkle2
	{
		get
		{
			return starsTwinkle2;
		}
		set
		{
			starsTwinkle2 = value;
		}
	}

	public bool UseBakedStars
	{
		get
		{
			return useBakedStars;
		}
		set
		{
			useBakedStars = value;
		}
	}

	public Cubemap StarsCubemap
	{
		get
		{
			return starsCubemap;
		}
		set
		{
			starsCubemap = value;
		}
	}

	public Texture2D StarsTwinkleMap
	{
		get
		{
			return starsTwinkleMap;
		}
		set
		{
			starsTwinkleMap = value;
		}
	}

	public bool EnableSun
	{
		get
		{
			return enableSun;
		}
		set
		{
			enableSun = value;
		}
	}

	public Texture2D SunTexture
	{
		get
		{
			return sunTexture;
		}
		set
		{
			sunTexture = value;
		}
	}

	[JAnimatable("Sun/Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color SunColor
	{
		get
		{
			return sunColor;
		}
		set
		{
			sunColor = value;
		}
	}

	[JAnimatable("Sun/Size", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float SunSize
	{
		get
		{
			return sunSize;
		}
		set
		{
			sunSize = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Sun/Soft Edge", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float SunSoftEdge
	{
		get
		{
			return sunSoftEdge;
		}
		set
		{
			sunSoftEdge = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Sun/Glow", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float SunGlow
	{
		get
		{
			return sunGlow;
		}
		set
		{
			sunGlow = Mathf.Clamp01(value);
		}
	}

	public bool UseBakedSun
	{
		get
		{
			return useBakedSun;
		}
		set
		{
			useBakedSun = value;
		}
	}

	public Cubemap SunCubemap
	{
		get
		{
			return sunCubemap;
		}
		set
		{
			sunCubemap = value;
		}
	}

	[JAnimatable("Sun/Light Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color SunLightColor
	{
		get
		{
			return sunLightColor;
		}
		set
		{
			sunLightColor = value;
		}
	}

	[JAnimatable("Sun/Light Intensity", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float SunLightIntensity
	{
		get
		{
			return sunLightIntensity;
		}
		set
		{
			sunLightIntensity = value;
		}
	}

	public bool EnableMoon
	{
		get
		{
			return enableMoon;
		}
		set
		{
			enableMoon = value;
		}
	}

	public Texture2D MoonTexture
	{
		get
		{
			return moonTexture;
		}
		set
		{
			moonTexture = value;
		}
	}

	[JAnimatable("Moon/Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color MoonColor
	{
		get
		{
			return moonColor;
		}
		set
		{
			moonColor = value;
		}
	}

	[JAnimatable("Moon/Size", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float MoonSize
	{
		get
		{
			return moonSize;
		}
		set
		{
			moonSize = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Moon/Soft Edge", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float MoonSoftEdge
	{
		get
		{
			return moonSoftEdge;
		}
		set
		{
			moonSoftEdge = Mathf.Clamp01(value);
		}
	}

	[JAnimatable("Moon/Glow", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float MoonGlow
	{
		get
		{
			return moonGlow;
		}
		set
		{
			moonGlow = Mathf.Clamp01(value);
		}
	}

	public bool UseBakedMoon
	{
		get
		{
			return useBakedMoon;
		}
		set
		{
			useBakedMoon = value;
		}
	}

	public Cubemap MoonCubemap
	{
		get
		{
			return moonCubemap;
		}
		set
		{
			moonCubemap = value;
		}
	}

	[JAnimatable("Moon/Light Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color MoonLightColor
	{
		get
		{
			return moonLightColor;
		}
		set
		{
			moonLightColor = value;
		}
	}

	[JAnimatable("Moon/Light Intensity", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float MoonLightIntensity
	{
		get
		{
			return moonLightIntensity;
		}
		set
		{
			moonLightIntensity = value;
		}
	}

	public Texture2D CustomCloudTexture
	{
		get
		{
			return customCloudTexture;
		}
		set
		{
			customCloudTexture = value;
		}
	}

	public bool EnableHorizonCloud
	{
		get
		{
			return enableHorizonCloud;
		}
		set
		{
			enableHorizonCloud = value;
		}
	}

	[JAnimatable("Horizon Cloud/Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color HorizonCloudColor
	{
		get
		{
			return horizonCloudColor;
		}
		set
		{
			horizonCloudColor = value;
		}
	}

	[JAnimatable("Horizon Cloud/Start Position", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonCloudStartPosition
	{
		get
		{
			return horizonCloudStartPosition;
		}
		set
		{
			horizonCloudStartPosition = Mathf.Min(value, -0.01f);
		}
	}

	[JAnimatable("Horizon Cloud/End Position", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonCloudEndPosition
	{
		get
		{
			return horizonCloudEndPosition;
		}
		set
		{
			horizonCloudEndPosition = Mathf.Max(value, 0.01f);
		}
	}

	[JAnimatable("Horizon Cloud/Size", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonCloudSize
	{
		get
		{
			return horizonCloudSize;
		}
		set
		{
			horizonCloudSize = Mathf.Max(0f, value);
		}
	}

	public int HorizonCloudStep
	{
		get
		{
			return horizonCloudStep;
		}
		set
		{
			horizonCloudStep = Mathf.Max(1, value);
		}
	}

	[JAnimatable("Horizon Cloud/Animation Speed", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float HorizonCloudAnimationSpeed
	{
		get
		{
			return horizonCloudAnimationSpeed;
		}
		set
		{
			horizonCloudAnimationSpeed = value;
		}
	}

	public bool EnableOverheadCloud
	{
		get
		{
			return enableOverheadCloud;
		}
		set
		{
			enableOverheadCloud = value;
		}
	}

	[JAnimatable("Overhead Cloud/Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color OverheadCloudColor
	{
		get
		{
			return overheadCloudColor;
		}
		set
		{
			overheadCloudColor = value;
		}
	}

	[JAnimatable("Overhead Cloud/Altitude", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudAltitude
	{
		get
		{
			return overheadCloudAltitude;
		}
		set
		{
			overheadCloudAltitude = Mathf.Max(0f, value);
		}
	}

	[JAnimatable("Overhead Cloud/Size", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudSize
	{
		get
		{
			return overheadCloudSize;
		}
		set
		{
			overheadCloudSize = Mathf.Max(0f, value);
		}
	}

	public int OverheadCloudStep
	{
		get
		{
			return overheadCloudStep;
		}
		set
		{
			overheadCloudStep = Mathf.Max(1, value);
		}
	}

	[JAnimatable("Overhead Cloud/Animation Speed", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudAnimationSpeed
	{
		get
		{
			return overheadCloudAnimationSpeed;
		}
		set
		{
			overheadCloudAnimationSpeed = value;
		}
	}

	[JAnimatable("Overhead Cloud/Flow Speed", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudFlowSpeed
	{
		get
		{
			return overheadCloudFlowSpeed;
		}
		set
		{
			overheadCloudFlowSpeed = value;
		}
	}

	[JAnimatable("Overhead Cloud/Flow X", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudFlowDirectionX
	{
		get
		{
			return overheadCloudFlowDirectionX;
		}
		set
		{
			overheadCloudFlowDirectionX = Mathf.Clamp(value, -1f, 1f);
		}
	}

	[JAnimatable("Overhead Cloud/Flow Z", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudFlowDirectionZ
	{
		get
		{
			return overheadCloudFlowDirectionZ;
		}
		set
		{
			overheadCloudFlowDirectionZ = Mathf.Clamp(value, -1f, 1f);
		}
	}

	[JAnimatable("Overhead Cloud/Remap Min", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudRemapMin
	{
		get
		{
			return overheadCloudRemapMin;
		}
		set
		{
			overheadCloudRemapMin = value;
		}
	}

	[JAnimatable("Overhead Cloud/Remap Max", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float OverheadCloudRemapMax
	{
		get
		{
			return overheadCloudRemapMax;
		}
		set
		{
			overheadCloudRemapMax = value;
		}
	}

	public bool OverheadCloudCastShadow
	{
		get
		{
			return overheadCloudCastShadow;
		}
		set
		{
			overheadCloudCastShadow = value;
		}
	}

	public float OverheadCloudShadowClipMask
	{
		get
		{
			return overheadCloudShadowClipMask;
		}
		set
		{
			overheadCloudShadowClipMask = value;
		}
	}

	public bool EnableDetailOverlay
	{
		get
		{
			return enableDetailOverlay;
		}
		set
		{
			enableDetailOverlay = value;
		}
	}

	[JAnimatable("Detail Overlay/Color", JCurveOrGradient.Gradient, JAnimateTarget.Material)]
	public Color DetailOverlayTintColor
	{
		get
		{
			return detailOverlayTintColor;
		}
		set
		{
			detailOverlayTintColor = value;
		}
	}

	public Cubemap DetailOverlayCubeMap
	{
		get
		{
			return detailOverlayCubeMap;
		}
		set
		{
			detailOverlayCubeMap = value;
		}
	}

	public JDetailOverlayLayer DetailOverlayLayer
	{
		get
		{
			return detailOverlayLayer;
		}
		set
		{
			detailOverlayLayer = value;
		}
	}

	[JAnimatable("Detail Overlay/Rotation Speed", JCurveOrGradient.Curve, JAnimateTarget.Material)]
	public float DetailOverlayRotationSpeed
	{
		get
		{
			return detailOverlayRotationSpeed;
		}
		set
		{
			detailOverlayRotationSpeed = value;
		}
	}

	public bool AllowStepEffect
	{
		get
		{
			return allowStepEffect;
		}
		set
		{
			allowStepEffect = value;
		}
	}

	public Material Material
	{
		get
		{
			if (material == null)
			{
				material = new Material(JJupiterSettings.Instance.InternalShaders.SkyShader);
			}
			material.name = material.shader.name;
			return material;
		}
	}

	public Material ShadowMaterial
	{
		get
		{
			if (shadowMaterial == null)
			{
				shadowMaterial = new Material(JJupiterSettings.Instance.InternalShaders.SkyShadowShader);
			}
			shadowMaterial.name = shadowMaterial.shader.name;
			return shadowMaterial;
		}
	}

	public void Reset()
	{
		JSkyProfile defaultProfileSunnyDay = JJupiterSettings.Instance.DefaultProfileSunnyDay;
		if (defaultProfileSunnyDay != this)
		{
			CopyFrom(defaultProfileSunnyDay);
		}
	}

	public void UpdateMaterialProperties(Material mat)
	{
		JMat.SetActiveMaterial(mat);
		JMat.SetColor(JMat.SKY_COLOR, SkyColor);
		JMat.SetColor(JMat.HORIZON_COLOR, HorizonColor);
		JMat.SetColor(JMat.GROUND_COLOR, GroundColor);
		JMat.SetFloat(JMat.HORIZON_THICKNESS, HorizonThickness);
		JMat.SetFloat(JMat.HORIZON_EXPONENT, HorizonExponent);
		JMat.SetFloat(JMat.HORIZON_STEP, HorizonStep);
		JMat.SetColor(JMat.FOG_COLOR, FogColor);
		JMat.SetKeywordEnable(JMat.KW_STARS, EnableStars);
		JMat.SetKeywordEnable(JMat.KW_STARS_BAKED, UseBakedStars);
		JMat.SetFloat(JMat.STARS_OPACITY, StarsOpacity);
		if (UseBakedStars)
		{
			JMat.SetTexture(JMat.STARS_CUBEMAP, StarsCubemap);
			JMat.SetTexture(JMat.STARS_TWINKLE_MAP, StarsTwinkleMap);
		}
		else
		{
			JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_0, StarsLayerCount > 0);
			JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_1, StarsLayerCount > 1);
			JMat.SetKeywordEnable(JMat.KW_STARS_LAYER_2, StarsLayerCount > 2);
			JMat.SetFloat(JMat.STARS_START, StarsStartPosition);
			JMat.SetFloat(JMat.STARS_END, StarsEndPosition);
			JMat.SetColor(JMat.STARS_COLOR_0, StarsColor0);
			JMat.SetColor(JMat.STARS_COLOR_1, StarsColor1);
			JMat.SetColor(JMat.STARS_COLOR_2, StarsColor2);
			JMat.SetFloat(JMat.STARS_DENSITY_0, StarsDensity0);
			JMat.SetFloat(JMat.STARS_DENSITY_1, StarsDensity1);
			JMat.SetFloat(JMat.STARS_DENSITY_2, StarsDensity2);
			JMat.SetFloat(JMat.STARS_SIZE_0, StarsSize0);
			JMat.SetFloat(JMat.STARS_SIZE_1, StarsSize1);
			JMat.SetFloat(JMat.STARS_SIZE_2, StarsSize2);
			JMat.SetFloat(JMat.STARS_GLOW_0, StarsGlow0);
			JMat.SetFloat(JMat.STARS_GLOW_1, StarsGlow1);
			JMat.SetFloat(JMat.STARS_GLOW_2, StarsGlow2);
			JMat.SetFloat(JMat.STARS_TWINKLE_0, StarsTwinkle0);
			JMat.SetFloat(JMat.STARS_TWINKLE_1, StarsTwinkle1);
			JMat.SetFloat(JMat.STARS_TWINKLE_2, StarsTwinkle2);
		}
		JMat.SetKeywordEnable(JMat.KW_SUN, EnableSun);
		JMat.SetKeywordEnable(JMat.KW_SUN_BAKED, UseBakedSun);
		if (UseBakedSun)
		{
			JMat.SetTexture(JMat.SUN_CUBEMAP, SunCubemap);
		}
		else
		{
			JMat.SetKeywordEnable(JMat.KW_SUN_USE_TEXTURE, SunTexture != null);
			JMat.SetTexture(JMat.SUN_TEX, SunTexture);
			JMat.SetColor(JMat.SUN_COLOR, SunColor);
			JMat.SetFloat(JMat.SUN_SIZE, SunSize);
			JMat.SetFloat(JMat.SUN_SOFT_EDGE, SunSoftEdge);
			JMat.SetFloat(JMat.SUN_GLOW, SunGlow);
		}
		JMat.SetColor(JMat.SUN_LIGHT_COLOR, SunLightColor);
		JMat.SetFloat(JMat.SUN_LIGHT_INTENSITY, SunLightIntensity);
		JMat.SetKeywordEnable(JMat.KW_MOON, EnableMoon);
		JMat.SetKeywordEnable(JMat.KW_MOON_BAKED, UseBakedMoon);
		if (UseBakedMoon)
		{
			JMat.SetTexture(JMat.MOON_CUBEMAP, MoonCubemap);
		}
		else
		{
			JMat.SetKeywordEnable(JMat.KW_MOON_USE_TEXTURE, MoonTexture != null);
			JMat.SetTexture(JMat.MOON_TEX, MoonTexture);
			JMat.SetColor(JMat.MOON_COLOR, MoonColor);
			JMat.SetFloat(JMat.MOON_SIZE, MoonSize);
			JMat.SetFloat(JMat.MOON_SOFT_EDGE, MoonSoftEdge);
			JMat.SetFloat(JMat.MOON_GLOW, MoonGlow);
		}
		JMat.SetColor(JMat.MOON_LIGHT_COLOR, MoonLightColor);
		JMat.SetFloat(JMat.MOON_LIGHT_INTENSITY, MoonLightIntensity);
		JMat.SetKeywordEnable(JMat.KW_HORIZON_CLOUD, EnableHorizonCloud);
		JMat.SetColor(JMat.HORIZON_CLOUD_COLOR, HorizonCloudColor);
		JMat.SetFloat(JMat.HORIZON_CLOUD_START, HorizonCloudStartPosition);
		JMat.SetFloat(JMat.HORIZON_CLOUD_END, HorizonCloudEndPosition);
		JMat.SetFloat(JMat.HORIZON_CLOUD_SIZE, HorizonCloudSize);
		JMat.SetFloat(JMat.HORIZON_CLOUD_STEP, HorizonCloudStep);
		JMat.SetFloat(JMat.HORIZON_CLOUD_ANIMATION_SPEED, HorizonCloudAnimationSpeed);
		JMat.SetKeywordEnable(JMat.KW_OVERHEAD_CLOUD, EnableOverheadCloud);
		JMat.SetColor(JMat.OVERHEAD_CLOUD_COLOR, OverheadCloudColor);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_ALTITUDE, OverheadCloudAltitude);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_SIZE, OverheadCloudSize);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_STEP, OverheadCloudStep);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_ANIMATION_SPEED, OverheadCloudAnimationSpeed);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_FLOW_X, OverheadCloudFlowDirectionX);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_FLOW_Z, OverheadCloudFlowDirectionZ);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_REMAP_MIN, OverheadCloudRemapMin);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_REMAP_MAX, OverheadCloudRemapMax);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_SHADOW_CLIP_MASK, OverheadCloudShadowClipMask);
		JMat.SetFloat(JMat.OVERHEAD_CLOUD_FLOW_SPEED, OverheadCloudFlowSpeed);
		JMat.SetKeywordEnable(JMat.KW_DETAIL_OVERLAY, EnableDetailOverlay);
		JMat.SetKeywordEnable(JMat.KW_DETAIL_OVERLAY_ROTATION, DetailOverlayRotationSpeed != 0f);
		JMat.SetColor(JMat.DETAIL_OVERLAY_COLOR, DetailOverlayTintColor);
		JMat.SetTexture(JMat.DETAIL_OVERLAY_CUBEMAP, DetailOverlayCubeMap);
		JMat.SetFloat(JMat.DETAIL_OVERLAY_LAYER, (float)DetailOverlayLayer);
		JMat.SetFloat(JMat.DETAIL_OVERLAY_ROTATION_SPEED, DetailOverlayRotationSpeed);
		JMat.SetKeywordEnable(JMat.KW_ALLOW_STEP_EFFECT, AllowStepEffect);
		JMat.SetActiveMaterial(null);
	}

	public void UpdateMaterialProperties()
	{
		UpdateMaterialProperties(Material);
		UpdateMaterialProperties(ShadowMaterial);
	}

	public void CopyFrom(JSkyProfile p)
	{
		SkyColor = p.SkyColor;
		HorizonColor = p.HorizonColor;
		GroundColor = p.GroundColor;
		HorizonThickness = p.HorizonThickness;
		HorizonExponent = p.HorizonExponent;
		HorizonStep = p.HorizonStep;
		FogSyncOption = p.FogSyncOption;
		FogColor = p.FogColor;
		EnableStars = p.EnableStars;
		StarsStartPosition = p.StarsStartPosition;
		StarsEndPosition = p.StarsEndPosition;
		StarsOpacity = p.StarsOpacity;
		StarsLayerCount = p.StarsLayerCount;
		StarsColor0 = p.StarsColor0;
		StarsColor1 = p.StarsColor1;
		StarsColor2 = p.StarsColor2;
		StarsDensity0 = p.StarsDensity0;
		StarsDensity1 = p.StarsDensity1;
		StarsDensity2 = p.StarsDensity2;
		StarsSize0 = p.StarsSize0;
		StarsSize1 = p.StarsSize1;
		StarsSize2 = p.StarsSize2;
		StarsGlow0 = p.StarsGlow0;
		StarsGlow1 = p.StarsGlow1;
		StarsGlow2 = p.StarsGlow2;
		StarsTwinkle0 = p.StarsTwinkle0;
		StarsTwinkle1 = p.StarsTwinkle1;
		StarsTwinkle2 = p.StarsTwinkle2;
		UseBakedStars = p.UseBakedStars;
		StarsCubemap = p.StarsCubemap;
		StarsTwinkleMap = p.StarsTwinkleMap;
		EnableSun = p.EnableSun;
		SunTexture = p.SunTexture;
		SunColor = p.SunColor;
		SunSize = p.SunSize;
		SunSoftEdge = p.SunSoftEdge;
		SunGlow = p.SunGlow;
		UseBakedSun = p.UseBakedSun;
		SunCubemap = p.SunCubemap;
		SunLightColor = p.SunLightColor;
		SunLightIntensity = p.SunLightIntensity;
		EnableMoon = p.EnableMoon;
		MoonTexture = p.MoonTexture;
		MoonColor = p.MoonColor;
		MoonSize = p.MoonSize;
		MoonSoftEdge = p.MoonSoftEdge;
		MoonGlow = p.MoonGlow;
		UseBakedMoon = p.UseBakedMoon;
		MoonCubemap = p.MoonCubemap;
		MoonLightColor = p.MoonLightColor;
		MoonLightIntensity = p.MoonLightIntensity;
		CustomCloudTexture = p.CustomCloudTexture;
		EnableHorizonCloud = p.EnableHorizonCloud;
		HorizonCloudColor = p.HorizonCloudColor;
		HorizonCloudStartPosition = p.HorizonCloudStartPosition;
		HorizonCloudEndPosition = p.HorizonCloudEndPosition;
		HorizonCloudSize = p.HorizonCloudSize;
		HorizonCloudStep = p.HorizonCloudStep;
		HorizonCloudAnimationSpeed = p.HorizonCloudAnimationSpeed;
		EnableOverheadCloud = p.EnableOverheadCloud;
		OverheadCloudColor = p.OverheadCloudColor;
		OverheadCloudAltitude = p.OverheadCloudAltitude;
		OverheadCloudSize = p.OverheadCloudSize;
		OverheadCloudStep = p.OverheadCloudStep;
		OverheadCloudAnimationSpeed = p.OverheadCloudAnimationSpeed;
		OverheadCloudFlowDirectionX = p.OverheadCloudFlowDirectionX;
		OverheadCloudFlowDirectionZ = p.OverheadCloudFlowDirectionZ;
		OverheadCloudRemapMin = p.OverheadCloudRemapMin;
		OverheadCloudRemapMax = p.OverheadCloudRemapMax;
		EnableDetailOverlay = p.EnableDetailOverlay;
		DetailOverlayTintColor = p.DetailOverlayTintColor;
		DetailOverlayCubeMap = p.DetailOverlayCubeMap;
		DetailOverlayLayer = p.DetailOverlayLayer;
		DetailOverlayRotationSpeed = p.DetailOverlayRotationSpeed;
		AllowStepEffect = p.AllowStepEffect;
		UpdateMaterialProperties();
	}
}
