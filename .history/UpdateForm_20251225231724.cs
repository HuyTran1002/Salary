using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

namespace SalaryCalculator
{
    public partial class UpdateForm : Form
    {
        private string downloadUrl;
        private string installerPath;
        private HttpClient httpClient;
        private CancellationTokenSource cancellationTokenSource;
        private long totalBytes = 0;
        private long downloadedBytes = 0;

        public UpdateForm(string newVersion, string downloadUrl)
        {
            InitializeComponent();
            this.downloadUrl = downloadUrl;
            this.Text = "SalaryCalculator - Update";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 520;
            this.Height = 300;

            httpClient = new HttpClient();
            cancellationTokenSource = new CancellationTokenSource();

            // Setup form controls
            SetupControls(newVersion);
        }

        private void SetupControls(string newVersion)
        {
            // Title Label
            Label titleLabel = new Label();
            titleLabel.Text = "Phiên bản mới có sẵn!";
            titleLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(440, 30);
            this.Controls.Add(titleLabel);

            // Version Label
            Label versionLabel = new Label();
            versionLabel.Text = $"Phiên bản mới: {newVersion}";
            versionLabel.Font = new Font("Segoe UI", 10);
            versionLabel.Location = new Point(20, 55);
            versionLabel.Size = new Size(440, 25);
            this.Controls.Add(versionLabel);

            // Current Version Label
            Label currentVersionLabel = new Label();
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            currentVersionLabel.Text = $"Phiên bản hiện tại: {version?.Major}.{version?.Minor}.{version?.Build}.{version?.Revision}";
            currentVersionLabel.Font = new Font("Segoe UI", 10);
            currentVersionLabel.Location = new Point(20, 80);
            currentVersionLabel.Size = new Size(440, 25);
            this.Controls.Add(currentVersionLabel);

            // Progress Bar
            ProgressBar progressBar = new ProgressBar();
            progressBar.Name = "progressBar";
            progressBar.Location = new Point(20, 120);
            progressBar.Size = new Size(440, 30);
            progressBar.Style = ProgressBarStyle.Continuous;
            this.Controls.Add(progressBar);

            // Status Label
            Label statusLabel = new Label();
            statusLabel.Name = "statusLabel";
            statusLabel.Text = "Sẵn sàng để tải...";
            statusLabel.Font = new Font("Segoe UI", 9);
            statusLabel.Location = new Point(20, 155);
            statusLabel.Size = new Size(440, 20);
            statusLabel.ForeColor = Color.Gray;
            this.Controls.Add(statusLabel);

            // Download Button
            Button downloadBtn = new Button();
            downloadBtn.Name = "downloadBtn";
            downloadBtn.Text = "Update";
            downloadBtn.Location = new Point(140, 200);
            downloadBtn.Size = new Size(120, 35);
            downloadBtn.BackColor = Color.FromArgb(0, 120, 215);
            downloadBtn.ForeColor = Color.White;
            downloadBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            downloadBtn.Click += DownloadBtn_Click;
            this.Controls.Add(downloadBtn);

            // Cancel Button
            Button cancelBtn = new Button();
            cancelBtn.Text = "Hủy";
            cancelBtn.Location = new Point(280, 200);
            cancelBtn.Size = new Size(120, 35);
            cancelBtn.Font = new Font("Segoe UI", 10);
            cancelBtn.Click += CancelBtn_Click;
            this.Controls.Add(cancelBtn);
        }

        private async void DownloadBtn_Click(object sender, EventArgs e)
        {
            Button downloadBtn = (Button)sender;
            downloadBtn.Enabled = false;

            try
            {
                Label statusLabel = (Label)this.Controls["statusLabel"];
                statusLabel.Text = "Đang tải phiên bản mới...";
                this.Refresh();

                // Create temp folder for installer
                string tempPath = Path.Combine(Path.GetTempPath(), "SalaryCalculatorUpdate");
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);



                // Đường dẫn file exe mới tải về (file tạm)
                string newExePath = Path.Combine(tempPath, "SalaryCalculator_new.exe");
                await DownloadFileAsync(downloadUrl, newExePath);

                // Step 2: After download completes, run batch file để ghi đè exe gốc
                if (File.Exists(newExePath))
                {
                    statusLabel.Text = "Tải xong! Đang cập nhật...";
                    this.Refresh();
                    await Task.Delay(500);

                    // Tạo file batch để copy đè exe và khởi động lại app
                    string batchPath = Path.Combine(tempPath, "update_salary.bat");

                    // Xác định đường dẫn file gốc thông minh
                    string originalExePath = Application.ExecutablePath;
                    string exeName = Path.GetFileNameWithoutExtension(originalExePath);
                    // Nếu app chạy từ thư mục tạm, thử tìm shortcut trên desktop
                    string tempPathRoot = Path.GetTempPath().TrimEnd('\\');
                    if (originalExePath.StartsWith(tempPathRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        // Tìm shortcut trên desktop
                        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        string shortcutPath = Path.Combine(desktop, "SalaryCalculator.lnk");
                        if (File.Exists(shortcutPath))
                        {
                            try
                            {
                                // Lấy TargetPath từ shortcut
                                var shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                                var shortcut = shell.GetType().InvokeMember("CreateShortcut", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { shortcutPath });
                                var targetPath = shortcut.GetType().InvokeMember("TargetPath", System.Reflection.BindingFlags.GetProperty, null, shortcut, null) as string;
                                if (!string.IsNullOrEmpty(targetPath) && File.Exists(targetPath))
                                {
                                    originalExePath = targetPath;
                                    exeName = Path.GetFileNameWithoutExtension(originalExePath);
                                }
                            }
                            catch { /* Nếu lỗi thì giữ nguyên Application.ExecutablePath */ }
                        }
                    }
                    string batchContent = string.Join("\r\n", new[] {
                        "@echo off",
                        "timeout /t 2 >nul",
                        ":loop",
                        $"tasklist | findstr /i \"{exeName}.exe\" >nul",
                        "if not errorlevel 1 (",
                        "    timeout /t 1 >nul",
                        "    goto loop",
                        ")",
                        $"copy /y \"{newExePath}\" \"{originalExePath}\"",
                        $"start \"\" \"{originalExePath}\"",
                        $"del \"{newExePath}\"",
                        "del \"%~f0\""
                    });
                    File.WriteAllText(batchPath, batchContent);

                    // Chạy batch file và thoát app
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = batchPath,
                        UseShellExecute = true,
                        Verb = "runas"
                    });
                    Application.Exit();
                    Environment.Exit(0);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Tải xuống đã bị hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                downloadBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải xuống: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                downloadBtn.Enabled = true;
            }
        }

        private async Task DownloadFileAsync(string url, string filePath)
        {
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
            {
                response.EnsureSuccessStatusCode();

                totalBytes = response.Content.Headers.ContentLength ?? -1L;
                if (totalBytes == -1)
                {
                    // If content length is unknown, download without progress tracking
                    await response.Content.CopyToAsync(new FileStream(filePath, FileMode.Create, FileAccess.Write));
                    return;
                }

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true))
                {
                    downloadedBytes = 0;
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationTokenSource.Token);
                        downloadedBytes += bytesRead;

                        // Update UI
                        int percentage = (int)((downloadedBytes * 100) / totalBytes);
                        this.Invoke((MethodInvoker)(() =>
                        {
                            ProgressBar progressBar = (ProgressBar)this.Controls["progressBar"];
                            Label statusLabel = (Label)this.Controls["statusLabel"];

                            progressBar.Value = Math.Min(percentage, 100);
                            statusLabel.Text = $"Đang tải: {downloadedBytes / (1024 * 1024)}MB / {totalBytes / (1024 * 1024)}MB ({percentage}%)";
                        }));
                    }
                }
            }
        }

        private void RunInstaller()
        {
            try
            {
                // Run installer with /AUTOUPDATE parameter for silent uninstall
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = installerPath,
                    Arguments = "/AUTOUPDATE /VERYSILENT /NORESTART", // Silent mode for auto-update
                    UseShellExecute = true,
                    Verb = "runas" // Run as admin
                };

                Process.Start(psi);
                
                // Exit current app to let installer work
                Application.Exit();
                Environment.Exit(0); // Force immediate termination
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể chạy installer: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            httpClient?.Dispose();
            cancellationTokenSource?.Dispose();
            base.OnFormClosing(e);
        }
    }

    // Designer support
    partial class UpdateForm
    {
        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(500, 250);
            this.Name = "UpdateForm";
            this.Font = new Font("Segoe UI", 9F);
        }
    }
}
