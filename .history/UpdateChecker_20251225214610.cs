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
