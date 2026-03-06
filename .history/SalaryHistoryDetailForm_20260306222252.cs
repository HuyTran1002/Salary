using System;
using System.Windows.Forms;

namespace SalaryCalculator
{
    public class SalaryHistoryDetailForm : Form
    {
        public SalaryHistoryDetailForm(UserInfo user, string periodKey, string resultDetail)
        {
            this.Text = $"Chi tiết input lương tháng {periodKey} - {user.FullName}";
            this.Width = 720;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.WhiteSmoke;

            var titleLabel = new Label
            {
                Text = $"Chi tiết dữ liệu đã nhập ({periodKey})",
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.DarkBlue
            };
            this.Controls.Add(titleLabel);

            var detailBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                BackColor = System.Drawing.Color.FromArgb(246, 251, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            detailBox.Text = resultDetail;

            this.Controls.Add(detailBox);

            var closeBtn = new Button
            {
                Text = "Đóng",
                Dock = DockStyle.Bottom,
                Height = 36,
                Font = new System.Drawing.Font("Segoe UI", 9, System.Drawing.FontStyle.Bold),
                BackColor = System.Drawing.Color.FromArgb(88, 63, 130),
                ForeColor = System.Drawing.Color.White
            };
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);

            try { Theme.ApplyInfinityGlassTheme(this); } catch { }
        }
    }
}
