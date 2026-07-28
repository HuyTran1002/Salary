using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace SalaryCalculator
{
    public class MainWebForm : Form
    {
        private WebView2 _webView;
        private WebBackendBridge _backendBridge;

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        private void TrimWorkingSetMemory()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                EmptyWorkingSet(Process.GetCurrentProcess().Handle);
            }
            catch { /* silent memory trim failure */ }
        }

        public MainWebForm()
        {
            this.Text = "Salary Calculator 3D";
            this.Width = 1180;
            this.Height = 740;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            try {
                if (System.IO.File.Exists("calculator_coin_dollar_money_icon_127186.ico"))
                    this.Icon = new System.Drawing.Icon("calculator_coin_dollar_money_icon_127186.ico");
            } catch { /* ignore if icon is missing */ }

            _backendBridge = new WebBackendBridge();

            InitializeWebView();

            this.Shown += async (s, e) => {
                await System.Threading.Tasks.Task.Delay(1500);
                CheckForAutoUpdate();
                TrimWorkingSetMemory();
            };

            // Trim memory when app is deactivated or minimized
            this.Deactivate += (s, e) => TrimWorkingSetMemory();
            this.Resize += (s, e) => {
                if (this.WindowState == FormWindowState.Minimized)
                {
                    TrimWorkingSetMemory();
                }
            };
        }

        private async void CheckForAutoUpdate()
        {
            try
            {
                var result = await UpdateChecker.CheckForUpdateAsync();
                if (result.hasUpdate && !string.IsNullOrEmpty(result.downloadUrl))
                {
                    UpdateChecker.ShowAutoUpdateDialog(result.latestVersion, result.downloadUrl);
                }
            }
            catch { /* silent update check */ }
        }

        private async void InitializeWebView()
        {
            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(_webView);

            // Ensure WebView2 is initialized with RAM optimization switches
            var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SalaryCalculatorWebView2");
            var options = new CoreWebView2EnvironmentOptions(
                "--disable-features=Translate,MediaRouter,OptimizationHints --disable-background-networking --disable-sync --no-default-browser-check"
            );
            var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
            await _webView.EnsureCoreWebView2Async(environment);

            // Disable context menu and developer tools for production feel (optional)
            // _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            // _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;

            // Inject the C# backend bridge into JS
            _webView.CoreWebView2.AddHostObjectToScript("backend", _backendBridge);

            // Navigate to the local index.html
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string htmlPath = Path.Combine(appDir, "wwwroot", "index.html");
            
            if (!File.Exists(htmlPath))
            {
                // Standalone EXE fallback: Extract embedded wwwroot files to LocalAppData
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string extractedDir = Path.Combine(localAppData, "SalaryCalculatorWeb", "wwwroot");
                EnsureFrontendFilesExtracted(extractedDir);
                htmlPath = Path.Combine(extractedDir, "index.html");
            }

            if (File.Exists(htmlPath))
            {
                _webView.CoreWebView2.Navigate(htmlPath);
            }
            else
            {
                MessageBox.Show($"Cannot find frontend files at: {htmlPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnsureFrontendFilesExtracted(string targetDir)
        {
            try
            {
                Directory.CreateDirectory(targetDir);
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();

                const string prefix = "SalaryCalculator.wwwroot.";
                foreach (var resourceName in resourceNames)
                {
                    if (resourceName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        string relativeName = resourceName.Substring(prefix.Length);
                        
                        string fileExt = Path.GetExtension(relativeName);
                        string baseName = relativeName.Substring(0, relativeName.Length - fileExt.Length);
                        string finalRelativePath = baseName.Replace('.', Path.DirectorySeparatorChar) + fileExt;

                        string fullPath = Path.Combine(targetDir, finalRelativePath);
                        string dirName = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(dirName)) Directory.CreateDirectory(dirName);

                        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                                {
                                    stream.CopyTo(fs);
                                }
                            }
                        }
                    }
                }
            }
            catch { /* fallback extraction error */ }
        }
    }
}
