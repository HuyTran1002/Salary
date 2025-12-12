using System;
using System.Windows.Forms;

namespace SalaryCalculator
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Tắt AutoUpdater lúc khởi động theo yêu cầu
            // _ = AutoUpdater.CheckForUpdatesAsync();

            try
            {
                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi chạy ứng dụng:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
