using System;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace SalaryCalculator
{
    public class UserDetailForm : Form
    {
        private static void EnableDoubleBuffer(Control control)
        {
            try
            {
                var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                property?.SetValue(control, true, null);
            }
            catch { }
        }

        public UserDetailForm(UserInfo user)
        {
            this.Text = $"Chi tiết nhân viên: {user.FullName}";
            this.Width = 520;
            this.Height = 620;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
            this.UpdateStyles();
            EnableDoubleBuffer(this);

            var mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(14, 14, 14, 14);
            mainPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainPanel.RowCount = 3;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            EnableDoubleBuffer(mainPanel);

            // Tiêu đề
            var titleLabel = new Label();
            titleLabel.Text = $"Thông tin nhân viên";
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.DarkBlue;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titleLabel.Height = 34;
            mainPanel.Controls.Add(titleLabel, 0, 0);

            // Thông tin cá nhân
            var infoGroup = new GroupBox();
            infoGroup.Text = "Thông tin cá nhân";
            infoGroup.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            infoGroup.Dock = DockStyle.Top;
            infoGroup.Height = 272;
            infoGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
            EnableDoubleBuffer(infoGroup);

            var infoTable = new TableLayoutPanel();
            infoTable.Dock = DockStyle.Fill;
            infoTable.AutoSize = false;
            infoTable.ColumnCount = 2;
            infoTable.RowCount = 10;
            infoTable.Padding = new Padding(8, 6, 8, 6);
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            infoTable.RowStyles.Clear();
            for (int i = 0; i < 10; i++)
                infoTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            infoTable.BackColor = System.Drawing.Color.Transparent;
            EnableDoubleBuffer(infoTable);

            Label CreateLeftLabel(string text)
            {
                return new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    Font = new System.Drawing.Font("Segoe UI", 9.25f),
                    AutoSize = false,
                    AutoEllipsis = true,
                    Margin = new Padding(0, 2, 8, 2)
                };
            }

            Label CreateRightLabel(string text, bool bold = false)
            {
                return new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Font = bold ? new System.Drawing.Font("Segoe UI", 9.25f, System.Drawing.FontStyle.Bold) : new System.Drawing.Font("Segoe UI", 9.25f),
                    AutoSize = false,
                    AutoEllipsis = true,
                    Margin = new Padding(8, 2, 0, 2)
                };
            }

            void AddInfoRow(int row, string labelText, string valueText, bool boldValue = false)
            {
                infoTable.Controls.Add(CreateLeftLabel(labelText), 0, row);
                infoTable.Controls.Add(CreateRightLabel(valueText, boldValue), 1, row);
            }

            AddInfoRow(0, "Tên đăng nhập:", user.Username, true);
            AddInfoRow(1, "Họ tên:", user.FullName, true);
            AddInfoRow(2, "Số điện thoại:", user.Phone);
            AddInfoRow(3, "Tuổi:", user.Age.ToString());
            AddInfoRow(4, "Lương cơ bản:", user.BasicSalary.ToString("N0") + " VND");
            AddInfoRow(5, "Tiền ăn:", user.MealAllowance.ToString("N0") + " VND");
            AddInfoRow(6, "Tiền chuyên cần/ngày:", user.AttendancePerDay.ToString("N0") + " VND");
            AddInfoRow(7, "Tiền đi lại/ngày:", user.TravelAllowance.ToString("N0") + " VND");
            AddInfoRow(8, "Tiền nhà ở:", user.HousingAllowance.ToString("N0") + " VND");
            AddInfoRow(9, "Thưởng cert:", user.CertificateBonus.ToString("N0") + " VND");

            infoGroup.Controls.Add(infoTable);
            mainPanel.Controls.Add(infoGroup, 0, 1);

            // Lịch sử lương
            var salaryGroup = new GroupBox();
            salaryGroup.Text = "Lịch sử lương";
            salaryGroup.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            salaryGroup.Dock = DockStyle.Fill;
            salaryGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
            EnableDoubleBuffer(salaryGroup);

            var salaryTable = new TableLayoutPanel();
            salaryTable.AutoSize = true;
            salaryTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            salaryTable.Dock = DockStyle.Top;
            salaryTable.ColumnCount = 2;
            salaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            salaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            salaryTable.BackColor = System.Drawing.Color.Transparent;
            EnableDoubleBuffer(salaryTable);
            // Đảm bảo tối thiểu 6 dòng (6 tháng)
            int salaryRows = Math.Max(6, user.SalaryHistory?.Count ?? 0);
            salaryTable.RowCount = salaryRows;
            salaryTable.RowStyles.Clear();
            for (int i = 0; i < salaryRows; i++)
                salaryTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));

            if (user.SalaryHistory != null && user.SalaryHistory.Count > 0)
            {
                int row = 0;
                foreach (var entry in user.SalaryHistory
                                            .OrderByDescending(e =>
                                            {
                                                DateTime dt;
                                                if (DateTime.TryParseExact(e.Key, "MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                                                {
                                                    return dt;
                                                }
                                                return DateTime.MinValue;
                                            }))
                {
                    if (row >= salaryRows) break;
                    salaryTable.Controls.Add(new Label { Text = entry.Key, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Font = new System.Drawing.Font("Segoe UI", 8.75f), AutoSize = false, AutoEllipsis = true, Padding = new Padding(0, 2, 0, 0) }, 0, row);
                    salaryTable.Controls.Add(new Label { Text = entry.Value.ToString("N0") + " VND", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Font = new System.Drawing.Font("Segoe UI", 8.75f, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.DarkGreen, AutoSize = false, AutoEllipsis = true, Padding = new Padding(0, 2, 0, 0) }, 1, row);
                    row++;
                }
                // Nếu ít hơn 6 tháng thì thêm dòng trống
                for (; row < salaryRows; row++)
                {
                    salaryTable.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Font = new System.Drawing.Font("Segoe UI", 8.75f), AutoSize = false }, 0, row);
                    salaryTable.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Font = new System.Drawing.Font("Segoe UI", 8.75f), AutoSize = false }, 1, row);
                }
            }
            else
            {
                salaryTable.Controls.Add(new Label { Text = "Không có dữ liệu", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Font = new System.Drawing.Font("Segoe UI", 8.75f, System.Drawing.FontStyle.Italic), ForeColor = System.Drawing.Color.Gray, AutoSize = false, Padding = new Padding(0, 2, 0, 0) }, 0, 0);
                for (int i = 1; i < salaryRows; i++)
                {
                    salaryTable.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Font = new System.Drawing.Font("Segoe UI", 8.75f), AutoSize = false }, 0, i);
                    salaryTable.Controls.Add(new Label { Text = "", Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Font = new System.Drawing.Font("Segoe UI", 8.75f), AutoSize = false }, 1, i);
                }
            }
            // Wrap salary table in a scrollable panel to handle overflow
            var salaryScrollPanel = new Panel();
            salaryScrollPanel.Dock = DockStyle.Fill;
            salaryScrollPanel.AutoScroll = true;
            salaryScrollPanel.BackColor = System.Drawing.Color.FromArgb(246, 251, 255);
            EnableDoubleBuffer(salaryScrollPanel);
            salaryScrollPanel.Controls.Add(salaryTable);

            salaryGroup.Controls.Add(salaryScrollPanel);
            mainPanel.Controls.Add(salaryGroup, 0, 2);

            this.Controls.Add(mainPanel);
            try { Theme.ApplyInfinityGlassTheme(this); } catch { }

            // Re-assert card surfaces after global theme for readability/stability
            try
            {
                infoGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
                salaryGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
                salaryScrollPanel.BackColor = System.Drawing.Color.FromArgb(246, 251, 255);
            }
            catch { }
        }
    }
}
