using System;
using UnityEngine;

namespace Pinwheel.Jupiter;

[Serializable]
public struct JInternalShaderSettings
{
	[SerializeField]
	private Shader skyShader;

	[SerializeField]
	private Shader skyShadowShader;

	[SerializeField]
	private Shader copyTextureShader;

	[SerializeField]
	private Shader solidColorShader;

	public Shader SkyShader
	{
		get
		{
			return skyShader;
		}
		set
		{
			skyShader = value;
		}
	}

	public Shader SkyShadowShader
	{
		get
		{
			return skyShadowShader;
		}
		set
		{
			skyShadowShader = value;
		}
	}

	public Shader CopyTextureShader
	{
		get
		{
			return copyTextureShader;
		}
		set
		{
			copyTextureShader = value;
		}
	}

	public Shader SolidColorShader
	{
		get
		{
			return solidColorShader;
		}
		set
		{
			solidColorShader = value;
		}
	}
}
