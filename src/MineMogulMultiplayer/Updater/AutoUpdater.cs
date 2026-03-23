using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using BepInEx.Logging;

namespace MineMogulMultiplayer.Updater
{
    /// <summary>
    /// Checks a private GitHub Release for a newer mod version on a background thread.
    /// If found, downloads and extracts the zip over the current mod files.
    /// The update takes effect on next game launch.
    /// Requires an <c>update_token.txt</c> file in the mod directory containing a
    /// GitHub fine-grained PAT with read access to the repo's contents and releases.
    /// </summary>
    public static class AutoUpdater
    {
        private const string Owner = "SeshBot-Development";
        private const string Repo = "MineMogulMultiplayer";
        private const string AssetName = "MineMogulMultiplayer-mod.zip";
        private const string ApiBase = "https://api.github.com";

        // Embedded read-only PAT (base64) — allows all mod users to receive updates
        // from the private repo without needing their own token file.
        private const string EmbeddedTokenB64 =
            "Z2l0aHViX3BhdF8xMUJIQUczQlkweGhtY29DS1JRYktlX3JjQ2V4a1JVcjQyQ3N5a1lzbUU5YXQ5WkFXZnU3Q1pVam1CbWZJTGQxcnBDVk9CWVlZQVU2Mk1PNWpH";

        private static ManualLogSource _log;

        /// <summary>Kick off the update check on a background thread so it never blocks the game.</summary>
        public static void CheckInBackground(string modDirectory, string currentVersion, ManualLogSource log)
        {
            _log = log;
            new Thread(() =>
            {
                try { Run(modDirectory, currentVersion); }
                catch (Exception ex) { _log?.LogWarning($"[Updater] {ex.GetType().Name}: {ex.Message}"); }
            })
            {
                IsBackground = true,
                Name = "ModAutoUpdater"
            }.Start();
        }

        private static void Run(string modDir, string currentVersion)
        {
            // ── 1. Resolve auth token: file override > embedded ─────────
            string token = null;
            var tokenPath = Path.Combine(modDir, "update_token.txt");
            if (File.Exists(tokenPath))
            {
                var fileToken = File.ReadAllText(tokenPath).Trim();
                if (!string.IsNullOrEmpty(fileToken))
                    token = fileToken;
            }
            if (string.IsNullOrEmpty(token))
            {
                try { token = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EmbeddedTokenB64)); }
                catch { }
            }
            if (string.IsNullOrEmpty(token))
            {
                _log.LogWarning("[Updater] No token available — auto-update disabled");
                return;
            }

            // TLS 1.2 required by GitHub
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // ── 2. Fetch latest release metadata ────────────────────────
            var releaseUrl = $"{ApiBase}/repos/{Owner}/{Repo}/releases/latest";
            var json = ApiGet(releaseUrl, token);
            if (json == null) return;

            // ── 3. Parse version tag ────────────────────────────────────
            var tagMatch = Regex.Match(json, @"""tag_name""\s*:\s*""([^""]+)""");
            if (!tagMatch.Success)
            {
                _log.LogWarning("[Updater] Could not parse release tag from response");
                return;
            }
            var remoteTag = tagMatch.Groups[1].Value.TrimStart('v');

            if (!TryParseVersion(remoteTag, out var remote) ||
                !TryParseVersion(currentVersion, out var local))
            {
                _log.LogWarning($"[Updater] Version parse failed — remote='{remoteTag}' local='{currentVersion}'");
                return;
            }

            if (remote <= local)
            {
                _log.LogInfo($"[Updater] Up to date ({currentVersion})");
                return;
            }

            _log.LogInfo($"[Updater] Update available: {currentVersion} -> {remoteTag}");

            // ── 4. Find the zip asset's API download URL ────────────────
            // GitHub release JSON embeds assets with "url" (API endpoint) and "name".
            // For private repos we must download via the API URL with Accept: application/octet-stream.
            var assetUrl = FindAssetApiUrl(json, AssetName);
            if (assetUrl == null)
            {
                _log.LogWarning($"[Updater] Release has no asset named '{AssetName}'");
                return;
            }

            // ── 5. Download the zip ─────────────────────────────────────
            var tempZip = Path.Combine(Path.GetTempPath(), $"mmmp_update_{Guid.NewGuid():N}.zip");
            try
            {
                DownloadAsset(assetUrl, token, tempZip);
                var size = new FileInfo(tempZip).Length;
                _log.LogInfo($"[Updater] Downloaded {size:N0} bytes");

                // ── 6. Extract over the mod directory ───────────────────
                ExtractOverwrite(tempZip, modDir);
                _log.LogInfo($"[Updater] Updated to v{remoteTag} — restart the game to apply");
            }
            finally
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            }
        }

        // ── HTTP helpers ────────────────────────────────────────────────

        private static string ApiGet(string url, string token)
        {
            try
            {
                using (var wc = new WebClient())
                {
                    wc.Headers[HttpRequestHeader.UserAgent] = "MineMogulMultiplayer-Updater";
                    wc.Headers[HttpRequestHeader.Authorization] = $"token {token}";
                    wc.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                    return wc.DownloadString(url);
                }
            }
            catch (WebException ex)
            {
                var statusCode = -1;
                var responseBody = "";
                if (ex.Response is HttpWebResponse hr)
                {
                    statusCode = (int)hr.StatusCode;
                    try
                    {
                        using (var sr = new StreamReader(hr.GetResponseStream()))
                            responseBody = sr.ReadToEnd();
                    }
                    catch { }
                }

                if (statusCode == 404)
                    _log.LogInfo("[Updater] No releases published yet");
                else
                    _log.LogWarning($"[Updater] API request failed (HTTP {statusCode}): {ex.Message}");

                if (!string.IsNullOrEmpty(responseBody))
                    _log.LogWarning($"[Updater] Response: {responseBody.Substring(0, Math.Min(500, responseBody.Length))}");

                return null;
            }
        }

        private static void DownloadAsset(string assetApiUrl, string token, string destPath)
        {
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.UserAgent] = "MineMogulMultiplayer-Updater";
                wc.Headers[HttpRequestHeader.Authorization] = $"token {token}";
                wc.Headers[HttpRequestHeader.Accept] = "application/octet-stream";
                wc.DownloadFile(assetApiUrl, destPath);
            }
        }

        // ── JSON parsing (minimal — avoids adding a JSON dependency) ────

        /// <summary>
        /// Finds the API URL for a release asset by name.
        /// The JSON has an "assets" array with objects containing "url" and "name".
        /// </summary>
        private static string FindAssetApiUrl(string json, string assetName)
        {
            // Walk through all "url" fields that point to /releases/assets/ and check the nearby "name"
            var assetBlocks = Regex.Matches(json,
                @"\{[^{}]*""name""\s*:\s*""[^""]*""[^{}]*""url""\s*:\s*""[^""]*""[^{}]*\}",
                RegexOptions.Singleline);

            foreach (Match block in assetBlocks)
            {
                var nameMatch = Regex.Match(block.Value, @"""name""\s*:\s*""([^""]+)""");
                if (!nameMatch.Success || nameMatch.Groups[1].Value != assetName) continue;

                var urlMatch = Regex.Match(block.Value, @"""url""\s*:\s*""(https://api\.github\.com/[^""]+)""");
                if (urlMatch.Success) return urlMatch.Groups[1].Value;
            }

            // Try reversed field order (url before name)
            assetBlocks = Regex.Matches(json,
                @"\{[^{}]*""url""\s*:\s*""[^""]*""[^{}]*""name""\s*:\s*""[^""]*""[^{}]*\}",
                RegexOptions.Singleline);

            foreach (Match block in assetBlocks)
            {
                var nameMatch = Regex.Match(block.Value, @"""name""\s*:\s*""([^""]+)""");
                if (!nameMatch.Success || nameMatch.Groups[1].Value != assetName) continue;

                var urlMatch = Regex.Match(block.Value, @"""url""\s*:\s*""(https://api\.github\.com/[^""]+)""");
                if (urlMatch.Success) return urlMatch.Groups[1].Value;
            }

            return null;
        }

        // ── Zip extraction ──────────────────────────────────────────────

        private static void ExtractOverwrite(string zipPath, string destDir)
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                foreach (var entry in archive.Entries)
                {
                    // Skip directory entries
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
            // Normalize: "0.1.0" → Version, "0.1" → add .0
            var parts = s.Split('.');
            if (parts.Length == 2) s += ".0";
            return Version.TryParse(s, out v);
        }
    }
}
