using System;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace SalaryCalculator
{
    public class UserDetailForm : Form
    {
        public UserDetailForm(UserInfo user)
        {
            this.Text = $"Chi tiết nhân viên: {user.FullName}";
            this.Width = 440;
            this.Height = 520;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.WhiteSmoke;

            var mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Padding = new Padding(18, 18, 18, 18);
            mainPanel.BackColor = System.Drawing.Color.WhiteSmoke;
            mainPanel.RowCount = 3;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Tiêu đề
            var titleLabel = new Label();
            titleLabel.Text = $"Thông tin nhân viên";
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.DarkBlue;
            titleLabel.Dock = DockStyle.Top;
            titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titleLabel.Height = 38;
            mainPanel.Controls.Add(titleLabel, 0, 0);

            // Thông tin cá nhân
            var infoGroup = new GroupBox();
            infoGroup.Text = "Thông tin cá nhân";
            infoGroup.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            infoGroup.Dock = DockStyle.Top;
            infoGroup.Height = 210;
            infoGroup.BackColor = System.Drawing.Color.White;

            var infoTable = new TableLayoutPanel();
            infoTable.Dock = DockStyle.Fill;
            infoTable.AutoSize = true;
            infoTable.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            infoTable.ColumnCount = 2;
            infoTable.RowCount = 7;
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            infoTable.RowStyles.Clear();
            for (int i = 0; i < 7; i++)
                infoTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Padding cellPadding = new Padding(0, 2, 0, 2);
            infoTable.Controls.Add(new Label { Text = "Tên đăng nhập:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 0);
            infoTable.Controls.Add(new Label { Text = user.Username, Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 0);
            infoTable.Controls.Add(new Label { Text = "Họ tên:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 1);
            infoTable.Controls.Add(new Label { Text = user.FullName, Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 1);
            infoTable.Controls.Add(new Label { Text = "Số điện thoại:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 2);
            infoTable.Controls.Add(new Label { Text = user.Phone, Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 2);
            infoTable.Controls.Add(new Label { Text = "Tuổi:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 3);
            infoTable.Controls.Add(new Label { Text = user.Age.ToString(), Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 3);
            infoTable.Controls.Add(new Label { Text = "Lương cơ bản:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 4);
            infoTable.Controls.Add(new Label { Text = user.BasicSalary.ToString("N0") + " VND", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 4);
            infoTable.Controls.Add(new Label { Text = "Tiền ăn:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 5);
            infoTable.Controls.Add(new Label { Text = user.MealAllowance.ToString("N0") + " VND", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 5);
            infoTable.Controls.Add(new Label { Text = "Tiền chuyên cần:", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Padding = cellPadding }, 0, 6);
            infoTable.Controls.Add(new Label { Text = user.AttendanceIncentive.ToString("N0") + " VND", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 10), AutoSize = true, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = cellPadding }, 1, 6);

            infoGroup.Controls.Add(infoTable);
            mainPanel.Controls.Add(infoGroup, 0, 1);

            // Lịch sử lương
            var salaryGroup = new GroupBox();
            salaryGroup.Text = "Lịch sử lương";
            salaryGroup.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);
            salaryGroup.Dock = DockStyle.Fill;
            salaryGroup.BackColor = System.Drawing.Color.White;

            var salaryTable = new TableLayoutPanel();
            salaryTable.Dock = DockStyle.Fill;
            salaryTable.ColumnCount = 2;
            salaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            salaryTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            // Đảm bảo tối thiểu 6 dòng (6 tháng)
            int salaryRows = Math.Max(6, user.SalaryHistory?.Count ?? 0);
            salaryTable.RowCount = salaryRows;
            salaryTable.RowStyles.Clear();
            for (int i = 0; i < salaryRows; i++)
                salaryTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); // Giảm chiều cao dòng

            if (user.SalaryHistory != null && user.SalaryHistory.Count > 0)
            {
                int row = 0;
                foreach (var entry in user.SalaryHistory.OrderByDescending(e => e.Key))
                {
                    if (row >= salaryRows) break;
                    salaryTable.Controls.Add(new Label { Text = entry.Key, Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 9), Padding = new Padding(0, 2, 0, 0) }, 0, row);
                    salaryTable.Controls.Add(new Label { Text = entry.Value.ToString("N0") + " VND", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.DarkGreen, Padding = new Padding(0, 2, 0, 0) }, 1, row);
                    row++;
                }
                // Nếu ít hơn 6 tháng thì thêm dòng trống
                for (; row < salaryRows; row++)
                {
                    salaryTable.Controls.Add(new Label { Text = "", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 9) }, 0, row);
                    salaryTable.Controls.Add(new Label { Text = "", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 9) }, 1, row);
                }
            }
            else
            {
                salaryTable.Controls.Add(new Label { Text = "Không có dữ liệu", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Italic), ForeColor = System.Drawing.Color.Gray, Padding = new Padding(0, 2, 0, 0) }, 0, 0);
                for (int i = 1; i < salaryRows; i++)
                {
                    salaryTable.Controls.Add(new Label { Text = "", Anchor = AnchorStyles.Right, Font = new System.Drawing.Font("Segoe UI", 9) }, 0, i);
                    salaryTable.Controls.Add(new Label { Text = "", Anchor = AnchorStyles.Left, Font = new System.Drawing.Font("Segoe UI", 9) }, 1, i);
                }
            }
            salaryGroup.Controls.Add(salaryTable);
            mainPanel.Controls.Add(salaryGroup, 0, 2);

            this.Controls.Add(mainPanel);
            try { Theme.ApplyEcommerceTheme(this); } catch { }
        }
    }
}
