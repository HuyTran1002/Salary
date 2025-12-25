using System;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace SalaryCalculator
{
    public class UpdateChecker
            /// <summary>
            /// Ghi đè file exe gốc bằng file mới tải về và cập nhật shortcut nếu có.
            /// </summary>
            /// <param name="downloadedExePath">Đường dẫn file exe mới tải về</param>
            /// <param name="originalExePath">Đường dẫn file exe gốc</param>
            /// <param name="shortcutPath">Đường dẫn shortcut (nếu có, có thể null)</param>
            public static void ReplaceOriginalExeAndUpdateShortcut(string downloadedExePath, string originalExePath, string shortcutPath = null)
            {
                try
                {
                    // Đóng app gốc nếu đang chạy (nếu cần, có thể bổ sung logic kiểm tra)
                    // Ghi đè file exe gốc
                    File.Copy(downloadedExePath, originalExePath, true);

                    // Nếu có shortcut, cập nhật shortcut trỏ về file exe gốc
                    if (!string.IsNullOrEmpty(shortcutPath) && File.Exists(shortcutPath))
                    {
                        // Sử dụng Windows Script Host để sửa shortcut
                        var shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                        var shortcut = shell.GetType().InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                        shortcut.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.SetProperty, null, shortcut, new object[] { originalExePath });
                        shortcut.GetType().InvokeMember("Save", System.Reflection.BindingFlags.InvokeMethod, null, shortcut, null);
                    }
                }
                catch (Exception ex)
                {
                    string logPath = Path.Combine(Path.GetTempPath(), "SalaryCalculator_update.log");
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Update error: {ex.Message}\n");
                    MessageBox.Show($"Lỗi khi cập nhật file gốc hoặc shortcut: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
    {
        private const string GITHUB_API_RELEASE_URL = "https://api.github.com/repos/HuyTran1002/Salary/releases/latest";

            public static async Task<string> GetLatestExeDownloadUrlAsync()
            {
                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("request");
                var json = await client.GetStringAsync(GITHUB_API_RELEASE_URL);
                // Đơn giản: tìm trường "browser_download_url" cho file .exe
                var exeUrl = ExtractExeUrlFromJson(json);
                return exeUrl;
            }

            private static string ExtractExeUrlFromJson(string json)
            {
                // Đơn giản dùng string search, nếu cần có thể dùng thư viện JSON
                var marker = "browser_download_url\":\"";
                var exeMarker = ".exe";
                int idx = json.IndexOf(marker);
                while (idx != -1)
                {
                    int start = idx + marker.Length;
                    int end = json.IndexOf(exeMarker, start);
                    if (end != -1)
                    {
                        end += exeMarker.Length;
                        var url = json.Substring(start, end - start);
                        if (url.EndsWith(".exe")) return url;
                    }
                    idx = json.IndexOf(marker, idx + 1);
                }
                return null;
            }
        private const string VERSION_CHECK_URL = "https://raw.githubusercontent.com/HuyTran1002/Salary/main/version.txt";
        private const string DOWNLOAD_URL = "https://github.com/HuyTran1002/Salary/releases/download/{0}/SalaryCalculator.exe";

        public static Version CurrentVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version ?? new Version(1, 0, 0);
            }
        }

        public static async Task<(bool hasUpdate, string latestVersion, string downloadUrl)> CheckForUpdateAsync()
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = System.TimeSpan.FromSeconds(5);
                var versionString = await client.GetStringAsync(VERSION_CHECK_URL);
                versionString = versionString.Trim();

                if (Version.TryParse(versionString, out var latestVersion))
                {
                    bool hasUpdate = latestVersion > CurrentVersion;
                    string downloadUrl = string.Format(DOWNLOAD_URL, versionString);
                    return (hasUpdate, versionString, downloadUrl);
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Path.GetTempPath(), "SalaryCalculator_update.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error: {ex.Message}\n");
            }
            return (false, CurrentVersion.ToString(), null);
        }

        public static void ShowAutoUpdateDialog(string latestVersion, string downloadUrl)
        {
            try
            {
                UpdateForm updateForm = new UpdateForm(latestVersion, downloadUrl);
                updateForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ShowManualUpdateDialog(string latestVersion, string downloadUrl)
        {
            var result = MessageBox.Show(
                $"Phiên bản mới ({latestVersion}) có sẵn!\n\n" +
                $"Phiên bản hiện tại: {CurrentVersion}\n" +
                $"Phiên bản mới nhất: {latestVersion}\n\n" +
                $"Bạn có muốn tải phiên bản mới ngay bây giờ không?",
                "Cập Nhật Có Sẵn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                ShowAutoUpdateDialog(latestVersion, downloadUrl);
            }
        }
    }
}
