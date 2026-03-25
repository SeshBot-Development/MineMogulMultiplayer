using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SteamNewsFetcher : MonoBehaviour
{
	[Serializable]
	private class SteamNewsRoot
	{
		public AppNews appnews;
	}

	[Serializable]
	private class AppNews
	{
		public int appid;

		public List<NewsItem> newsitems;

		public bool more;

		public int count;
	}

	[Serializable]
	private class NewsItem
	{
		public string gid;

		public string title;

		public string url;

		public bool is_external_url;

		public string author;

		public string contents;

		public string feedlabel;

		public int date;

		public string feedname;

		public int appid;
	}

	[Header("Steam")]
	public int AppId = 3846120;

	[Range(1f, 20f)]
	public int Count = 4;

	public int MaxLength;

	[Header("UI")]
	public RectTransform Container;

	public SteamNewsItemUI NewsItemPrefab;

	[Header("Formatting")]
	[Tooltip("Trim snippet to this many chars after formatting (0 = unlimited).")]
	public int LocalSnippetMaxChars = 1000;

	private static List<NewsItem> _cache;

	private static float _cacheTime;

	private const float CacheTTLSeconds = 120f;

	private static readonly Dictionary<string, Texture2D> _imageCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private static readonly Regex kImgSrcBb = new Regex("\\[img[^\\]]*?\\bsrc\\s*=\\s*['\"]?(?<url>[^'\"\\]]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	private static readonly Regex kImgBlockBb = new Regex("\\[img[^\\]]*\\]\\s*(?<url>.*?)\\s*\\[/img\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly Regex kImgHtml = new Regex("<img[^>]*?\\bsrc\\s*=\\s*['\"](?<url>[^'\"]+)['\"][^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

	public void CloseNewsPanel()
	{
		Singleton<MenuDataManager>.Instance.MarkLatestNewsPostAsRead();
		base.gameObject.SetActive(value: false);
	}

	public void ToggleNewsPanelAndMarkAsRead()
	{
		if (base.gameObject.activeSelf)
		{
			Singleton<MenuDataManager>.Instance.MarkLatestNewsPostAsRead();
		}
		base.gameObject.SetActive(!base.gameObject.activeSelf);
	}

	public void ToggleNewsPanel(bool enabled)
	{
		base.gameObject.SetActive(enabled);
	}

	private void OnEnable()
	{
		FetchAndPopulate();
	}

	public void FetchAndPopulate()
	{
		if (_cache != null && Time.unscaledTime - _cacheTime < 120f)
		{
			PopulateUI(_cache);
		}
		else
		{
			StartCoroutine(FetchNewsCoroutine());
		}
	}

	private IEnumerator<UnityWebRequestAsyncOperation> FetchNewsCoroutine()
	{
		string url = $"https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={AppId}&feeds=steam_community_announcements&count={Count}";
		if (MaxLength > 0)
		{
			url += $"&maxlength={MaxLength}";
		}
		using UnityWebRequest req = UnityWebRequest.Get(url);
		req.timeout = 15;
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Debug.LogWarning("Steam news fetch failed: " + req.error + " — " + url);
			PopulateUI(new List<NewsItem>());
			yield break;
		}
		List<NewsItem> items = (_cache = JsonUtility.FromJson<SteamNewsRoot>(req.downloadHandler.text)?.appnews?.newsitems ?? new List<NewsItem>());
		_cacheTime = Time.unscaledTime;
		PopulateUI(items);
	}

	private void PopulateUI(List<NewsItem> items)
	{
		for (int num = Container.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(Container.GetChild(num).gameObject);
		}
		foreach (NewsItem item in items)
		{
			SteamNewsItemUI steamNewsItemUI = UnityEngine.Object.Instantiate(NewsItemPrefab, Container);
			string t = SafeDecode(item.title);
			string text = SafeDecode(item.contents);
			string urlOut = null;
			TryExtractFirstImageUrl(text, out urlOut);
			string s = ConvertSteamToTmp(text);
			s = RemoveLeadingDemoHasLine(s);
			if (LocalSnippetMaxChars > 0 && s.Length > LocalSnippetMaxChars)
			{
				s = s.Substring(0, LocalSnippetMaxChars) + "…";
			}
			string dateLabel = DateTimeOffset.FromUnixTimeSeconds(item.date).ToLocalTime().DateTime.ToString("MMMM d, yyyy");
			string openUrl = ((!string.IsNullOrEmpty(item.url)) ? item.url : MakeCommunityUrl(item));
			steamNewsItemUI.SetData(MakeTitle(t), dateLabel, s, openUrl, this);
			if (!string.IsNullOrEmpty(urlOut))
			{
				StartCoroutine(LoadCoverImage(urlOut, steamNewsItemUI));
			}
			else
			{
				steamNewsItemUI.SetCoverTexture(null);
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(Container);
		StartCoroutine(WaitThenRebuildLayout());
	}

	private IEnumerator WaitThenRebuildLayout()
	{
		yield return new WaitForSeconds(1f);
		if (!(base.gameObject == null) && base.gameObject.activeSelf)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(Container);
		}
	}

	private static string SafeDecode(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		try
		{
			return WebUtility.HtmlDecode(s);
		}
		catch
		{
			return s;
		}
	}

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

	private static string ConvertSteamToTmp(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return CollapseWhitespaceButKeepParagraphs(NormalizeNewlines(Regex.Replace(SafeDecode(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(ConvertListsToBullets(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(RemoveMediaBlocks(input), "<br\\s*\\/?>", "\n", RegexOptions.IgnoreCase), "\\[h1\\](.*?)\\[/h1\\]", (Match m) => "\n<b><size=130%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[h2\\](.*?)\\[/h2\\]", (Match m) => "\n<b><size=118%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[h3\\](.*?)\\[/h3\\]", (Match m) => "\n<b><size=108%>" + m.Groups[1].Value + "</size></b>\n", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[(\\/)?h[1-3]\\]", string.Empty, RegexOptions.IgnoreCase), "\\[b\\](.*?)\\[/b\\]", "<b>$1</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[i\\](.*?)\\[/i\\]", "<i>$1</i>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[u\\](.*?)\\[/u\\]", "<u>$1</u>", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[strike\\](.*?)\\[/strike\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[quote\\](.*?)\\[/quote\\]", delegate(Match m)
		{
			string input2 = NormalizeNewlines(m.Groups[1].Value).Trim();
			input2 = Regex.Replace(input2, "\\n", "\n> ");
			return "> " + input2 + "\n\n";
		}, RegexOptions.IgnoreCase | RegexOptions.Singleline)), "\\[url=(.+?)\\](.*?)\\[/url\\]", "$2", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[url\\](.*?)\\[/url\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[color=.*?\\](.*?)\\[/color\\]", "$1", RegexOptions.IgnoreCase | RegexOptions.Singleline), "\\[(\\/)?[a-zA-Z0-9]+(=[^\\]]+)?\\]", string.Empty)), "<(?!\\/?(?:b|i|u|size|color)(?:\\s|>|=)).*?>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline))).Trim();
	}

	private static string RemoveLeadingDemoHasLine(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = NormalizeNewlines(s);
		s = Regex.Replace(s, "^\\s*(?:<[^>]+>\\s*)*the\\s+demo\\s+has[^\\n]*\\n?", string.Empty, RegexOptions.IgnoreCase);
		s = Regex.Replace(s, "^\\s*\\n", string.Empty);
		return s.TrimStart();
	}

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

	private static string ConvertListsToBullets(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return s;
		}
		s = Regex.Replace(s, "\\[(\\/)?list(=[^\\]]+)?\\]", string.Empty, RegexOptions.IgnoreCase);
		s = Regex.Replace(s, "\\[\\*\\]\\s*(.+?)\\s*\\[\\/\\*\\]", (Match m) => "• " + NormalizeItemText(m.Groups[1].Value) + "\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[\\*\\]\\s*(.+?)(?=(\\[\\*\\]|\\[\\/\\*\\]|\\Z))", (Match m) => "• " + NormalizeItemText(m.Groups[1].Value) + "\n", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		s = Regex.Replace(s, "\\[\\/\\*\\]", "\n", RegexOptions.IgnoreCase);
		return s;
	}

	private static string NormalizeItemText(string t)
	{
		t = NormalizeNewlines(t);
		t = Regex.Replace(t, "\\s+", " ");
		return t.Trim();
	}

	private static string NormalizeNewlines(string s)
	{
		s = s.Replace("\r\n", "\n").Replace("\r", "\n");
		s = Regex.Replace(s, "\\n{3,}", "\n\n");
		return s;
	}

	private static string CollapseWhitespaceButKeepParagraphs(string s)
	{
		s = Regex.Replace(s, "[ \\t\\f\\v]+", " ");
		s = Regex.Replace(s, "[ \\t]+\\n", "\n");
		s = Regex.Replace(s, "\\n[ \\t]+", "\n");
		return s;
	}

	private static bool IsUnsupportedInlineImageUrl(string url)
	{
		if (string.IsNullOrEmpty(url))
		{
			return true;
		}
		string input = url.ToLowerInvariant();
		if (Regex.IsMatch(input, "\\.(gif|gifv)(?:$|[?#])"))
		{
			return true;
		}
		if (Regex.IsMatch(input, "\\.webp(?:$|[?#])"))
		{
			return true;
		}
		if (Regex.IsMatch(input, "[?&#](?:format|type|f)=(?:gif|webp)\\b"))
		{
			return true;
		}
		return false;
	}

	private static bool TryExtractFirstImageUrl(string raw, out string urlOut)
	{
		urlOut = null;
		if (string.IsNullOrEmpty(raw))
		{
			return false;
		}
		List<(int, string)> list = new List<(int, string)>();
		foreach (Match item in kImgSrcBb.Matches(raw))
		{
			if (item.Success)
			{
				list.Add((item.Index, item.Groups["url"].Value));
			}
		}
		foreach (Match item2 in kImgBlockBb.Matches(raw))
		{
			if (item2.Success)
			{
				list.Add((item2.Index, item2.Groups["url"].Value));
			}
		}
		foreach (Match item3 in kImgHtml.Matches(raw))
		{
			if (item3.Success)
			{
				list.Add((item3.Index, item3.Groups["url"].Value));
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		list.Sort(((int index, string url) a, (int index, string url) b) => a.index.CompareTo(b.index));
		foreach (var item4 in list)
		{
			string text = ExpandSteamTokenUrl(item4.Item2);
			if (!IsUnsupportedInlineImageUrl(text))
			{
				urlOut = text;
				return true;
			}
		}
		return false;
	}

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

	private IEnumerator LoadCoverImage(string url, SteamNewsItemUI ui)
	{
		if (string.IsNullOrEmpty(url) || ui == null)
		{
			yield break;
		}
		if (_imageCache.TryGetValue(url, out var value) && value != null)
		{
			ui.SetCoverTexture(value);
			yield break;
		}
		using UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
		req.timeout = 15;
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Debug.Log("Cover image load failed: " + req.error + " — " + url);
			ui.SetCoverTexture(null);
			yield break;
		}
		Texture2D content = DownloadHandlerTexture.GetContent(req);
		_imageCache[url] = content;
		ui.SetCoverTexture(content);
	}

	private static string MakeCommunityUrl(NewsItem item)
	{
		return $"https://store.steampowered.com/news/app/{item.appid}/view/{item.gid}";
	}
}
