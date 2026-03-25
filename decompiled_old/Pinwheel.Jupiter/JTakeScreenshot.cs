using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Pinwheel.Jupiter;

public class JTakeScreenshot : MonoBehaviour
{
	[SerializeField]
	private KeyCode hotKey;

	[SerializeField]
	private string fileNamePrefix;

	public KeyCode HotKey
	{
		get
		{
			return hotKey;
		}
		set
		{
			hotKey = value;
		}
	}

	public string FileNamePrefix
	{
		get
		{
			return fileNamePrefix;
		}
		set
		{
			fileNamePrefix = value;
		}
	}

	private void Reset()
	{
		HotKey = KeyCode.F9;
		FileNamePrefix = "Screenshot";
	}

	private void Update()
	{
		if (Input.GetKeyDown(HotKey))
		{
			StartCoroutine(CrTakeScreenshot());
		}
	}

	private IEnumerator CrTakeScreenshot()
	{
		yield return new WaitForEndOfFrame();
		Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, mipChain: false);
		texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
		texture.Apply();
		yield return 0;
		byte[] bytes = texture.EncodeToPNG();
		DateTime now = DateTime.Now;
		string text = $"{now.Year}-{now.Month}-{now.Day}-{now.Hour}-{now.Minute}-{now.Second}";
		string text2 = string.Format("{0}{1}{2}{3}", FileNamePrefix, (FileNamePrefix == null) ? "" : "-", text, ".png");
		string text3 = Application.dataPath + "/" + text2;
		File.WriteAllBytes(text3, bytes);
		UnityEngine.Object.Destroy(texture);
		Debug.Log("Screenshot saved at: " + text3);
	}
}
