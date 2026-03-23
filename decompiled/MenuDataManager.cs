using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[DefaultExecutionOrder(-800)]
public class MenuDataManager : Singleton<MenuDataManager>
{
	[Serializable]
	private class SteamNewsResponse
	{
		public AppNews appnews;
	}

	[Serializable]
	private class AppNews
	{
		public NewsItem[] newsitems;
	}

	[Serializable]
	private class NewsItem
	{
		public string gid;

		public string title;

		public string url;

		public long date;

		public string contents;
	}

	private MenuData _menuDataCache;

	private string _latestNewsPost;

	private const string MenuDataFileName = "menu_data.json";

	private static int LatestNewMapPopupVersion = 1;

	protected override void Awake()
	{
		base.Awake();
		if (!(Singleton<MenuDataManager>.Instance != this))
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public IEnumerator ShouldShowLatestSteamNewsPost(Action<bool> onResult)
	{
		string uri = $"https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid={3846120}&count=1&maxlength=1";
		using UnityWebRequest req = UnityWebRequest.Get(uri);
		req.timeout = 15;
		yield return req.SendWebRequest();
		if (req.result != UnityWebRequest.Result.Success)
		{
			Debug.LogWarning("Steam news request failed: " + req.error);
			onResult?.Invoke(obj: false);
			yield break;
		}
		SteamNewsResponse steamNewsResponse = JsonUtility.FromJson<SteamNewsResponse>(req.downloadHandler.text);
		if (steamNewsResponse?.appnews?.newsitems != null && steamNewsResponse.appnews.newsitems.Length != 0)
		{
			_latestNewsPost = steamNewsResponse.appnews.newsitems[0].gid;
		}
		if (string.IsNullOrEmpty(_latestNewsPost))
		{
			onResult?.Invoke(obj: false);
			yield break;
		}
		if (_latestNewsPost == GetMenuData().MostRecentlyReadNewsPost)
		{
			onResult?.Invoke(obj: false);
			yield break;
		}
		onResult?.Invoke(obj: true);
	}

	public void MarkLatestNewsPostAsRead()
	{
		if (!string.IsNullOrEmpty(_latestNewsPost))
		{
			GetMenuData().MostRecentlyReadNewsPost = _latestNewsPost;
			SaveMenuData();
		}
	}

	public MenuData GetMenuData()
	{
		string fullMenuDataFilePath = GetFullMenuDataFilePath();
		if (!File.Exists(fullMenuDataFilePath))
		{
			_menuDataCache = new MenuData();
			_menuDataCache.NewMapPopupVersion = LatestNewMapPopupVersion;
			return _menuDataCache;
		}
		string json = File.ReadAllText(fullMenuDataFilePath);
		_menuDataCache = JsonUtility.FromJson<MenuData>(json);
		return _menuDataCache;
	}

	public void SaveMenuData()
	{
		if (_menuDataCache != null)
		{
			string contents = JsonUtility.ToJson(_menuDataCache, prettyPrint: true);
			File.WriteAllText(GetFullMenuDataFilePath(), contents);
		}
	}

	public string GetFullMenuDataFilePath()
	{
		return Path.Combine(Application.persistentDataPath, "menu_data.json");
	}

	public bool ShouldShowNewMapIcon()
	{
		if (Singleton<DemoManager>.Instance.IsDemoVersion)
		{
			return false;
		}
		return GetMenuData().NewMapPopupVersion < LatestNewMapPopupVersion;
	}

	public void HideNewMapIcon()
	{
		GetMenuData().NewMapPopupVersion = LatestNewMapPopupVersion;
		SaveMenuData();
	}
}
