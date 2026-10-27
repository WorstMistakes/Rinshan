using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Rinshan
{
    // A newer release published on GitHub
    class UpdateInfo
    {
        public Version Version;
        public string PageUrl;      // the release page, for the notes
        public string ZipUrl;
        public long Size;
        public string Sha256;       // GitHub's asset digest, when it has one
    }

    // Updates are the same zip people download by hand: the files next to Rinshan.exe are swapped
    // for the new ones and the app restarts. Settings and the Tenhou login live in %LOCALAPPDATA%.
    static class Updater
    {
        const string LatestReleaseApi = "https://api.github.com/repos/WorstMistakes/Rinshan/releases/latest";

        public static Version Current
        {
            get { return typeof(Updater).Assembly.GetName().Version; }
        }

        public static string CurrentText
        {
            get { return Format(Current); }
        }

        public static string Format(Version v)
        {
            return v.Major + "." + v.Minor + "." + Math.Max(v.Build, 0);
        }

        static string AppDirectory
        {
            get { return Path.GetDirectoryName(Application.ExecutablePath); }
        }

        static WebClient Client()
        {
            // Without a target framework in the exe, .NET defaults to TLS 1.0, which GitHub refuses
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var client = new WebClient();
            client.Headers[HttpRequestHeader.UserAgent] = "Rinshan/" + CurrentText;
            return client;
        }

        // Returns null when this is already the latest version
        public static async Task<UpdateInfo> CheckAsync()
        {
            string json;
            using (var client = Client())
            {
                client.Headers[HttpRequestHeader.Accept] = "application/vnd.github+json";
                json = await client.DownloadStringTaskAsync(LatestReleaseApi);
            }
            var release = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            Version version;
            if (!Version.TryParse(Convert.ToString(release["tag_name"]).TrimStart('v', 'V'), out version)) return null;
            if (Normalize(version) <= Normalize(Current)) return null;

            foreach (Dictionary<string, object> asset in (ArrayList)release["assets"])
            {
                var name = Convert.ToString(asset["name"]);
                if (!name.StartsWith("Rinshan", StringComparison.OrdinalIgnoreCase) ||
                    !name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) continue;
                object digest;
                asset.TryGetValue("digest", out digest);
                var sha = Convert.ToString(digest);
                var url = Convert.ToString(asset["browser_download_url"]);
                if (!url.StartsWith("https://github.com/")) return null;
                return new UpdateInfo
                {
                    Version = version,
                    PageUrl = "https://github.com/WorstMistakes/Rinshan/releases/tag/" + Uri.EscapeDataString(Convert.ToString(release["tag_name"])),
                    ZipUrl = url,
                    Size = Convert.ToInt64(asset["size"]),
                    Sha256 = sha.StartsWith("sha256:") ? sha.Substring(7) : null
                };
            }
            return null;   // a release without the zip yet (still uploading)
        }

        static Version Normalize(Version v)
        {
            return new Version(v.Major, v.Minor, Math.Max(v.Build, 0));
        }

        // Downloads and swaps the files. Returns the path of the new exe, to start once this one has exited.
        public static async Task<string> InstallAsync(UpdateInfo update)
        {
            var work = Path.Combine(Path.GetTempPath(), "Rinshan-update");
            if (Directory.Exists(work)) Directory.Delete(work, true);
            Directory.CreateDirectory(work);
            var zip = Path.Combine(work, "update.zip");
            using (var client = Client())
                await client.DownloadFileTaskAsync(update.ZipUrl, zip);

            if (new FileInfo(zip).Length != update.Size) throw new InvalidDataException("Incomplete download");
            if (update.Sha256 != null && !string.Equals(Sha256Of(zip), update.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("The download doesn't match the release");

            var extracted = Path.Combine(work, "files");
            ZipFile.ExtractToDirectory(zip, extracted);
            // The zip holds a Rinshan\ folder; take whichever folder has the exe
            var exe = Directory.GetFiles(extracted, "Rinshan.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (exe == null) throw new InvalidDataException("Rinshan.exe is missing from the release");

            Replace(Path.GetDirectoryName(exe), AppDirectory);
            return Path.Combine(AppDirectory, "Rinshan.exe");
        }

        static string Sha256Of(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        // Files in use (the exe and its DLLs) can't be overwritten but can be renamed, so the current ones
        // become *.old and are deleted on the next start. If anything fails, the old files are put back.
        static void Replace(string source, string target)
        {
            var done = new List<string>();
            try
            {
                foreach (var file in Directory.GetFiles(source))
                {
                    var dest = Path.Combine(target, Path.GetFileName(file));
                    var old = dest + ".old";
                    if (File.Exists(old)) File.Delete(old);
                    if (File.Exists(dest)) File.Move(dest, old);
                    done.Add(dest);
                    File.Copy(file, dest);
                }
            }
            catch
            {
                foreach (var dest in done)
                {
                    try
                    {
                        if (File.Exists(dest)) File.Delete(dest);
                        if (File.Exists(dest + ".old")) File.Move(dest + ".old", dest);
                    }
                    catch { }
                }
                throw;
            }
        }

        // Leftovers from the previous update
        public static void CleanUp()
        {
            try
            {
                foreach (var old in Directory.GetFiles(AppDirectory, "*.old"))
                    try { File.Delete(old); } catch { }
                var work = Path.Combine(Path.GetTempPath(), "Rinshan-update");
                if (Directory.Exists(work)) Directory.Delete(work, true);
            }
            catch { }
        }
    }
}
