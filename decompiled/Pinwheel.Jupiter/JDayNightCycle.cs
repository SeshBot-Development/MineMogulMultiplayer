using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pinwheel.Jupiter;

[ExecuteInEditMode]
public class JDayNightCycle : MonoBehaviour
{
	[SerializeField]
	private JDayNightCycleProfile profile;

	[SerializeField]
	private JSky sky;

	[SerializeField]
	private bool useSunPivot;

	[SerializeField]
	private Transform sunOrbitPivot;

	[SerializeField]
	private bool useMoonPivot;

	[SerializeField]
	private Transform moonOrbitPivot;

	[SerializeField]
	private float startTime;

	[SerializeField]
	private float timeIncrement;

	[SerializeField]
	private bool autoTimeIncrement;

	private float time;

	[SerializeField]
	private bool shouldUpdateEnvironmentReflection;

	[SerializeField]
	private int environmentReflectionResolution;

	[SerializeField]
	private ReflectionProbeTimeSlicingMode environmentReflectionTimeSlicingMode;

	[SerializeField]
	private ReflectionProbe environmentProbe;

	private Cubemap environmentReflection;

	private int probeRenderId = -1;

	public JDayNightCycleProfile Profile
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

	public JSky Sky
	{
		get
		{
			return sky;
		}
		set
		{
			sky = value;
		}
	}

	public bool UseSunPivot
	{
		get
		{
			return useSunPivot;
		}
		set
		{
			useSunPivot = value;
		}
	}

	public Transform SunOrbitPivot
	{
		get
		{
			return sunOrbitPivot;
		}
		set
		{
			sunOrbitPivot = value;
		}
	}

	public bool UseMoonPivot
	{
		get
		{
			return useMoonPivot;
		}
		set
		{
			useMoonPivot = value;
		}
	}

	public Transform MoonOrbitPivot
	{
		get
		{
			return moonOrbitPivot;
		}
		set
		{
			moonOrbitPivot = value;
		}
	}

	public float StartTime
	{
		get
		{
			return startTime;
		}
		set
		{
			startTime = Mathf.Clamp(value, 0f, 24f);
		}
	}

	public float TimeIncrement
	{
		get
		{
			return timeIncrement;
		}
		set
		{
			timeIncrement = Mathf.Max(0f, value);
		}
	}

	public bool AutoTimeIncrement
	{
		get
		{
			return autoTimeIncrement;
		}
		set
		{
			autoTimeIncrement = value;
		}
	}

	public float Time
	{
		get
		{
			return time % 24f;
		}
		set
		{
			time = value % 24f;
		}
	}

	public bool ShouldUpdateEnvironmentReflection
	{
		get
		{
			return shouldUpdateEnvironmentReflection;
		}
		set
		{
			shouldUpdateEnvironmentReflection = value;
		}
	}

	public int EnvironmentReflectionResolution
	{
		get
		{
			return environmentReflectionResolution;
		}
		set
		{
			if (environmentReflectionResolution != (environmentReflectionResolution = Mathf.Clamp(value, 16, 2048)))
			{
				if (environmentReflection != null)
				{
					JUtilities.DestroyObject(environmentReflection);
				}
				if (environmentProbe != null)
				{
					JUtilities.DestroyGameobject(environmentProbe.gameObject);
				}
			}
		}
	}

	public ReflectionProbeTimeSlicingMode EnvironmentReflectionTimeSlicingMode
	{
		get
		{
			return environmentReflectionTimeSlicingMode;
		}
		set
		{
			environmentReflectionTimeSlicingMode = value;
		}
	}

	private ReflectionProbe EnvironmentProbe
	{
		get
		{
			if (environmentProbe == null)
			{
				GameObject gameObject = new GameObject("~EnvironmentReflectionRenderer");
				gameObject.transform.parent = base.transform;
				gameObject.transform.position = new Vector3(0f, -1000f, 0f);
				gameObject.transform.rotation = Quaternion.identity;
				gameObject.transform.localScale = Vector3.one;
				environmentProbe = gameObject.AddComponent<ReflectionProbe>();
				environmentProbe.resolution = EnvironmentReflectionResolution;
				environmentProbe.size = new Vector3(1f, 1f, 1f);
				environmentProbe.cullingMask = 0;
			}
			environmentProbe.clearFlags = ReflectionProbeClearFlags.Skybox;
			environmentProbe.mode = ReflectionProbeMode.Realtime;
			environmentProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
			environmentProbe.timeSlicingMode = EnvironmentReflectionTimeSlicingMode;
			environmentProbe.hdr = false;
			return environmentProbe;
		}
	}

	private Cubemap EnvironmentReflection
	{
		get
		{
			if (environmentReflection == null)
			{
				environmentReflection = new Cubemap(EnvironmentProbe.resolution, TextureFormat.RGBA32, mipChain: true);
			}
			return environmentReflection;
		}
	}

	private float DeltaTime
	{
		get
		{
			if (Application.isPlaying)
			{
				return UnityEngine.Time.deltaTime;
			}
			return 1f / 60f;
		}
	}

	private void Reset()
	{
		Sky = GetComponent<JSky>();
		StartTime = 0f;
		TimeIncrement = 1f;
		AutoTimeIncrement = true;
		Time = 0f;
	}

	private void OnEnable()
	{
		time = StartTime;
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Combine(Camera.onPreCull, new Camera.CameraCallback(OnCameraPreCull));
		RenderPipelineManager.beginContextRendering += OnBeginContextRenderingSRP;
	}

	private void OnDisable()
	{
		Camera.onPreCull = (Camera.CameraCallback)Delegate.Remove(Camera.onPreCull, new Camera.CameraCallback(OnCameraPreCull));
		RenderPipelineManager.beginContextRendering -= OnBeginContextRenderingSRP;
		CleanUp();
	}

	private void OnCameraPreCull(Camera cam)
	{
		if (!Application.isPlaying)
		{
			Update();
		}
	}

	private void OnBeginFrameRenderingSRP(ScriptableRenderContext context, Camera[] cameras)
	{
		if (!Application.isPlaying)
		{
			Update();
		}
	}

	private void OnBeginContextRenderingSRP(ScriptableRenderContext context, List<Camera> cameras)
	{
		if (!Application.isPlaying)
		{
			Update();
		}
	}

	private void CleanUp()
	{
		if (environmentProbe != null)
		{
			JUtilities.DestroyGameobject(environmentProbe.gameObject);
		}
		if (environmentReflection != null)
		{
			JUtilities.DestroyObject(environmentReflection);
		}
		if (Sky != null)
		{
			Sky.DNC = null;
		}
	}

	private void Update()
	{
		AnimateSky();
		if (ShouldUpdateEnvironmentReflection)
		{
			UpdateEnvironmentReflection();
		}
		else
		{
			RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
		}
	}

	private void AnimateSky()
	{
		if (!(Profile == null) && !(Sky == null) && !(Sky.Profile == null))
		{
			Sky.DNC = this;
			if (AutoTimeIncrement)
			{
				Time += TimeIncrement * DeltaTime;
			}
			float num = Mathf.InverseLerp(0f, 24f, Time);
			Profile.Animate(Sky, num);
			if (Sky.Profile.EnableSun && Sky.SunLightSource != null)
			{
				Vector3 vector = Matrix4x4.Rotate(Quaternion.Euler(num * 360f, 0f, 0f)).MultiplyVector(Vector3.up);
				Vector3 forward = ((UseSunPivot && SunOrbitPivot != null) ? SunOrbitPivot : base.transform).localToWorldMatrix.MultiplyVector(vector);
				Sky.SunLightSource.transform.forward = forward;
				Sky.SunLightSource.color = Sky.Profile.Material.GetColor(JMat.SUN_LIGHT_COLOR);
				Sky.SunLightSource.intensity = Sky.Profile.Material.GetFloat(JMat.SUN_LIGHT_INTENSITY);
			}
			if (Sky.Profile.EnableMoon && Sky.MoonLightSource != null)
			{
				Vector3 vector2 = Matrix4x4.Rotate(Quaternion.Euler(num * 360f, 0f, 0f)).MultiplyVector(Vector3.down);
				Vector3 forward2 = ((UseMoonPivot && MoonOrbitPivot != null) ? MoonOrbitPivot : base.transform).localToWorldMatrix.MultiplyVector(vector2);
				Sky.MoonLightSource.transform.forward = forward2;
				Sky.MoonLightSource.color = Sky.Profile.Material.GetColor(JMat.MOON_LIGHT_COLOR);
				Sky.MoonLightSource.intensity = Sky.Profile.Material.GetFloat(JMat.MOON_LIGHT_INTENSITY);
			}
		}
	}

	private void UpdateEnvironmentReflection()
	{
		if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) != CopyTextureSupport.None)
		{
			if (EnvironmentProbe.texture == null)
			{
				probeRenderId = EnvironmentProbe.RenderProbe();
			}
			else if (EnvironmentProbe.texture != null || EnvironmentProbe.IsFinishedRendering(probeRenderId))
			{
				Graphics.CopyTexture(EnvironmentProbe.texture, EnvironmentReflection);
				RenderSettings.customReflectionTexture = EnvironmentReflection;
				RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
				probeRenderId = EnvironmentProbe.RenderProbe();
			}
		}
	}
}
