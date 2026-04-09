using System;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace SalaryCalculator
{
    public class UpdateChecker
    {
        private const string GITHUB_API_RELEASE_URL = "https://api.github.com/repos/HuyTran1002/Salary/releases/latest";
        private static bool isShowingUpdateDialog = false;


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
                client.Timeout = System.TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("SalaryCalculator-Updater/2.0");

                var response = await client.GetStringAsync(GITHUB_API_RELEASE_URL);
                
                // Extract version from tag_name (e.g. "v2.0.1.0" or "2.0.1.0")
                string tagName = ExtractValueFromJson(response, "tag_name");
                string versionString = tagName.TrimStart('v', 'V');
                string downloadUrl = ExtractExeUrlFromJson(response);

                if (Version.TryParse(versionString, out var latestVersion))
                {
                    bool hasUpdate = latestVersion > CurrentVersion;
                    return (hasUpdate, versionString, downloadUrl);
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Path.GetTempPath(), "SalaryCalculator_update.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error during version check: {ex.Message}\n");
            }
            return (false, CurrentVersion.ToString(), null);
        }

        private static string ExtractValueFromJson(string json, string key)
        {
            var marker = $"\"{key}\":\"";
            int startIdx = json.IndexOf(marker);
            if (startIdx == -1) return "";
            
            startIdx += marker.Length;
            int endIdx = json.IndexOf("\"", startIdx);
            if (endIdx == -1) return "";
            
            return json.Substring(startIdx, endIdx - startIdx);
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
                MessageBox.Show($"Lỗi khi hiển thị cửa sổ cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ShowManualUpdateDialog(string latestVersion, string downloadUrl)
        {
            if (isShowingUpdateDialog) return;

            isShowingUpdateDialog = true;
            try
            {
                // Thay vì hiện MessageBox, hiện thẳng form cập nhật để tránh hỏi 2 lần
                ShowAutoUpdateDialog(latestVersion, downloadUrl);
            }
            finally
            {
                isShowingUpdateDialog = false;
            }
        }
    }
}
