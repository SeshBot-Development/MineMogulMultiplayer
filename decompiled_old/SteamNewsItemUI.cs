using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SteamNewsItemUI : MonoBehaviour
{
	public RawImage CoverImage;

	public GameObject CoverImageRoot;

	public AspectRatioFitter AspectRatioFitter;

	public TextMeshProUGUI TitleText;

	public TextMeshProUGUI DateText;

	public TextMeshProUGUI SnippetText;

	public Button ReadMoreButton;

	public Button CloseButton;

	private string _url;

	private void Awake()
	{
		if ((bool)SnippetText)
		{
			SnippetText.richText = true;
		}
		if ((bool)TitleText)
		{
			TitleText.richText = true;
		}
		SetCoverVisible(v: false);
	}

	public void SetData(string title, string dateLabel, string snippet, string openUrl, SteamNewsFetcher fetcher)
	{
		if ((bool)TitleText)
		{
			TitleText.text = title;
		}
		if ((bool)DateText)
		{
			DateText.text = dateLabel;
		}
		if ((bool)SnippetText)
		{
			SnippetText.text = snippet;
		}
		_url = openUrl;
		if ((bool)ReadMoreButton)
		{
			ReadMoreButton.onClick.RemoveAllListeners();
			ReadMoreButton.onClick.AddListener(OpenUrl);
		}
		if ((bool)CloseButton)
		{
			CloseButton.onClick.RemoveAllListeners();
			CloseButton.onClick.AddListener(fetcher.CloseNewsPanel);
		}
	}

	public void SetCoverTexture(Texture2D tex)
	{
		if ((bool)CoverImage)
		{
			CoverImage.texture = tex;
		}
		if (tex != null)
		{
			float num = tex.width;
			float num2 = tex.height;
			if (num > 0f && num2 > 0f)
			{
				AspectRatioFitter.aspectRatio = Mathf.Clamp(num / num2, 0.01f, 100f);
			}
		}
		SetCoverVisible(tex != null);
		StartCoroutine(WaitThenRebuildLayout());
	}

	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
	}

	private void SetCoverVisible(bool v)
	{
		if ((bool)CoverImageRoot)
		{
			CoverImageRoot.SetActive(v);
		}
		else if ((bool)CoverImage)
		{
			CoverImage.gameObject.SetActive(v);
		}
	}

	private void OpenUrl()
	{
		if (!string.IsNullOrEmpty(_url))
		{
			Application.OpenURL(_url);
		}
	}
}
