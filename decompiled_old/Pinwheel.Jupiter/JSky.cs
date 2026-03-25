using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter;

[ExecuteInEditMode]
public class JSky : MonoBehaviour
{
	public static readonly Vector3 DefaultSunDirection = Vector3.forward;

	public static readonly Vector3 DefaultMoonDirection = Vector3.forward;

	[SerializeField]
	private JSkyProfile profile;

	[SerializeField]
	private Light sunLightSource;

	[SerializeField]
	private Light moonLightSource;

	private static Mesh sphereMesh;

	private Stopwatch timer;

	private double lastElapsedSeconds;

	private double frameTime;

	public JSkyProfile Profile
	{
		get
		{
			return profile;
		}
		set
		{
			profile = value;
		}
	}

	public Light SunLightSource
	{
		get
		{
			return sunLightSource;
		}
		set
		{
			if (value != null && value.type == LightType.Directional)
			{
				sunLightSource = value;
			}
			else
			{
				sunLightSource = null;
			}
		}
	}

	public Light MoonLightSource
	{
		get
		{
			return moonLightSource;
		}
		set
		{
			if (value != null && value.type == LightType.Directional)
			{
				moonLightSource = value;
			}
			else
			{
				moonLightSource = null;
			}
		}
	}

	public JDayNightCycle DNC { get; set; }

	private static Mesh SphereMesh
	{
		get
		{
			if (sphereMesh == null)
			{
				sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
			}
			return sphereMesh;
		}
	}

	private void OnEnable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnCameraPreCull));
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRenderingSRP;
		timer = new Stopwatch();
		timer.Start();
	}

	private void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnCameraPreCull));
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRenderingSRP;
		RenderSettings.skybox = JJupiterSettings.Instance.DefaultSkybox;
	}

	private void OnDestroy()
	{
		RenderSettings.skybox = JJupiterSettings.Instance.DefaultSkybox;
	}

	private void Reset()
	{
		Light[] array = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].type == LightType.Directional)
			{
				SunLightSource = array[i];
				break;
			}
		}
	}

	private void OnCameraPreCull(Camera cam)
	{
		SetupSkyMaterial();
		SyncFog();
		RenderShadow(cam);
	}

	private void OnBeginCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
	{
		OnCameraPreCull(cam);
	}

	public double GetTimeParam()
	{
		if (timer == null)
		{
			return 0.0;
		}
		double num = (float)timer.ElapsedMilliseconds * 0.001f;
		double num2 = num - lastElapsedSeconds;
		frameTime += num2 * (double)Time.timeScale;
		lastElapsedSeconds = num;
		return frameTime;
	}

	private void SetupSkyMaterial()
	{
		if (Profile == null)
		{
			RenderSettings.skybox = JJupiterSettings.Instance.DefaultSkybox;
			return;
		}
		RenderSettings.skybox = Profile.Material;
		Shader.SetGlobalFloat(JMat.TIME, (float)GetTimeParam());
		Profile.Material.SetTexture(JMat.NOISE_TEX, JJupiterSettings.Instance.NoiseTexture);
		Profile.Material.SetTexture(JMat.CLOUD_TEX, Profile.CustomCloudTexture ? Profile.CustomCloudTexture : JJupiterSettings.Instance.CloudTexture);
		if (Profile.EnableSun)
		{
			if (SunLightSource != null)
			{
				JDayNightCycleProfile jDayNightCycleProfile = (DNC ? DNC.Profile : null);
				if (!(jDayNightCycleProfile != null) || !jDayNightCycleProfile.ContainProperty("SunLightColor"))
				{
					SunLightSource.color = Profile.SunLightColor;
				}
				if (!(jDayNightCycleProfile != null) || !jDayNightCycleProfile.ContainProperty("SunLightIntensity"))
				{
					SunLightSource.intensity = Profile.SunLightIntensity;
				}
			}
			Vector3 vector = (SunLightSource ? SunLightSource.transform.forward : DefaultSunDirection);
			if (Profile.UseBakedSun)
			{
				Matrix4x4 value = Matrix4x4.Rotate(Quaternion.FromToRotation(vector, DefaultSunDirection));
				Profile.Material.SetMatrix(JMat.SUN_ROTATION_MATRIX, value);
			}
			else
			{
				Matrix4x4 inverse = Matrix4x4.TRS(-vector, Quaternion.LookRotation(vector), Profile.SunSize * Vector3.one).inverse;
				Profile.Material.SetVector(JMat.SUN_DIRECTION, vector);
				Profile.Material.SetMatrix(JMat.SUN_TRANSFORM_MATRIX, inverse);
			}
		}
		if (!Profile.EnableMoon)
		{
			return;
		}
		if (MoonLightSource != null)
		{
			JDayNightCycleProfile jDayNightCycleProfile2 = (DNC ? DNC.Profile : null);
			if (!(jDayNightCycleProfile2 != null) || !jDayNightCycleProfile2.ContainProperty("MoonLightColor"))
			{
				MoonLightSource.color = Profile.MoonLightColor;
			}
			if (!(jDayNightCycleProfile2 != null) || !jDayNightCycleProfile2.ContainProperty("MoonLightIntensity"))
			{
				MoonLightSource.intensity = Profile.MoonLightIntensity;
			}
		}
		Vector3 vector2 = (MoonLightSource ? MoonLightSource.transform.forward : DefaultMoonDirection);
		if (Profile.UseBakedMoon)
		{
			Matrix4x4 value2 = Matrix4x4.Rotate(Quaternion.FromToRotation(vector2, DefaultMoonDirection));
			Profile.Material.SetMatrix(JMat.MOON_ROTATION_MATRIX, value2);
		}
		else
		{
			Matrix4x4 inverse2 = Matrix4x4.TRS(-vector2, Quaternion.LookRotation(vector2), Profile.MoonSize * Vector3.one).inverse;
			Profile.Material.SetVector(JMat.MOON_DIRECTION, vector2);
			Profile.Material.SetMatrix(JMat.MOON_TRANSFORM_MATRIX, inverse2);
		}
	}

	private void SyncFog()
	{
		if (!(Profile == null) && Profile.FogSyncOption != JFogSyncOption.DontSync)
		{
			if (Profile.FogSyncOption == JFogSyncOption.SkyColor)
			{
				RenderSettings.fogColor = Profile.Material.GetColor(JMat.SKY_COLOR);
			}
			else if (Profile.FogSyncOption == JFogSyncOption.HorizonColor)
			{
				RenderSettings.fogColor = Profile.Material.GetColor(JMat.HORIZON_COLOR);
			}
			else if (Profile.FogSyncOption == JFogSyncOption.GroundColor)
			{
				RenderSettings.fogColor = Profile.Material.GetColor(JMat.GROUND_COLOR);
			}
			else if (Profile.FogSyncOption == JFogSyncOption.CustomColor)
			{
				RenderSettings.fogColor = Profile.Material.GetColor(JMat.FOG_COLOR);
			}
		}
	}

	private void RenderShadow(Camera cam)
	{
		if (!(Profile == null) && Profile.EnableOverheadCloud && Profile.OverheadCloudCastShadow)
		{
			Profile.ShadowMaterial.SetTexture(JMat.NOISE_TEX, JJupiterSettings.Instance.NoiseTexture);
			Profile.ShadowMaterial.SetTexture(JMat.CLOUD_TEX, Profile.CustomCloudTexture ? Profile.CustomCloudTexture : JJupiterSettings.Instance.CloudTexture);
			Graphics.DrawMesh(SphereMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, 2f * Vector3.one * Profile.OverheadCloudAltitude), Profile.ShadowMaterial, 0, cam, 0, null, ShadowCastingMode.ShadowsOnly, receiveShadows: false, null, LightProbeUsage.Off, null);
		}
	}
}
