using System;
using System.Windows.Forms;

namespace SalaryCalculator
{
    public class SalaryHistoryDetailForm : Form
    {
        public SalaryHistoryDetailForm(UserInfo user, string periodKey, string resultDetail)
        {
            this.Text = $"Chi tiết lương tháng {periodKey} - {user.FullName}";
            this.Width = 1000;
            this.Height = 900;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = System.Drawing.Color.WhiteSmoke;

            var titleLabel = new Label
            {
                Text = $"Chi tiết tính lương tháng {periodKey.Replace("-", "/")}",
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.White,
                BackColor = System.Drawing.Color.FromArgb(106, 162, 255)
            };
            this.Controls.Add(titleLabel);

            var detailBox = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = true,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 10),
                BackColor = System.Drawing.Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                TabStop = false
            };

            detailBox.Text = resultDetail;

            this.Controls.Add(detailBox);

            var closeBtn = new Button
            {
                Text = "Đóng",
                Dock = DockStyle.Bottom,
                Height = 32,
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
