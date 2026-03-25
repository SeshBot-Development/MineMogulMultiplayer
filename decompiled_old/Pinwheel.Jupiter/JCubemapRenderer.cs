using UnityEngine;

namespace Pinwheel.Jupiter;

public static class JCubemapRenderer
{
	public static bool Render(JCubemapRendererArgs args)
	{
		GameObject gameObject = new GameObject("~CubemapRendererCamera");
		gameObject.transform.position = args.CameraPosition;
		Camera camera = gameObject.AddComponent<Camera>();
		camera.clearFlags = args.CameraClearFlag;
		camera.nearClipPlane = args.CameraNearPlane;
		camera.farClipPlane = args.CameraFarPlane;
		camera.backgroundColor = args.CameraBackgroundColor;
		bool result = camera.RenderToCubemap(args.Cubemap, (int)args.Face);
		JUtilities.DestroyGameobject(gameObject);
		return result;
	}
}
