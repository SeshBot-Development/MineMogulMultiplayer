using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Token: 0x020000BB RID: 187
[DefaultExecutionOrder(-800)]
public class MenuDataManager : Singleton<MenuDataManager>
{
	// Token: 0x0600050E RID: 1294 RVA: 0x0001A8E2 File Offset: 0x00018AE2
	protected override void Awake()
	{
		base.Awake();
		if (Singleton<MenuDataManager>.Instance != this)
		{
			return;
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	// Token: 0x0600050F RID: 1295 RVA: 0x0001A903 File Offset: 0x00018B03
	public IEnumerator ShouldShowLatestSteamNewsPost(Action<bool> onResult)
	{
		string text = string.Format("https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={0}&count=1&maxlength=1", 3846120);
		using (UnityWebRequest req = UnityWebRequest.Get(text))
		{
			req.timeout = 15;
			yield return req.SendWebRequest();
			if (req.result != UnityWebRequest.Result.Success)
			{
				Debug.LogWarning("Steam news request failed: " + req.error);
				if (onResult != null)
				{
					onResult(false);
				}
				yield break;
			}
			MenuDataManager.SteamNewsResponse steamNewsResponse = JsonUtility.FromJson<MenuDataManager.SteamNewsResponse>(req.downloadHandler.text);
			bool flag;
			if (steamNewsResponse == null)
			{
				flag = null != null;
			}
			else
			{
				MenuDataManager.AppNews appnews = steamNewsResponse.appnews;
				flag = ((appnews != null) ? appnews.newsitems : null) != null;
			}
			if (flag && steamNewsResponse.appnews.newsitems.Length != 0)
			{
				this._latestNewsPost = steamNewsResponse.appnews.newsitems[0].gid;
			}
			if (string.IsNullOrEmpty(this._latestNewsPost))
			{
				if (onResult != null)
				{
					onResult(false);
				}
				yield break;
			}
			if (this._latestNewsPost == this.GetMenuData().MostRecentlyReadNewsPost)
			{
				if (onResult != null)
				{
					onResult(false);
				}
				yield break;
			}
			if (onResult != null)
			{
				onResult(true);
			}
		}
		UnityWebRequest req = null;
		yield break;
		yield break;
	}

	// Token: 0x06000510 RID: 1296 RVA: 0x0001A919 File Offset: 0x00018B19
	public void MarkLatestNewsPostAsRead()
	{
		if (string.IsNullOrEmpty(this._latestNewsPost))
		{
			return;
		}
		this.GetMenuData().MostRecentlyReadNewsPost = this._latestNewsPost;
		this.SaveMenuData();
	}

	// Token: 0x06000511 RID: 1297 RVA: 0x0001A940 File Offset: 0x00018B40
	public MenuData GetMenuData()
	{
		string fullMenuDataFilePath = this.GetFullMenuDataFilePath();
		if (!File.Exists(fullMenuDataFilePath))
		{
			this._menuDataCache = new MenuData();
			this._menuDataCache.NewMapPopupVersion = MenuDataManager.LatestNewMapPopupVersion;
			return this._menuDataCache;
		}
		string text = File.ReadAllText(fullMenuDataFilePath);
		this._menuDataCache = JsonUtility.FromJson<MenuData>(text);
		return this._menuDataCache;
	}

	// Token: 0x06000512 RID: 1298 RVA: 0x0001A998 File Offset: 0x00018B98
	public void SaveMenuData()
	{
		if (this._menuDataCache == null)
		{
			return;
		}
		string text = JsonUtility.ToJson(this._menuDataCache, true);
		File.WriteAllText(this.GetFullMenuDataFilePath(), text);
	}

	// Token: 0x06000513 RID: 1299 RVA: 0x0001A9C7 File Offset: 0x00018BC7
	public string GetFullMenuDataFilePath()
	{
		return Path.Combine(Application.persistentDataPath, "menu_data.json");
	}

	// Token: 0x06000514 RID: 1300 RVA: 0x0001A9D8 File Offset: 0x00018BD8
	public bool ShouldShowNewMapIcon()
	{
		return this.GetMenuData().NewMapPopupVersion < MenuDataManager.LatestNewMapPopupVersion;
	}

	// Token: 0x06000515 RID: 1301 RVA: 0x0001A9EC File Offset: 0x00018BEC
	public void HideNewMapIcon()
	{
		this.GetMenuData().NewMapPopupVersion = MenuDataManager.LatestNewMapPopupVersion;
		this.SaveMenuData();
	}

	// Token: 0x04000642 RID: 1602
	private MenuData _menuDataCache;

	// Token: 0x04000643 RID: 1603
	private string _latestNewsPost;

	// Token: 0x04000644 RID: 1604
	private const string MenuDataFileName = "menu_data.json";

	// Token: 0x04000645 RID: 1605
	private static int LatestNewMapPopupVersion = 1;

	// Token: 0x02000171 RID: 369
	[Serializable]
	private class SteamNewsResponse
	{
		// Token: 0x04000957 RID: 2391
		public MenuDataManager.AppNews appnews;
	}

	// Token: 0x02000172 RID: 370
	[Serializable]
	private class AppNews
	{
		// Token: 0x04000958 RID: 2392
		public MenuDataManager.NewsItem[] newsitems;
	}

	// Token: 0x02000173 RID: 371
	[Serializable]
	private class NewsItem
	{
		// Token: 0x04000959 RID: 2393
		public string gid;

		// Token: 0x0400095A RID: 2394
		public string title;

		// Token: 0x0400095B RID: 2395
		public string url;

		// Token: 0x0400095C RID: 2396
		public long date;

		// Token: 0x0400095D RID: 2397
		public string contents;
	}
}
