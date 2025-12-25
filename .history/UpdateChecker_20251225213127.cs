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
        private const string UPDATE_XML_PATH = "update.xml";

        public static Version CurrentVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                return version ?? new Version(1, 0, 0);
            }
        }

        public static async Task<(bool hasUpdate, string latestVersion, string downloadUrl, string changelog, bool mandatory)> CheckForUpdateAsync()
        {
            try
            {
                // Đọc file update.xml local
                if (!File.Exists(UPDATE_XML_PATH))
                    return (false, CurrentVersion.ToString(), null, null, false);

                var doc = XDocument.Load(UPDATE_XML_PATH);
                var item = doc.Element("item");
                if (item == null) return (false, CurrentVersion.ToString(), null, null, false);

                var versionString = item.Element("version")?.Value.Trim();
                var downloadUrl = item.Element("url")?.Value.Trim();
                var changelog = item.Element("changelog")?.Value.Trim();
                var mandatoryStr = item.Element("mandatory")?.Value.Trim();
                bool mandatory = mandatoryStr == "true";

                if (Version.TryParse(versionString, out var latestVersion))
                {
                    bool hasUpdate = latestVersion > CurrentVersion;
                    return (hasUpdate, versionString, downloadUrl, changelog, mandatory);
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(Path.GetTempPath(), "SalaryCalculator_update.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error: {ex.Message}\n");
            }
            return (false, CurrentVersion.ToString(), null, null, false);
        }

        public static void ShowAutoUpdateDialog(string latestVersion, string downloadUrl, string changelog, bool mandatory)
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

        public static void ShowManualUpdateDialog(string latestVersion, string downloadUrl, string changelog, bool mandatory)
        {
            var result = MessageBox.Show(
                $"Phiên bản mới ({latestVersion}) có sẵn!\n\n" +
                $"Phiên bản hiện tại: {CurrentVersion}\n" +
                $"Phiên bản mới nhất: {latestVersion}\n\n" +
                $"Changelog: {changelog}\n\n" +
                $"Bạn có muốn tải phiên bản mới ngay bây giờ không?",
                "Cập Nhật Có Sẵn",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information
            );

            if (result == DialogResult.Yes)
            {
                ShowAutoUpdateDialog(latestVersion, downloadUrl, changelog, mandatory);
            }
        }
    }
}
