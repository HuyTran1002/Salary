using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

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

        public static async Task<(bool hasUpdate, string latestVersion)> CheckForUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                
                var versionString = await client.GetStringAsync(VERSION_CHECK_URL);
                versionString = versionString.Trim();
                
                // DEBUG: Ghi log
                string logPath = Path.Combine(Path.GetTempPath(), "CpuTempApp_update.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Checked version: {versionString}, Current: {CurrentVersion}\n");
                
                if (Version.TryParse(versionString, out var latestVersion))
                {
                    bool hasUpdate = latestVersion > CurrentVersion;
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Has update: {hasUpdate}\n");
                    return (hasUpdate, versionString);
                }
            }
            catch (Exception ex)
            {
                // DEBUG: Ghi log lỗi
                string logPath = Path.Combine(Path.GetTempPath(), "CpuTempApp_update.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error: {ex.Message}\n");
            }
            
            return (false, CurrentVersion.ToString());
        }

    public static void ShowAutoUpdateDialog(string latestVersion)
    {
        try
        {
            // Tạo URL cho file download trực tiếp từ Release
            // Format: https://github.com/HuyTran1002/CpuTempApp/releases/download/v1.0.2/CpuTempSetup.exe
            string downloadUrl = $"https://github.com/HuyTran1002/CpuTempApp/releases/download/v{latestVersion}/CpuTempSetup.exe";
            
            // Hiển thị form update tự động
            UpdateForm updateForm = new UpdateForm(latestVersion, downloadUrl);
            updateForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

        public static void ShowManualUpdateDialog(string latestVersion)
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
                ShowAutoUpdateDialog(latestVersion);
            }
        }
    }
}
