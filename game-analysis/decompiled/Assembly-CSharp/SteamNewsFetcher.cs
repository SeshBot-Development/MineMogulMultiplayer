using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

// Token: 0x0200006C RID: 108
public class SteamNewsFetcher : MonoBehaviour
{
	// Token: 0x060002D6 RID: 726 RVA: 0x0000DC68 File Offset: 0x0000BE68
	public void CloseNewsPanel()
	{
		Singleton<MenuDataManager>.Instance.MarkLatestNewsPostAsRead();
		base.gameObject.SetActive(false);
	}

	// Token: 0x060002D7 RID: 727 RVA: 0x0000DC80 File Offset: 0x0000BE80
	public void ToggleNewsPanelAndMarkAsRead()
	{
		if (base.gameObject.activeSelf)
		{
			Singleton<MenuDataManager>.Instance.MarkLatestNewsPostAsRead();
		}
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}

	// Token: 0x060002D8 RID: 728 RVA: 0x0000DCB2 File Offset: 0x0000BEB2
	public void ToggleNewsPanel(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	// Token: 0x060002D9 RID: 729 RVA: 0x0000DCC0 File Offset: 0x0000BEC0
	private void OnEnable()
	{
		this.FetchAndPopulate();
	}

	// Token: 0x060002DA RID: 730 RVA: 0x0000DCC8 File Offset: 0x0000BEC8
	public void FetchAndPopulate()
	{
		if (SteamNewsFetcher._cache != null && Time.unscaledTime - SteamNewsFetcher._cacheTime < 120f)
		{
			this.PopulateUI(SteamNewsFetcher._cache);
			return;
		}
		base.StartCoroutine(this.FetchNewsCoroutine());
	}

	// Token: 0x060002DB RID: 731 RVA: 0x0000DCFC File Offset: 0x0000BEFC
	private IEnumerator<UnityWebRequestAsyncOperation> FetchNewsCoroutine()
	{
		string url = string.Format("https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={0}&feeds=steam_community_announcements&count={1}", this.AppId, this.Count);
		if (this.MaxLength > 0)
		{
			url += string.Format("&maxlength={0}", this.MaxLength);
		}
		using (UnityWebRequest req = UnityWebRequest.Get(url))
		{
			req.timeout = 15;
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Debug.LogWarning("Steam news fetch failed: " + req.error + " — " + url);
				this.PopulateUI(new List<SteamNewsFetcher.NewsItem>());
				yield break;
			}
			SteamNewsFetcher.SteamNewsRoot steamNewsRoot = JsonUtility.FromJson<SteamNewsFetcher.SteamNewsRoot>(req.downloadHandler.text);
			List<SteamNewsFetcher.NewsItem> list;
			if (steamNewsRoot == null)
			{
				list = null;
			}
			else
			{
				SteamNewsFetcher.AppNews appnews = steamNewsRoot.appnews;
				list = ((appnews != null) ? appnews.newsitems : null);
			}
			List<SteamNewsFetcher.NewsItem> list2 = list ?? new List<SteamNewsFetcher.NewsItem>();
			SteamNewsFetcher._cache = list2;
			SteamNewsFetcher._cacheTime = Time.unscaledTime;
			this.PopulateUI(list2);
		}
		UnityWebRequest req = null;
		yield break;
		yield break;
	}

	// Token: 0x060002DC RID: 732 RVA: 0x0000DD0C File Offset: 0x0000BF0C
	private void PopulateUI(List<SteamNewsFetcher.NewsItem> items)
	{
		for (int i = this.Container.childCount - 1; i >= 0; i--)
		{
			Object.Destroy(this.Container.GetChild(i).gameObject);
		}
		foreach (SteamNewsFetcher.NewsItem newsItem in items)
		{
			SteamNewsItemUI steamNewsItemUI = Object.Instantiate<SteamNewsItemUI>(this.NewsItemPrefab, this.Container);
			string text = SteamNewsFetcher.SafeDecode(newsItem.title);
			string text2 = SteamNewsFetcher.SafeDecode(newsItem.contents);
			string text3 = null;
			SteamNewsFetcher.TryExtractFirstImageUrl(text2, out text3);
			string text4 = SteamNewsFetcher.ConvertSteamToTmp(text2);
			text4 = SteamNewsFetcher.RemoveLeadingDemoHasLine(text4);
			if (this.LocalSnippetMaxChars > 0 && text4.Length > this.LocalSnippetMaxChars)
			{
				text4 = text4.Substring(0, this.LocalSnippetMaxChars) + "…";
			}
			string text5 = DateTimeOffset.FromUnixTimeSeconds((long)newsItem.date).ToLocalTime().DateTime.ToString("MMMM d, yyyy");
			string text6 = ((!string.IsNullOrEmpty(newsItem.url)) ? newsItem.url : SteamNewsFetcher.MakeCommunityUrl(newsItem));
			steamNewsItemUI.SetData(SteamNewsFetcher.MakeTitle(text), text5, text4, text6, this);
			if (!string.IsNullOrEmpty(text3))
			{
				base.StartCoroutine(this.LoadCoverImage(text3, steamNewsItemUI));
			}
			else
			{
				steamNewsItemUI.SetCoverTexture(null);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.Container);
		base.StartCoroutine(this.WaitThenRebuildLayout());
	}

	// Token: 0x060002DD RID: 733 RVA: 0x0000DEA4 File Offset: 0x0000C0A4
	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForSeconds(1f);
		if (base.gameObject == null || !base.gameObject.activeSelf)
		{
			yield break;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(this.Container);
		yield break;
	}

	// Token: 0x060002DE RID: 734 RVA: 0x0000DEB4 File Offset: 0x0000C0B4
	private static string SafeDecode(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		string text;
		try
		{
			text = WebUtility.HtmlDecode(s);
		}
		catch
		{
			text = s;
		}
		return text;
	}

	// Token: 0x060002DF RID: 735 RVA: 0x0000DEEC File Offset: 0x0000C0EC
	private static string MakeTitle(string t)
	{
		if (string.IsNullOrEmpty(t))
		{
			return t;
		}
		t = Regex.Replace(t, "\\[(\\/)?[a-zA-Z0-9]+(=[^\\]]+)?\\]", string.Empty);
		t = Regex.Replace(t, "<[^>]+>", string.Empty);
		return t.Trim();
	}

	// Token: 0x060002E0 RID: 736 RVA: 0x0000DF24 File Offset: 0x0000C124
	private static string ConvertSteamToTmp(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return SteamNewsFetcher.CollapseWhitespaceButKeepParagraphs(SteamNewsFetcher.NormalizeNewlines(Regex.Replace(SteamNewsFetcher.SafeDecode(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(SteamNewsFetcher.ConvertListsToBullets(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(SteamNewsFetcher.RemoveMediaBlocks(input), "<br\\s*\\/?>", "\n", RegexOptions.IgnoreCase), "\\[h1\\](.*?)\\[/h1\\]", (Match m) => "\n<b><size=130%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[h2\\](.*?)\\[/h2\\]", (Match m) => "\n<b><size=118%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[h3\\](.*?)\\[/h3\\]", (Match m) => "\n<b><size=108%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[(\\/)?h[1-3]\\]", string.Empty, RegexOptions.IgnoreCase), "\\[b\\](.*?)\\[/b\\]", "<b>$1</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[i\\](.*?)\\[/i\\]", "<i>$1</i>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[u\\](.*?)\\[/u\\]", "<u>$1</u>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[strike\\](.*?)\\[/strike\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[quote\\](.*?)\\[/quote\\]", delegate(Match m)
		{
			string text = SteamNewsFetcher.NormalizeNewlines(m.Groups[1].Value).Trim();
			text = Regex.Replace(text, "\\n", "\n> ");
			return "> " + text + "\n\n";
		}, RegexOptions.IgnoreCase | RegexOptions.Singleline)), "\\[url=(.+?)\\](.*?)\\[/url\\]", "$2", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[url\\](.*?)\\[/url\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[color=.*?\\](.*?)\\[/color\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[(\\/)?[a-zA-Z0-9]+(=[^\\]]+)?\\]", string.Empty)), "<(?!\\/?(?:b|i|u|size|color)(?:\\s|>|=)).*?>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline))).Trim();
	}

	// Token: 0x060002E1 RID: 737 RVA: 0x0000E0BD File Offset: 0x0000C2BD
	private static string RemoveLeadingDemoHasLine(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = SteamNewsFetcher.NormalizeNewlines(s);
		s = Regex.Replace(s, "^\\s*(?:<[^>]+>\\s*)*the\\s+demo\\s+has[^\\n]*\\n?", string.Empty, RegexOptions.IgnoreCase);
		s = Regex.Replace(s, "^\\s*\\n", string.Empty);
		return s.TrimStart();
	}

	// Token: 0x060002E2 RID: 738 RVA: 0x0000E0FC File Offset: 0x0000C2FC
	private static string RemoveMediaBlocks(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = Regex.Replace(s, "\\[img[^\\]]*\\].*?\\[/img\\]", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[img[^\\]]*\\]", " ", RegexOptions.IgnoreCase);
		s = Regex.Replace(s, "<img[^>]*>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[previewyoutube[^\\]]*\\].*?\\[/previewyoutube\\]", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[video[^\\]]*\\].*?\\[/video\\]", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[(?:timg|img_thumb)[^\\]]*\\].*?\\[\\/(?:timg|img_thumb)\\]", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = s.Replace("{STEAM_CLAN_IMAGE}", string.Empty);
		s = Regex.Replace(s, "[ \\t]{2,}", " ");
		return s;
	}

	// Token: 0x060002E3 RID: 739 RVA: 0x0000E1B0 File Offset: 0x0000C3B0
	private static string ConvertListsToBullets(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = Regex.Replace(s, "\\[(\\/)?list(=[^\\]]+)?\\]", string.Empty, RegexOptions.IgnoreCase);
		s = Regex.Replace(s, "\\[\\*\\]\\s*(.+?)\\s*\\[\\/\\*\\]", (Match m) => "• " + SteamNewsFetcher.NormalizeItemText(m.Groups[1].Value) + "\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[\\*\\]\\s*(.+?)(?=(\\[\\*\\]|\\[\\/\\*\\]|\\Z))", (Match m) => "• " + SteamNewsFetcher.NormalizeItemText(m.Groups[1].Value) + "\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[\\/\\*\\]", "\n", RegexOptions.IgnoreCase);
		return s;
	}

	// Token: 0x060002E4 RID: 740 RVA: 0x0000E24A File Offset: 0x0000C44A
	private static string NormalizeItemText(string t)
	{
		t = SteamNewsFetcher.NormalizeNewlines(t);
		t = Regex.Replace(t, "\\s+", " ");
		return t.Trim();
	}

	// Token: 0x060002E5 RID: 741 RVA: 0x0000E26C File Offset: 0x0000C46C
	private static string NormalizeNewlines(string s)
	{
		s = s.Replace("\r\n", "\n").Replace("\r", "\n");
		s = Regex.Replace(s, "\\n{3,}", "\n\n");
		return s;
	}

	// Token: 0x060002E6 RID: 742 RVA: 0x0000E2A2 File Offset: 0x0000C4A2
	private static string CollapseWhitespaceButKeepParagraphs(string s)
	{
		s = Regex.Replace(s, "[ \\t\\f\\v]+", " ");
		s = Regex.Replace(s, "[ \\t]+\\n", "\n");
		s = Regex.Replace(s, "\\n[ \\t]+", "\n");
		return s;
	}

	// Token: 0x060002E7 RID: 743 RVA: 0x0000E2DC File Offset: 0x0000C4DC
	private static bool IsUnsupportedInlineImageUrl(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return true;
		}
		string text = url.ToLowerInvariant();
		return Regex.IsMatch(text, "\\.(gif|gifv)(?:$|[?#])") || Regex.IsMatch(text, "\\.webp(?:$|[?#])") || Regex.IsMatch(text, "[?&#](?:format|type|f)=(?:gif|webp)\\b");
	}

	// Token: 0x060002E8 RID: 744 RVA: 0x0000E328 File Offset: 0x0000C528
	private static bool TryExtractFirstImageUrl(string raw, out string urlOut)
	{
		urlOut = null;
		if (string.IsNullOrEmpty(raw))
		{
			return false;
		}
		List<ValueTuple<int, string>> list = new List<ValueTuple<int, string>>();
		foreach (object obj in SteamNewsFetcher.kImgSrcBb.Matches(raw))
		{
			Match match = (Match)obj;
			if (match.Success)
			{
				list.Add(new ValueTuple<int, string>(match.Index, match.Groups["url"].Value));
			}
		}
		foreach (object obj2 in SteamNewsFetcher.kImgBlockBb.Matches(raw))
		{
			Match match2 = (Match)obj2;
			if (match2.Success)
			{
				list.Add(new ValueTuple<int, string>(match2.Index, match2.Groups["url"].Value));
			}
		}
		foreach (object obj3 in SteamNewsFetcher.kImgHtml.Matches(raw))
		{
			Match match3 = (Match)obj3;
			if (match3.Success)
			{
				list.Add(new ValueTuple<int, string>(match3.Index, match3.Groups["url"].Value));
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		list.Sort(([TupleElementNames(new string[] { "index", "url" })] ValueTuple<int, string> a, [TupleElementNames(new string[] { "index", "url" })] ValueTuple<int, string> b) => a.Item1.CompareTo(b.Item1));
		foreach (ValueTuple<int, string> valueTuple in list)
		{
			string text = SteamNewsFetcher.ExpandSteamTokenUrl(valueTuple.Item2);
			if (!SteamNewsFetcher.IsUnsupportedInlineImageUrl(text))
			{
				urlOut = text;
				return true;
			}
		}
		return false;
	}

	// Token: 0x060002E9 RID: 745 RVA: 0x0000E538 File Offset: 0x0000C738
	private static string ExpandSteamTokenUrl(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return url;
		}
		url = url.Replace("{STEAM_CLAN_IMAGE}", "https://clan.cloudflare.steamstatic.com/images");
		if (url.StartsWith("//"))
		{
			url = "https:" + url;
		}
		return url;
	}

	// Token: 0x060002EA RID: 746 RVA: 0x0000E571 File Offset: 0x0000C771
	private IEnumerator LoadCoverImage(string url, SteamNewsItemUI ui)
	{
		if (string.IsNullOrEmpty(url) || ui == null)
		{
			yield break;
		}
		Texture2D texture2D;
		if (SteamNewsFetcher._imageCache.TryGetValue(url, out texture2D) && texture2D != null)
		{
			ui.SetCoverTexture(texture2D);
			yield break;
		}
		using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
		{
			req.timeout = 15;
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Debug.Log("Cover image load failed: " + req.error + " — " + url);
				ui.SetCoverTexture(null);
				yield break;
			}
			Texture2D content = DownloadHandlerTexture.GetContent(req);
			SteamNewsFetcher._imageCache[url] = content;
			ui.SetCoverTexture(content);
		}
		UnityWebRequest req = null;
		yield break;
		yield break;
	}

	// Token: 0x060002EB RID: 747 RVA: 0x0000E587 File Offset: 0x0000C787
	private static string MakeCommunityUrl(SteamNewsFetcher.NewsItem item)
	{
		return string.Format("https://store.steampowered.com/news/app/{0}/view/{1}", item.appid, item.gid);
	}

	// Token: 0x040002B4 RID: 692
	[Header("Steam")]
	public int AppId = 3846120;

	// Token: 0x040002B5 RID: 693
	[Range(1f, 20f)]
	public int Count = 4;

	// Token: 0x040002B6 RID: 694
	public int MaxLength;

	// Token: 0x040002B7 RID: 695
	[Header("UI")]
	public RectTransform Container;

	// Token: 0x040002B8 RID: 696
	public SteamNewsItemUI NewsItemPrefab;

	// Token: 0x040002B9 RID: 697
	[Header("Formatting")]
	[Tooltip("Trim snippet to this many chars after formatting (0 = unlimited).")]
	public int LocalSnippetMaxChars = 1000;

	// Token: 0x040002BA RID: 698
	private static List<SteamNewsFetcher.NewsItem> _cache;

	// Token: 0x040002BB RID: 699
	private static float _cacheTime;

	// Token: 0x040002BC RID: 700
	private const float CacheTTLSeconds = 120f;

	// Token: 0x040002BD RID: 701
	private static readonly Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	// Token: 0x040002BE RID: 702
	private static readonly Regex kImgSrcBb = new Regex("\\[img[^\\]]*?\\bsrc\\s*=\\s*['\"]?(?<url>[^'\"\\]]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	// Token: 0x040002BF RID: 703
	private static readonly Regex kImgBlockBb = new Regex("\\[img[^\\]]*\\]\\s*(?<url>.*?)\\s*\\[/img\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	// Token: 0x040002C0 RID: 704
	private static readonly Regex kImgHtml = new Regex("<img[^>]*?\\bsrc\\s*=\\s*['\"](?<url>[^'\"]+)['\"][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	// Token: 0x0200013C RID: 316
	[Serializable]
	private class SteamNewsRoot
	{
		// Token: 0x040008AD RID: 2221
		public SteamNewsFetcher.AppNews appnews;
	}

	// Token: 0x0200013D RID: 317
	[Serializable]
	private class AppNews
	{
		// Token: 0x040008AE RID: 2222
		public int appid;

		// Token: 0x040008AF RID: 2223
		public List<SteamNewsFetcher.NewsItem> newsitems;

		// Token: 0x040008B0 RID: 2224
		public bool more;

		// Token: 0x040008B1 RID: 2225
		public int count;
	}

	// Token: 0x0200013E RID: 318
	[Serializable]
	private class NewsItem
	{
		// Token: 0x040008B2 RID: 2226
		public string gid;

		// Token: 0x040008B3 RID: 2227
		public string title;

		// Token: 0x040008B4 RID: 2228
		public string url;

		// Token: 0x040008B5 RID: 2229
		public bool is_external_url;

		// Token: 0x040008B6 RID: 2230
		public string author;

		// Token: 0x040008B7 RID: 2231
		public string contents;

		// Token: 0x040008B8 RID: 2232
		public string feedlabel;

		// Token: 0x040008B9 RID: 2233
		public int date;

		// Token: 0x040008BA RID: 2234
		public string feedname;

		// Token: 0x040008BB RID: 2235
		public int appid;
	}
}
