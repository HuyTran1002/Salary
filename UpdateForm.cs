using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
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
        private const int BUFFER_SIZE = 65536; // 64KB buffer for faster download
        private const int MAX_RETRIES = 3;
        private DateTime lastUpdateTime = DateTime.Now;
        private long lastDownloadedBytes = 0;

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

            // Optimize HttpClient for faster downloads
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = true,
                MaxConnectionsPerServer = 10,
                UseCookies = false
            };
            
            httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromMinutes(30) // Longer timeout for large files
            };
            
            // Add headers for better compatibility and speed
            httpClient.DefaultRequestHeaders.Add("User-Agent", "SalaryCalculator-Updater/2.0");
            httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep-alive
            
            cancellationTokenSource = new CancellationTokenSource();

            // Setup form controls
            SetupControls(newVersion);
            try { Theme.ApplyEcommerceTheme(this); } catch { }
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

                // Step 2: After download completes, run silent script to replace exe
                if (File.Exists(newExePath))
                {
                    statusLabel.Text = "Tải xong! Đang cập nhật ẩn...";
                    this.Refresh();
                    await Task.Delay(500);

                    // Robust executable path detection
                    string originalExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    
                    // Escape single quotes for PowerShell strings
                    string escapedNewPath = newExePath.Replace("'", "''");
                    string escapedOriginalPath = originalExePath.Replace("'", "''");

                    // Create PowerShell command for silent execution
                    // We use PowerShell as it handles UTF-8 better and can be run hidden
                    string psCommand = $"Start-Sleep -s 2; $success = $false; for ($i=1; $i -le 10; $i++) {{ try {{ Copy-Item -Path '{escapedNewPath}' -Destination '{escapedOriginalPath}' -Force -ErrorAction Stop; $success = $true; break; }} catch {{ Start-Sleep -s 1; }} }}; if ($success) {{ Start-Process -FilePath '{escapedOriginalPath}'; }}; Remove-Item -Path '{escapedNewPath}';";

                    // Start PowerShell silently
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -WindowStyle Hidden -Command \"{psCommand}\"",
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
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount < MAX_RETRIES)
            {
                try
                {
                    await DownloadFileWithProgressAsync(url, filePath);
                    return; // Success
                }
                catch (Exception ex) when (retryCount < MAX_RETRIES - 1 && IsRetryableException(ex))
                {
                    lastException = ex;
                    retryCount++;
                    
                    // Show retry message
                    this.Invoke((MethodInvoker)(() =>
                    {
                        Label statusLabel = (Label)this.Controls["statusLabel"];
                        statusLabel.Text = $"Kết nối bị gián đoạn. Đang thử lại ({retryCount}/{MAX_RETRIES})...";
                    }));
                    
                    await Task.Delay(1000 * retryCount); // Exponential backoff
                }
            }
            
            // If we got here, all retries failed
            throw lastException ?? new Exception("Download failed after multiple retries");
        }

        private bool IsRetryableException(Exception ex)
        {
            return ex is HttpRequestException || 
                   ex is TaskCanceledException || 
                   ex is IOException;
        }

        private async Task DownloadFileWithProgressAsync(string url, string filePath)
        {
            using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationTokenSource.Token))
            {
                response.EnsureSuccessStatusCode();

                totalBytes = response.Content.Headers.ContentLength ?? -1L;
                if (totalBytes == -1)
                {
                    // If content length is unknown, download without progress tracking
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, useAsync: true))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    return;
                }

                using (var contentStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE, useAsync: true))
                {
                    downloadedBytes = 0;
                    lastDownloadedBytes = 0;
                    lastUpdateTime = DateTime.Now;
                    byte[] buffer = new byte[BUFFER_SIZE]; // 64KB buffer
                    int bytesRead;
                    int uiUpdateCounter = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationTokenSource.Token)) != 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationTokenSource.Token);
                        downloadedBytes += bytesRead;
                        uiUpdateCounter++;

                        // Update UI less frequently (every 8 iterations or ~512KB) to reduce overhead
                        if (uiUpdateCounter >= 8)
                        {
                            uiUpdateCounter = 0;
                            UpdateDownloadProgress();
                        }
                    }
                    
                    // Final update
                    UpdateDownloadProgress();
                }
            }
        }

        private void UpdateDownloadProgress()
        {
            int percentage = (int)((downloadedBytes * 100) / totalBytes);
            
            // Calculate download speed
            DateTime now = DateTime.Now;
            double elapsedSeconds = (now - lastUpdateTime).TotalSeconds;
            double speed = 0;
            
            if (elapsedSeconds > 0)
            {
                speed = (downloadedBytes - lastDownloadedBytes) / elapsedSeconds / (1024 * 1024); // MB/s
                lastUpdateTime = now;
                lastDownloadedBytes = downloadedBytes;
            }

            this.Invoke((MethodInvoker)(() =>
            {
                ProgressBar progressBar = (ProgressBar)this.Controls["progressBar"];
                Label statusLabel = (Label)this.Controls["statusLabel"];

                progressBar.Value = Math.Min(percentage, 100);
                
                double downloadedMB = downloadedBytes / (1024.0 * 1024.0);
                double totalMB = totalBytes / (1024.0 * 1024.0);
                
                if (speed > 0)
                {
                    statusLabel.Text = $"Đang tải: {downloadedMB:F2}MB / {totalMB:F2}MB ({percentage}%) - {speed:F2} MB/s";
                }
                else
                {
                    statusLabel.Text = $"Đang tải: {downloadedMB:F2}MB / {totalMB:F2}MB ({percentage}%)";
                }
            }));
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
