using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace ScreenSpaceCavityCurvature;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("SSCC Screen Space Cavity Curvature")]
[RequireComponent(typeof(Camera))]
public class SSCC : MonoBehaviour
{
	public enum PerPixelNormals
	{
		DeferredGBuffer,
		Camera,
		ReconstructedFromDepth
	}

	public enum DebugMode
	{
		Disabled,
		EffectOnly,
		ViewNormals
	}

	public enum CavitySamples
	{
		Low6,
		Medium8,
		High12
	}

	public enum OutputEffectTo
	{
		Screen,
		[InspectorName("_SSCCTexture in shaders")]
		_SSCCTexture
	}

	private static class Pass
	{
		public const int Copy = 0;

		public const int Composite = 1;
	}

	private static class ShaderProperties
	{
		public static int mainTex = Shader.PropertyToID("_MainTex");

		public static int tempTex = Shader.PropertyToID("_TempTex");

		public static int uvTransform = Shader.PropertyToID("_UVTransform");

		public static int inputTexelSize = Shader.PropertyToID("_Input_TexelSize");

		public static int uvToView = Shader.PropertyToID("_UVToView");

		public static int worldToCameraMatrix = Shader.PropertyToID("_WorldToCameraMatrix");

		public static int effectIntensity = Shader.PropertyToID("_EffectIntensity");

		public static int distanceFade = Shader.PropertyToID("_DistanceFade");

		public static int curvaturePixelRadius = Shader.PropertyToID("_CurvaturePixelRadius");

		public static int curvatureRidge = Shader.PropertyToID("_CurvatureBrights");

		public static int curvatureValley = Shader.PropertyToID("_CurvatureDarks");

		public static int cavityWorldRadius = Shader.PropertyToID("_CavityWorldRadius");

		public static int cavityRidge = Shader.PropertyToID("_CavityBrights");

		public static int cavityValley = Shader.PropertyToID("_CavityDarks");

		public static int globalSSCCTexture = Shader.PropertyToID("_SSCCTexture");
	}

	[HideInInspector]
	public Shader ssccShader;

	[Tooltip("Lerps the whole effect from 0 to 1.")]
	[Range(0f, 1f)]
	public float effectIntensity = 1f;

	[Tooltip("Divides effect intensity by (depth * distanceFade).\nZero means effect doesn't fade with distance.")]
	[Range(0f, 1f)]
	public float distanceFade;

	[Space(6f)]
	[Tooltip("The radius of curvature calculations in pixels.")]
	[Range(0f, 4f)]
	public int curvaturePixelRadius = 2;

	[Tooltip("How bright does curvature get.")]
	[Range(0f, 5f)]
	public float curvatureBrights = 2f;

	[Tooltip("How dark does curvature get.")]
	[Range(0f, 5f)]
	public float curvatureDarks = 3f;

	[Space(6f)]
	[Tooltip("The amount of samples used for cavity calculation.")]
	public CavitySamples cavitySamples;

	[Tooltip("True: uses pow() blending to make colors more saturated in bright/dark areas of cavity.\n\nFalse: uses additive blending.")]
	public bool saturateCavity = true;

	[Tooltip("The radius of cavity calculations in world units.")]
	[Range(0f, 0.5f)]
	public float cavityRadius = 0.25f;

	[Tooltip("How bright does cavity get.")]
	[Range(0f, 5f)]
	public float cavityBrights = 3f;

	[Tooltip("How dark does cavity get.")]
	[Range(0f, 5f)]
	public float cavityDarks = 2f;

	[Space(6f)]
	[Tooltip("Where to get normals from.")]
	public PerPixelNormals normalsSource = PerPixelNormals.Camera;

	[Tooltip("May be useful to check what objects contribute normals, as objects that do not contribute their normals will not contribute to the effect.")]
	public DebugMode debugMode;

	[Space(6f)]
	[Tooltip("Screen: Applies the effect over the entire screen.\n\n_SSCCTexture: Instead of writing the effect to the screen, will write the effect into a global shader texture named _SSCCTexture, so you can sample it selectively in your shaders and exclude certain objects from receiving outlines etc. See \"Output To Texture Examples\" folder for example shaders.")]
	public OutputEffectTo output;

	private Material material;

	private Camera ssccCamera;

	private CommandBuffer cmdBuffer;

	private int width;

	private int height;

	private bool stereoActive;

	private XRSettings.StereoRenderingMode stereoRenderingMode;

	private int screenWidth;

	private int screenHeight;

	private CameraEvent[] possibleCameraEvents = new CameraEvent[2]
	{
		CameraEvent.BeforeImageEffectsOpaque,
		CameraEvent.BeforeForwardOpaque
	};

	private RenderTextureDescriptor m_sourceDescriptor;

	private bool m_IsCommandBufferDirty;

	private Mesh m_FullscreenTriangle;

	private DebugMode? m_PreviousDebugMode;

	private int m_PreviousWidth;

	private int m_PreviousHeight;

	private RenderingPath m_PreviousRenderingPath;

	private OutputEffectTo m_PreviousOutputEffectTo;

	private CameraEvent m_PreviousCameraEvent;

	private OutputEffectTo Output
	{
		get
		{
			if (debugMode == DebugMode.Disabled)
			{
				return output;
			}
			return OutputEffectTo.Screen;
		}
	}

	private CameraEvent cameraEvent
	{
		get
		{
			if (Output != OutputEffectTo.Screen)
			{
				return CameraEvent.BeforeForwardOpaque;
			}
			return CameraEvent.BeforeImageEffectsOpaque;
		}
	}

	private Mesh fullscreenTriangle
	{
		get
		{
			if (m_FullscreenTriangle != null)
			{
				return m_FullscreenTriangle;
			}
			m_FullscreenTriangle = new Mesh
			{
				name = "Fullscreen Triangle"
			};
			m_FullscreenTriangle.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			m_FullscreenTriangle.SetIndices(new int[3] { 0, 1, 2 }, MeshTopology.Triangles, 0, calculateBounds: false);
			m_FullscreenTriangle.UploadMeshData(markNoLongerReadable: false);
			return m_FullscreenTriangle;
		}
	}

	private bool isCommandBufferDirty
	{
		get
		{
			if (m_IsCommandBufferDirty || m_PreviousDebugMode != debugMode || m_PreviousWidth != width || m_PreviousHeight != height || m_PreviousRenderingPath != ssccCamera.actualRenderingPath || m_PreviousOutputEffectTo != Output || m_PreviousCameraEvent != cameraEvent)
			{
				m_PreviousDebugMode = debugMode;
				m_PreviousWidth = width;
				m_PreviousHeight = height;
				m_PreviousRenderingPath = ssccCamera.actualRenderingPath;
				m_PreviousOutputEffectTo = Output;
				m_PreviousCameraEvent = cameraEvent;
				return true;
			}
			return false;
		}
		set
		{
			m_IsCommandBufferDirty = value;
		}
	}

	private static RenderTextureFormat defaultHDRRenderTextureFormat => RenderTextureFormat.DefaultHDR;

	private RenderTextureFormat sourceFormat
	{
		get
		{
			if (!ssccCamera.allowHDR)
			{
				return RenderTextureFormat.Default;
			}
			return defaultHDRRenderTextureFormat;
		}
	}

	private static RenderTextureFormat colorFormat
	{
		get
		{
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
			{
				return RenderTextureFormat.Default;
			}
			return RenderTextureFormat.ARGBHalf;
		}
	}

	private void CheckParameters()
	{
		if (GraphicsSettings.defaultRenderPipeline != null)
		{
			Debug.LogWarning("Please follow the SRP usage instructions.");
			base.enabled = false;
		}
		ssccCamera.depthTextureMode |= DepthTextureMode.Depth;
		if (normalsSource == PerPixelNormals.Camera)
		{
			ssccCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
		}
		if (ssccCamera.actualRenderingPath != RenderingPath.DeferredShading && normalsSource == PerPixelNormals.DeferredGBuffer)
		{
			normalsSource = PerPixelNormals.Camera;
		}
		if (stereoActive && ssccCamera.actualRenderingPath != RenderingPath.DeferredShading && normalsSource != PerPixelNormals.ReconstructedFromDepth)
		{
			normalsSource = PerPixelNormals.ReconstructedFromDepth;
		}
		if (output == OutputEffectTo._SSCCTexture && ssccCamera.actualRenderingPath == RenderingPath.DeferredShading)
		{
			normalsSource = PerPixelNormals.ReconstructedFromDepth;
		}
	}

	private void OnEnable()
	{
		if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
		{
			Debug.LogWarning("SSCC shader is not supported on this platform.");
			base.enabled = false;
			return;
		}
		if (ssccShader == null)
		{
			ssccShader = Shader.Find("Hidden/SSCC");
		}
		if (ssccShader == null)
		{
			Debug.LogError("SSCC shader was not found...");
		}
		else if (!ssccShader.isSupported)
		{
			Debug.LogWarning("SSCC shader is not supported on this platform.");
			base.enabled = false;
		}
		else
		{
			Initialize();
		}
	}

	private void OnDisable()
	{
		ClearCommandBuffer(cmdBuffer);
		if (material != null)
		{
			UnityEngine.Object.DestroyImmediate(material);
		}
		if (fullscreenTriangle != null)
		{
			UnityEngine.Object.DestroyImmediate(fullscreenTriangle);
		}
	}

	private void OnPreRender()
	{
		if (!(ssccShader == null) && !(ssccCamera == null))
		{
			FetchRenderParameters();
			CheckParameters();
			UpdateMaterialProperties();
			UpdateShaderKeywords();
			if (isCommandBufferDirty)
			{
				ClearCommandBuffer(cmdBuffer);
				BuildCommandBuffer(cmdBuffer);
				ssccCamera.AddCommandBuffer(cameraEvent, cmdBuffer);
				isCommandBufferDirty = false;
			}
		}
	}

	private void OnValidate()
	{
		if (!(ssccShader == null) && !(ssccCamera == null))
		{
			CheckParameters();
		}
	}

	private void Initialize()
	{
		m_sourceDescriptor = new RenderTextureDescriptor(0, 0);
		ssccCamera = GetComponent<Camera>();
		ssccCamera.forceIntoRenderTexture = true;
		material = new Material(ssccShader);
		material.hideFlags = HideFlags.HideAndDontSave;
		cmdBuffer = new CommandBuffer
		{
			name = "SSCC"
		};
		isCommandBufferDirty = true;
	}

	private void FetchRenderParameters()
	{
		if (ssccCamera.stereoEnabled)
		{
			RenderTextureDescriptor eyeTextureDesc = XRSettings.eyeTextureDesc;
			stereoRenderingMode = XRSettings.StereoRenderingMode.SinglePass;
			if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass)
			{
				stereoRenderingMode = XRSettings.StereoRenderingMode.MultiPass;
			}
			if (eyeTextureDesc.dimension == TextureDimension.Tex2DArray)
			{
				stereoRenderingMode = XRSettings.StereoRenderingMode.SinglePassInstanced;
			}
			if (stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass)
			{
				eyeTextureDesc.vrUsage = VRTextureUsage.None;
			}
			width = eyeTextureDesc.width;
			height = eyeTextureDesc.height;
			m_sourceDescriptor = eyeTextureDesc;
			screenWidth = XRSettings.eyeTextureWidth;
			screenHeight = XRSettings.eyeTextureHeight;
			if (stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePass)
			{
				screenWidth /= 2;
			}
			stereoActive = true;
		}
		else
		{
			width = ssccCamera.pixelWidth;
			height = ssccCamera.pixelHeight;
			m_sourceDescriptor.width = width;
			m_sourceDescriptor.height = height;
			screenWidth = width;
			screenHeight = height;
			stereoActive = false;
		}
	}

	private void ClearCommandBuffer(CommandBuffer cmd)
	{
		if (cmd == null)
		{
			return;
		}
		if (ssccCamera != null)
		{
			CameraEvent[] array = possibleCameraEvents;
			foreach (CameraEvent evt in array)
			{
				ssccCamera.RemoveCommandBuffer(evt, cmd);
			}
		}
		cmd.Clear();
	}

	private void BuildCommandBuffer(CommandBuffer cmd)
	{
		if (Output == OutputEffectTo._SSCCTexture)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.globalSSCCTexture);
			cmd.GetTemporaryRT(ShaderProperties.globalSSCCTexture, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBHalf);
			cmd.SetGlobalTexture(ShaderProperties.globalSSCCTexture, new RenderTargetIdentifier(ShaderProperties.globalSSCCTexture, 0, CubemapFace.Unknown, -1));
		}
		else
		{
			cmd.SetGlobalTexture(ShaderProperties.globalSSCCTexture, BuiltinRenderTextureType.None);
			GetScreenSpaceTemporaryRT(cmd, ShaderProperties.tempTex, 0, sourceFormat);
			if (stereoActive && ssccCamera.actualRenderingPath != RenderingPath.DeferredShading)
			{
				cmd.Blit(BuiltinRenderTextureType.CameraTarget, ShaderProperties.tempTex);
			}
			else
			{
				BlitFullscreenTriangle(cmd, BuiltinRenderTextureType.CameraTarget, ShaderProperties.tempTex, material);
			}
		}
		cmd.SetGlobalVector(ShaderProperties.uvTransform, SystemInfo.graphicsUVStartsAtTop ? new Vector4(1f, -1f, 0f, 1f) : new Vector4(1f, 1f, 0f, 0f));
		if (Output == OutputEffectTo.Screen)
		{
			BlitFullscreenTriangle(cmd, ShaderProperties.tempTex, BuiltinRenderTextureType.CameraTarget, material, 1);
		}
		else
		{
			BlitFullscreenTriangle(cmd, ShaderProperties.globalSSCCTexture, material, 1);
		}
		if (Output == OutputEffectTo.Screen)
		{
			cmd.ReleaseTemporaryRT(ShaderProperties.tempTex);
		}
	}

	private void UpdateMaterialProperties()
	{
		float num = Mathf.Tan(0.5f * ssccCamera.fieldOfView * (MathF.PI / 180f));
		float num2 = 1f / (1f / num * ((float)screenHeight / (float)screenWidth));
		float num3 = 1f / (1f / num);
		material.SetVector(ShaderProperties.inputTexelSize, new Vector4(1f / (float)width, 1f / (float)height, width, height));
		material.SetVector(ShaderProperties.uvToView, new Vector4(2f * num2, -2f * num3, -1f * num2, 1f * num3));
		material.SetMatrix(ShaderProperties.worldToCameraMatrix, ssccCamera.worldToCameraMatrix);
		material.SetFloat(ShaderProperties.effectIntensity, effectIntensity);
		material.SetFloat(ShaderProperties.distanceFade, distanceFade);
		material.SetFloat(ShaderProperties.curvaturePixelRadius, (new float[5] { 0f, 0.5f, 1f, 1.5f, 2.5f })[curvaturePixelRadius]);
		material.SetFloat(ShaderProperties.curvatureRidge, (curvatureBrights == 0f) ? 999f : (5f - curvatureBrights));
		material.SetFloat(ShaderProperties.curvatureValley, (curvatureDarks == 0f) ? 999f : (5f - curvatureDarks));
		material.SetFloat(ShaderProperties.cavityWorldRadius, cavityRadius);
		material.SetFloat(ShaderProperties.cavityRidge, cavityBrights * 2f);
		material.SetFloat(ShaderProperties.cavityValley, cavityDarks * 2f);
	}

	private void UpdateShaderKeywords()
	{
		material.shaderKeywords = new string[6]
		{
			ssccCamera.orthographic ? "ORTHOGRAPHIC_PROJECTION" : "__",
			(debugMode == DebugMode.EffectOnly) ? "DEBUG_EFFECT" : ((debugMode == DebugMode.ViewNormals) ? "DEBUG_NORMALS" : "__"),
			(normalsSource == PerPixelNormals.Camera) ? "NORMALS_CAMERA" : ((normalsSource == PerPixelNormals.ReconstructedFromDepth) ? "NORMALS_RECONSTRUCT" : "__"),
			(cavitySamples == CavitySamples.Low6) ? "CAVITY_SAMPLES_6" : ((cavitySamples == CavitySamples.Medium8) ? "CAVITY_SAMPLES_8" : ((cavitySamples == CavitySamples.High12) ? "CAVITY_SAMPLES_12" : "")),
			saturateCavity ? "SATURATE_CAVITY" : "__",
			(Output == OutputEffectTo._SSCCTexture) ? "OUTPUT_TO_TEXTURE" : "__"
		};
	}

	private RenderTextureDescriptor GetDefaultDescriptor(int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default)
	{
		RenderTextureDescriptor result = new RenderTextureDescriptor(m_sourceDescriptor.width, m_sourceDescriptor.height, m_sourceDescriptor.colorFormat, depthBufferBits);
		result.dimension = m_sourceDescriptor.dimension;
		result.volumeDepth = m_sourceDescriptor.volumeDepth;
		result.vrUsage = m_sourceDescriptor.vrUsage;
		result.msaaSamples = m_sourceDescriptor.msaaSamples;
		result.memoryless = m_sourceDescriptor.memoryless;
		result.useMipMap = m_sourceDescriptor.useMipMap;
		result.autoGenerateMips = m_sourceDescriptor.autoGenerateMips;
		result.enableRandomWrite = m_sourceDescriptor.enableRandomWrite;
		result.shadowSamplingMode = m_sourceDescriptor.shadowSamplingMode;
		if (ssccCamera.allowDynamicResolution)
		{
			result.useDynamicScale = true;
		}
		if (colorFormat != RenderTextureFormat.Default)
		{
			result.colorFormat = colorFormat;
		}
		switch (readWrite)
		{
		case RenderTextureReadWrite.sRGB:
			result.sRGB = true;
			break;
		case RenderTextureReadWrite.Linear:
			result.sRGB = false;
			break;
		case RenderTextureReadWrite.Default:
			result.sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear;
			break;
		}
		return result;
	}

	private RenderTexture GetScreenSpaceRT(int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filter = FilterMode.Bilinear, int widthOverride = 0, int heightOverride = 0)
	{
		RenderTextureDescriptor defaultDescriptor = GetDefaultDescriptor(depthBufferBits, colorFormat, readWrite);
		if (widthOverride > 0)
		{
			defaultDescriptor.width = widthOverride;
		}
		if (heightOverride > 0)
		{
			defaultDescriptor.height = heightOverride;
		}
		if (stereoActive && defaultDescriptor.dimension == TextureDimension.Tex2DArray)
		{
			defaultDescriptor.dimension = TextureDimension.Tex2D;
		}
		return new RenderTexture(defaultDescriptor)
		{
			filterMode = filter
		};
	}

	private void GetScreenSpaceTemporaryRT(CommandBuffer cmd, int nameID, int depthBufferBits = 0, RenderTextureFormat colorFormat = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filter = FilterMode.Bilinear, int widthOverride = 0, int heightOverride = 0)
	{
		RenderTextureDescriptor defaultDescriptor = GetDefaultDescriptor(depthBufferBits, colorFormat, readWrite);
		if (widthOverride > 0)
		{
			defaultDescriptor.width = widthOverride;
		}
		if (heightOverride > 0)
		{
			defaultDescriptor.height = heightOverride;
		}
		if (stereoActive && defaultDescriptor.dimension == TextureDimension.Tex2DArray)
		{
			defaultDescriptor.dimension = TextureDimension.Tex2D;
		}
		cmd.GetTemporaryRT(nameID, defaultDescriptor, filter);
	}

	private void BlitFullscreenTriangle(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int pass = 0)
	{
		cmd.SetGlobalTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(destination);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
	}

	private void BlitFullscreenTriangle(CommandBuffer cmd, RenderTargetIdentifier destination, Material material, int pass = 0)
	{
		cmd.SetRenderTarget(destination);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
	}

	private void BlitFullscreenTriangleWithClear(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, Color clearColor, int pass = 0)
	{
		cmd.SetGlobalTexture(ShaderProperties.mainTex, source);
		cmd.SetRenderTarget(destination);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, clearColor);
		cmd.DrawMesh(fullscreenTriangle, Matrix4x4.identity, material, 0, pass);
	}
}
