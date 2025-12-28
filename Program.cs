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

            // Try to enable per-monitor DPI awareness when possible
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDpiAwareness();
                }
            }
            catch { }

            try
            {
                var login = new LoginForm();
                Theme.ApplyEcommerceTheme(login);
                Application.Run(login);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi chạy ứng dụng:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void SetProcessDpiAwareness()
        {
            try
            {
                // Windows 8.1+ Per Monitor v2 is preferred when available
                var shcore = "Shcore.dll";
                var setProc = NativeMethods.SetProcessDpiAwarenessContext;
                if (setProc != null)
                {
                    NativeMethods.SetProcessDpiAwarenessContext((IntPtr)(-4)); // DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2
                    return;
                }
            }
            catch { }
        }
    }

    internal static class NativeMethods
    {
        // Using SetProcessDpiAwarenessContext if available on the platform
        internal static Action<IntPtr> SetProcessDpiAwarenessContext = GetSetDpiAwarenessContext();

        private static Action<IntPtr> GetSetDpiAwarenessContext()
        {
            try
            {
                var user32 = System.Runtime.InteropServices.NativeLibrary.Load("user32.dll");
                var ptr = System.Runtime.InteropServices.NativeLibrary.GetExport(user32, "SetProcessDpiAwarenessContext");
                return (Action<IntPtr>)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr, typeof(Action<IntPtr>));
            }
            catch { return null; }
        }
    }
}
