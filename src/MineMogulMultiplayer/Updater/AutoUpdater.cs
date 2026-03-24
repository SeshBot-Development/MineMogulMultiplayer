using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using UnityEngine;

namespace MineMogulMultiplayer.Updater
{
    /// <summary>
    /// Checks GitHub Releases for a newer mod version synchronously at startup.
    /// If found, downloads and extracts the zip, then restarts the game so the
    /// new code is loaded on the same session — no double-launch needed.
    /// </summary>
    public static class AutoUpdater
    {
        private const string Owner = "SeshBot-Development";
        private const string Repo = "MineMogulMultiplayer";
        private const string AssetName = "MineMogulMultiplayer-mod.zip";
        private const string SteamAppId = "3846120";

        private static ManualLogSource _log;

        /// <summary>
        /// Synchronously checks for an update. If one is found, downloads it,
        /// extracts over the mod directory, and restarts the game.
        /// Returns true if the game is about to restart (caller should abort startup).
        /// </summary>
        public static bool CheckAndApply(string modDirectory, string currentVersion, ManualLogSource log)
        {
            _log = log;
            try
            {
                return Run(modDirectory, currentVersion);
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"[Updater] {ex.GetType().Name}: {ex.Message}");
                return false;
            }
        }

        /// <returns>true if game is restarting</returns>
        private static bool Run(string modDir, string currentVersion)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // ── 1. Fetch latest release metadata ────────────────────────
            var releaseUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
            _log.LogInfo("[Updater] Checking for updates...");
            var json = ApiGet(releaseUrl);
            if (json == null) return false;

            // ── 2. Parse version tag ────────────────────────────────────
            var tagMatch = Regex.Match(json, @"""tag_name""\s*:\s*""([^""]+)""");
            if (!tagMatch.Success)
            {
                _log.LogWarning("[Updater] Could not parse release tag from response");
                return false;
            }
            var remoteTag = tagMatch.Groups[1].Value.TrimStart('v');

            if (!TryParseVersion(remoteTag, out var remote) ||
                !TryParseVersion(currentVersion, out var local))
            {
                _log.LogWarning($"[Updater] Version parse failed — remote='{remoteTag}' local='{currentVersion}'");
                return false;
            }

            if (remote <= local)
            {
                _log.LogInfo($"[Updater] Up to date ({currentVersion})");
                return false;
            }

            _log.LogInfo($"[Updater] Update available: {currentVersion} -> {remoteTag}");

            // ── 3. Find the zip asset's browser download URL ────────────
            var downloadUrl = FindBrowserDownloadUrl(json, AssetName);
            if (downloadUrl == null)
            {
                _log.LogWarning($"[Updater] Release has no asset named '{AssetName}'");
                return false;
            }

            // ── 4. Download the zip ─────────────────────────────────────
            _log.LogInfo("[Updater] Downloading update...");
            var tempZip = Path.Combine(Path.GetTempPath(), $"mmmp_update_{Guid.NewGuid():N}.zip");
            try
            {
                Download(downloadUrl, tempZip);
                var size = new FileInfo(tempZip).Length;
                _log.LogInfo($"[Updater] Downloaded {size:N0} bytes");

                // ── 5. Extract over the mod directory ───────────────────
                ExtractOverwrite(tempZip, modDir);
                _log.LogInfo($"[Updater] Updated to v{remoteTag} — restarting game...");
            }
            finally
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            }

            // ── 6. Restart the game so the new DLL is loaded ────────────
            RestartGame();
            return true;
        }

        private static void RestartGame()
        {
            try
            {
                // Relaunch through Steam so BepInEx doorstop loads properly
                _log.LogInfo($"[Updater] Relaunching via Steam (AppId {SteamAppId})...");
                Process.Start($"steam://rungameid/{SteamAppId}");
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[Updater] Could not auto-restart: {ex.Message}");
            }
            Application.Quit();
        }

        // ── HTTP helpers ────────────────────────────────────────────────

        private static string ApiGet(string url)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.UserAgent] = "MineMogulMultiplayer-Updater";
                    wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                    return wc.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                var statusCode = -1;
                if (ex.Response is HttpWebResponse hr)
                    statusCode = (int)hr.StatusCode;

                if (statusCode == 404)
                    _log.LogInfo("[Updater] No releases published yet");
                else
                    _log.LogWarning($"[Updater] API request failed (HTTP {statusCode}): {ex.Message}");
                return null;
            }
        }

        private static void Download(string url, string destPath)
        {
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "MineMogulMultiplayer-Updater";
                wc.DownloadFile(url, destPath);
            }
        }

        // ── JSON parsing helpers ────────────────────────────────────────

        private static string FindBrowserDownloadUrl(string json, string assetName)
        {
            var pattern = $@"""name""\s*:\s*""{Regex.Escape(assetName)}""[^{{}}]*""browser_download_url""\s*:\s*""([^""]+)""";
            var m = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (m.Success) return m.Groups[1].Value;

            pattern = $@"""browser_download_url""\s*:\s*""([^""]+{Regex.Escape(assetName)}[^""]*)""";
            m = Regex.Match(json, pattern, RegexOptions.Singleline);
            if (m.Success) return m.Groups[1].Value;

            return null;
        }

        // ── Zip extraction ──────────────────────────────────────────────

        private static void ExtractOverwrite(string zipPath, string destDir)
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue;
                    var destFile = Path.Combine(destDir, entry.FullName);
                    var dir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);
                    entry.ExtractToFile(destFile, overwrite: true);
                }
            }
        }

        // ── Version helpers ─────────────────────────────────────────────

        private static bool TryParseVersion(string s, out Version v)
        {
            v = null;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split('.');
            if (parts.Length == 2) s += ".0";
            return Version.TryParse(s, out v);
        }
    }
}
