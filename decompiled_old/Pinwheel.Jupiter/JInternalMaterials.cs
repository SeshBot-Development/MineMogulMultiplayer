using UnityEngine;

namespace Pinwheel.Jupiter;

public static class JInternalMaterials
{
	private static Material copyTextureMaterial;

	private static Material solidColorMaterial;

	private static Material unlitTextureMaterial;

	public static Material CopyTextureMaterial
	{
		get
		{
			if (copyTextureMaterial == null)
			{
				copyTextureMaterial = new Material(JJupiterSettings.Instance.InternalShaders.CopyTextureShader);
			}
			return copyTextureMaterial;
		}
	}

	public static Material SolidColorMaterial
	{
		get
		{
			if (solidColorMaterial == null)
			{
				solidColorMaterial = new Material(JJupiterSettings.Instance.InternalShaders.SolidColorShader);
			}
			return solidColorMaterial;
		}
	}

	public static Material UnlitTextureMaterial
	{
		get
		{
			if (unlitTextureMaterial == null)
			{
				unlitTextureMaterial = new Material(Shader.Find("Unlit/Texture"));
			}
			return unlitTextureMaterial;
		}
	}
}
