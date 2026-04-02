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
            var userDataManager = new UserDataManager();
            UserInfo currentUser = user;

            this.Text = $"Chi tiết nhân viên: {user.FullName}";
            this.Width = 600;
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
            infoGroup.Height = 168;
            infoGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
            EnableDoubleBuffer(infoGroup);

            var infoTable = new TableLayoutPanel();
            infoTable.Dock = DockStyle.Fill;
            infoTable.AutoSize = false;
            infoTable.ColumnCount = 4;
            infoTable.RowCount = 5;
            infoTable.Padding = new Padding(8, 6, 8, 6);
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            infoTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            infoTable.RowStyles.Clear();
            for (int i = 0; i < 5; i++)
                infoTable.RowStyles.Add(new RowStyle(SizeType.Percent, 20));
            infoTable.BackColor = System.Drawing.Color.Transparent;
            EnableDoubleBuffer(infoTable);

            var infoToolTip = new ToolTip();
            infoToolTip.AutoPopDelay = 8000;
            infoToolTip.InitialDelay = 350;
            infoToolTip.ReshowDelay = 150;
            infoToolTip.ShowAlways = true;

            Label CreateLeftLabel(string text)
            {
                var lbl = new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleRight,
                    Font = new System.Drawing.Font("Segoe UI", 8.5f),
                    AutoSize = false,
                    AutoEllipsis = false,
                    Margin = new Padding(0, 2, 6, 2)
                };

                infoToolTip.SetToolTip(lbl, text);
                return lbl;
            }

            Label CreateRightLabel(string text, bool bold = false)
            {
                var lbl = new Label
                {
                    Text = text,
                    Dock = DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                    Font = bold ? new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Bold) : new System.Drawing.Font("Segoe UI", 8.5f),
                    AutoSize = false,
                    AutoEllipsis = false,
                    Margin = new Padding(6, 2, 0, 2)
                };

                infoToolTip.SetToolTip(lbl, text);
                return lbl;
            }

            void AddInfoPairRow(
                int row,
                string leftLabel,
                string leftValue,
                string rightLabel,
                string rightValue,
                bool leftBoldValue = false,
                bool rightBoldValue = false)
            {
                infoTable.Controls.Add(CreateLeftLabel(leftLabel), 0, row);
                infoTable.Controls.Add(CreateRightLabel(leftValue, leftBoldValue), 1, row);
                infoTable.Controls.Add(CreateLeftLabel(rightLabel), 2, row);
                infoTable.Controls.Add(CreateRightLabel(rightValue, rightBoldValue), 3, row);
            }

            decimal attendancePerMonth = user.AttendancePerDay > 0 && user.AttendancePerDay <= 20000m ? user.AttendancePerDay * 23m : user.AttendancePerDay;
            decimal travelPerMonth = user.TravelAllowance > 0 && user.TravelAllowance <= 20000m ? user.TravelAllowance * 23m : user.TravelAllowance;

            AddInfoPairRow(0, "Tên đăng nhập:", user.Username, "Họ tên:", user.FullName, true, true);
            AddInfoPairRow(1, "Số điện thoại:", user.Phone, "Tuổi:", user.Age.ToString());
            AddInfoPairRow(2, "Lương cơ bản:", user.BasicSalary.ToString("N0") + " VND", "Tiền ăn:", user.MealAllowance.ToString("N0") + " VND");
            AddInfoPairRow(3, "Tiền chuyên cần/tháng:", attendancePerMonth.ToString("N0") + " VND", "Tiền đi lại/tháng:", travelPerMonth.ToString("N0") + " VND");
            AddInfoPairRow(4, "Tiền nhà ở:", user.HousingAllowance.ToString("N0") + " VND", "Thưởng cert:", user.CertificateBonus.ToString("N0") + " VND");

            infoGroup.Controls.Add(infoTable);
            mainPanel.Controls.Add(infoGroup, 0, 1);

            // Lịch sử lương
            var salaryGroup = new GroupBox();
            salaryGroup.Text = "Lịch sử lương";
            salaryGroup.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
            salaryGroup.Dock = DockStyle.Fill;
            salaryGroup.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);
            EnableDoubleBuffer(salaryGroup);

            DateTime ParseHistoryKey(string key)
            {
                if (DateTime.TryParseExact(key, "MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                {
                    return dt;
                }
                return DateTime.MinValue;
            }

            var salaryGrid = new DataGridView();
            salaryGrid.Dock = DockStyle.Fill;
            salaryGrid.ReadOnly = true;
            salaryGrid.AllowUserToAddRows = false;
            salaryGrid.AllowUserToDeleteRows = false;
            salaryGrid.AllowUserToResizeRows = false;
            salaryGrid.AllowUserToResizeColumns = false;
            salaryGrid.RowHeadersVisible = false;
            salaryGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            salaryGrid.MultiSelect = false;
            salaryGrid.BackgroundColor = System.Drawing.Color.FromArgb(246, 251, 255);
            salaryGrid.BorderStyle = BorderStyle.None;
            salaryGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            salaryGrid.EnableHeadersVisualStyles = false;
            salaryGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(106, 162, 255);
            salaryGrid.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            salaryGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            salaryGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold);
            salaryGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            salaryGrid.DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 9);
            salaryGrid.DefaultCellStyle.ForeColor = System.Drawing.Color.FromArgb(40, 62, 104);
            salaryGrid.RowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(248, 252, 255);
            salaryGrid.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(237, 246, 255);
            salaryGrid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.FromArgb(162, 196, 255);
            salaryGrid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.FromArgb(24, 47, 89);
            salaryGrid.RowTemplate.Height = 30;

            salaryGrid.Columns.Add("Period", "Tháng");
            salaryGrid.Columns.Add("Net", "Lương net");
            salaryGrid.Columns.Add("Hint", "Chi tiết");
            salaryGrid.Columns[0].FillWeight = 20;
            salaryGrid.Columns[1].FillWeight = 40;
            salaryGrid.Columns[2].FillWeight = 40;
            salaryGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            void ReloadSalaryGrid()
            {
                salaryGrid.Rows.Clear();

                if (currentUser.SalaryHistory != null && currentUser.SalaryHistory.Count > 0)
                {
                    foreach (var entry in currentUser.SalaryHistory.OrderByDescending(e => ParseHistoryKey(e.Key)))
                    {
                        bool hasDetail = currentUser.SalaryResultHistory != null && currentUser.SalaryResultHistory.ContainsKey(entry.Key);
                        salaryGrid.Rows.Add(
                            entry.Key,
                            entry.Value.ToString("N0") + " VND",
                            hasDetail ? "Bấm để xem chi tiết lương tháng này" : "Dữ liệu cũ (chưa có chi tiết lương)");
                    }
                }
                else
                {
                    salaryGrid.Rows.Add("", "Không có dữ liệu", "");
                }
            }

            ReloadSalaryGrid();

            salaryGrid.CellClick += (s, e) =>
            {
                if (e.RowIndex < 0 || salaryGrid.Rows.Count == 0)
                {
                    return;
                }

                string periodKey = salaryGrid.Rows[e.RowIndex].Cells[0].Value?.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(periodKey))
                {
                    return;
                }

                if (currentUser.SalaryResultHistory != null && currentUser.SalaryResultHistory.TryGetValue(periodKey, out string resultDetail))
                {
                    var detailForm = new SalaryHistoryDetailForm(currentUser, periodKey, resultDetail);
                    detailForm.ShowDialog(this);
                }
                else
                {
                    MessageBox.Show(
                        "Dòng lịch sử này là dữ liệu cũ nên chưa có chi tiết lương. Hãy tính lương lại tháng đó để lưu đầy đủ chi tiết.",
                        "Chưa có chi tiết lương",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            };

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
            salaryScrollPanel.Controls.Add(salaryGrid);

            var clearOldYearButton = new Button();
            clearOldYearButton.Text = "Xóa lịch sử năm cũ";
            clearOldYearButton.Dock = DockStyle.Fill;
            clearOldYearButton.Height = 32;
            clearOldYearButton.Font = new System.Drawing.Font("Segoe UI", 8.75f, System.Drawing.FontStyle.Bold);
            clearOldYearButton.BackColor = System.Drawing.Color.FromArgb(255, 90, 0);
            clearOldYearButton.ForeColor = System.Drawing.Color.White;
            clearOldYearButton.FlatStyle = FlatStyle.Flat;
            clearOldYearButton.FlatAppearance.BorderSize = 0;

            clearOldYearButton.Click += (s, e) =>
            {
                int currentYear = DateTime.Now.Year;
                var confirm = MessageBox.Show(
                    $"Xóa toàn bộ lịch sử lương của các năm trước {currentYear}?",
                    "Xóa lịch sử năm cũ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes)
                {
                    return;
                }

                int removedCount = userDataManager.DeleteOldSalaryHistoryByYear(currentUser.Username, currentYear);
                currentUser = userDataManager.Login(currentUser.Username) ?? currentUser;
                ReloadSalaryGrid();

                MessageBox.Show(
                    removedCount > 0
                        ? $"Đã xóa {removedCount} bản ghi lịch sử năm cũ."
                        : "Không có dữ liệu năm cũ để xóa.",
                    "Hoàn tất",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            var salaryContent = new TableLayoutPanel();
            salaryContent.Dock = DockStyle.Fill;
            salaryContent.ColumnCount = 1;
            salaryContent.RowCount = 2;
            salaryContent.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            salaryContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            salaryContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            salaryContent.Controls.Add(clearOldYearButton, 0, 0);
            salaryContent.Controls.Add(salaryScrollPanel, 0, 1);

            salaryGroup.Controls.Add(salaryContent);
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
