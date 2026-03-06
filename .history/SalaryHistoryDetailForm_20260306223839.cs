using System;
using System.Windows.Forms;
using System.Drawing;

namespace SalaryCalculator
{
    public class SalaryHistoryDetailForm : Form
    {
        public SalaryHistoryDetailForm(UserInfo user, string periodKey, string resultDetail)
        {
            this.Text = $"Chi tiết lương tháng {periodKey} - {user.FullName}";
            this.Width = 700;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            // Header
            var titleLabel = new Label
            {
                Text = $"Chi tiết tính lương tháng {periodKey.Replace("-", "/")}",
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(106, 162, 255)
            };
            this.Controls.Add(titleLabel);

            // DataGridView
            var dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowHeadersVisible = false
            };

            // Add columns
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Item",
                HeaderText = "Mục",
                FillWeight = 50
            });
            dataGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Giá trị",
                FillWeight = 50
            });

            // Style header
            dataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(230, 240, 255);
            dataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(88, 63, 130);
            dataGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGrid.ColumnHeadersHeight = 35;

            // Style rows
            dataGrid.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dataGrid.DefaultCellStyle.Padding = new Padding(5);
            dataGrid.RowTemplate.Height = 30;

            // Parse and load data
            ParseAndLoadData(dataGrid, resultDetail);

            this.Controls.Add(dataGrid);

            // Close button
            var closeBtn = new Button
            {
                Text = "Đóng",
                Dock = DockStyle.Bottom,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(88, 63, 130),
                ForeColor = Color.White
            };
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);

            try { Theme.ApplyInfinityGlassTheme(this); } catch { }
        }

        private void ParseAndLoadData(DataGridView grid, string resultDetail)
        {
            // Remove "Chi Tiết:" header and split by lines
            var lines = resultDetail.Replace("Chi Tiết:", "").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed)) continue;

                // Remove bullet point
                trimmed = trimmed.TrimStart('•').Trim();

                // Find the colon separator
                var colonIndex = trimmed.LastIndexOf(':');
                if (colonIndex > 0)
                {
                    var item = trimmed.Substring(0, colonIndex).Trim();
                    var value = trimmed.Substring(colonIndex + 1).Trim();

                    grid.Rows.Add(item, value);
                }
            }
        }
    }
}
