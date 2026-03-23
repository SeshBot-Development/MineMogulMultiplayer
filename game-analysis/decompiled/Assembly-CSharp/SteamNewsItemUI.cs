using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x0200006D RID: 109
public class SteamNewsItemUI : MonoBehaviour
{
	// Token: 0x060002EE RID: 750 RVA: 0x0000E61B File Offset: 0x0000C81B
	private void Awake()
	{
		if (this.SnippetText)
		{
			this.SnippetText.richText = true;
		}
		if (this.TitleText)
		{
			this.TitleText.richText = true;
		}
		this.SetCoverVisible(false);
	}

	// Token: 0x060002EF RID: 751 RVA: 0x0000E658 File Offset: 0x0000C858
	public void SetData(string title, string dateLabel, string snippet, string openUrl, SteamNewsFetcher fetcher)
	{
		if (this.TitleText)
		{
			this.TitleText.text = title;
		}
		if (this.DateText)
		{
			this.DateText.text = dateLabel;
		}
		if (this.SnippetText)
		{
			this.SnippetText.text = snippet;
		}
		this._url = openUrl;
		if (this.ReadMoreButton)
		{
			this.ReadMoreButton.onClick.RemoveAllListeners();
			this.ReadMoreButton.onClick.AddListener(new UnityAction(this.OpenUrl));
		}
		if (this.CloseButton)
		{
			this.CloseButton.onClick.RemoveAllListeners();
			this.CloseButton.onClick.AddListener(new UnityAction(fetcher.CloseNewsPanel));
		}
	}

	// Token: 0x060002F0 RID: 752 RVA: 0x0000E72C File Offset: 0x0000C92C
	public void SetCoverTexture(Texture2D tex)
	{
		if (this.CoverImage)
		{
			this.CoverImage.texture = tex;
		}
		if (tex != null)
		{
			float num = (float)tex.width;
			float num2 = (float)tex.height;
			if (num > 0f && num2 > 0f)
			{
				this.AspectRatioFitter.aspectRatio = Mathf.Clamp(num / num2, 0.01f, 100f);
			}
		}
		this.SetCoverVisible(tex != null);
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x060002F1 RID: 753 RVA: 0x0000E7B2 File Offset: 0x0000C9B2
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)base.transform);
		yield break;
	}

	// Token: 0x060002F2 RID: 754 RVA: 0x0000E7C1 File Offset: 0x0000C9C1
	private void SetCoverVisible(bool v)
	{
		if (this.CoverImageRoot)
		{
			this.CoverImageRoot.SetActive(v);
			return;
		}
		if (this.CoverImage)
		{
			this.CoverImage.gameObject.SetActive(v);
		}
	}

	// Token: 0x060002F3 RID: 755 RVA: 0x0000E7FB File Offset: 0x0000C9FB
	private void OpenUrl()
	{
		if (!string.IsNullOrEmpty(this._url))
		{
			Application.OpenURL(this._url);
		}
	}

	// Token: 0x040002C1 RID: 705
	public RawImage CoverImage;

	// Token: 0x040002C2 RID: 706
	public GameObject CoverImageRoot;

	// Token: 0x040002C3 RID: 707
	public AspectRatioFitter AspectRatioFitter;

	// Token: 0x040002C4 RID: 708
	public TextMeshProUGUI TitleText;

	// Token: 0x040002C5 RID: 709
	public TextMeshProUGUI DateText;

	// Token: 0x040002C6 RID: 710
	public TextMeshProUGUI SnippetText;

	// Token: 0x040002C7 RID: 711
	public Button ReadMoreButton;

	// Token: 0x040002C8 RID: 712
	public Button CloseButton;

	// Token: 0x040002C9 RID: 713
	private string _url;
}
