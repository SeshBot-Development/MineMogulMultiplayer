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
    /// If found, downloads the zip, extracts to a staging folder, launches a
    /// batch script that waits for the game to exit, copies files, and relaunches.
    /// On next startup, ApplyPendingUpdate cleans up the staging marker.
    /// </summary>
    public static class AutoUpdater
    {
        private const string Owner = "SeshBot-Development";
        private const string Repo = "MineMogulMultiplayer";
        private const string AssetName = "MineMogulMultiplayer-mod.zip";
        private const string SteamAppId = "3846120";
        private const string StagingFolderName = "_mmmp_update_staging";

        private static ManualLogSource _log;

        /// <summary>
        /// Call early in Awake to apply any pending staged update from a previous run.
        /// </summary>
        public static void ApplyPendingUpdate(string modDirectory, ManualLogSource log)
        {
            _log = log;
            try
            {
                var stagingDir = Path.Combine(modDirectory, StagingFolderName);
                if (!Directory.Exists(stagingDir)) return;

                _log.LogInfo("[Updater] Applying pending staged update...");
                foreach (var file in Directory.GetFiles(stagingDir, "*", SearchOption.AllDirectories))
                {
                    var relative = file.Substring(stagingDir.Length + 1);
                    var dest = Path.Combine(modDirectory, relative);
                    var destDir = Path.GetDirectoryName(dest);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);
                    try
                    {
                        File.Copy(file, dest, overwrite: true);
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning($"[Updater] Could not copy staged file '{relative}': {ex.Message}");
                    }
                }
                try { Directory.Delete(stagingDir, true); } catch { }
                _log.LogInfo("[Updater] Staged update applied successfully");
            }
            catch (Exception ex)
            {
                _log?.LogWarning($"[Updater] ApplyPendingUpdate failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronously checks for an update. If one is found, downloads it,
        /// stages the files, launches an updater script, and quits.
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
            _log.LogInfo($"[Updater] Downloading update from {downloadUrl}...");
            var tempZip = Path.Combine(Path.GetTempPath(), $"mmmp_update_{Guid.NewGuid():N}.zip");
            try
            {
                Download(downloadUrl, tempZip);
                var size = new FileInfo(tempZip).Length;
                _log.LogInfo($"[Updater] Downloaded {size:N0} bytes");

                // ── 5. Extract to staging folder ────────────────────────
                var stagingDir = Path.Combine(modDir, StagingFolderName);
                if (Directory.Exists(stagingDir))
                    Directory.Delete(stagingDir, true);
                Directory.CreateDirectory(stagingDir);

                ExtractOverwrite(tempZip, stagingDir);
                _log.LogInfo($"[Updater] Extracted to staging folder");

                // ── 6. Launch batch script to copy after game exits ─────
                LaunchUpdateScript(modDir, stagingDir);
                _log.LogInfo($"[Updater] Update to v{remoteTag} staged — restarting game...");
            }
            finally
            {
                try { if (File.Exists(tempZip)) File.Delete(tempZip); } catch { }
            }

            // ── 7. Quit so the batch script can copy files ──────────────
            Application.Quit();
            return true;
        }

        private static void LaunchUpdateScript(string modDir, string stagingDir)
        {
            var pid = Process.GetCurrentProcess().Id;
            var batPath = Path.Combine(Path.GetTempPath(), $"mmmp_update_{Guid.NewGuid():N}.bat");

            // Batch script: wait for game to exit, xcopy staged files, clean up, relaunch
            var script = $@"@echo off
echo Waiting for game to exit...
:waitloop
tasklist /FI ""PID eq {pid}"" 2>NUL | find ""{pid}"" >NUL
if not errorlevel 1 (
    timeout /t 1 /nobreak >NUL
    goto waitloop
)
echo Copying update files...
xcopy ""{stagingDir}\*"" ""{modDir}\"" /E /Y /Q >NUL
echo Cleaning up staging...
rmdir /S /Q ""{stagingDir}"" >NUL 2>&1
echo Relaunching game...
start """" ""steam://rungameid/{SteamAppId}""
del ""%~f0""
";
            File.WriteAllText(batPath, script);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);
            _log.LogInfo($"[Updater] Update script launched (PID wait: {pid})");
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
            var escaped = Regex.Escape(assetName);

            // Match individual JSON object blocks (handles one level of nesting, e.g. "uploader":{...})
            foreach (Match block in Regex.Matches(json, @"\{(?:[^{}]|\{[^{}]*\})*\}", RegexOptions.Singleline))
            {
                var a = block.Value;
                // Check if this block has a matching "name" or "label" field
                if (!Regex.IsMatch(a, $@"""(?:name|label)""\s*:\s*""{escaped}""")) continue;
                var urlM = Regex.Match(a, @"""browser_download_url""\s*:\s*""([^""]+)""");
                if (urlM.Success) return urlM.Groups[1].Value;
            }

            // Fallback: first .zip browser_download_url
            var fb = Regex.Match(json, @"""browser_download_url""\s*:\s*""([^""]+\.zip)""");
            return fb.Success ? fb.Groups[1].Value : null;
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
