using System;
using System.Windows.Forms;

namespace SalaryCalculator
{
    public class SalaryHistoryDetailForm : Form
    {
        public SalaryHistoryDetailForm(UserInfo user, string periodKey, SalaryCalculationDetail detail)
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
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Consolas", 10),
                BackColor = System.Drawing.Color.FromArgb(246, 251, 255),
                BorderStyle = BorderStyle.FixedSingle
            };

            detailBox.Text =
                $"Chi tiết dữ liệu đã nhập cho kỳ lương {detail.Month:D2}-{detail.Year}" + Environment.NewLine + Environment.NewLine +

                "THÔNG TIN CƠ BẢN" + Environment.NewLine +
                $"- Lương cơ bản: {detail.BasicSalary:N0} VND" + Environment.NewLine +
                $"- Số ngày công: {detail.WorkingDays:N1}" + Environment.NewLine + Environment.NewLine +

                "THỜI GIAN LÀM VIỆC THÊM" + Environment.NewLine +
                $"- OT x2: {detail.Overtime2xHours:N1} giờ" + Environment.NewLine +
                $"- OT x3: {detail.Overtime3xHours:N1} giờ" + Environment.NewLine +
                $"- OT x1.5: {detail.Overtime15xHours:N1} giờ" + Environment.NewLine +
                $"- Ngày OT 8/12h: {detail.OtDays12:N1} ngày" + Environment.NewLine +
                $"- Ngày OT +4h: {detail.OtDays8:N1} ngày" + Environment.NewLine + Environment.NewLine +

                "PHỤ CẤP VÀ THƯỞNG" + Environment.NewLine +
                $"- Tiền ăn OT 8/12h: {detail.Meal12Amount:N0} VND" + Environment.NewLine +
                $"- Tiền ăn OT +4h: {detail.Meal8Amount:N0} VND" + Environment.NewLine +
                $"- Tiền đi lại/ngày: {detail.TravelPerDay:N0} VND" + Environment.NewLine +
                $"- Tiền chuyên cần/ngày: {detail.AttendancePerDay:N0} VND" + Environment.NewLine +
                $"- Thưởng chứng chỉ: {detail.CertificateBonus:N0} VND" + Environment.NewLine +
                $"- Thưởng loại A: {detail.RankingABonusAmount:N0} VND" + Environment.NewLine +
                $"- Thưởng loại B: {detail.RankingBBonusAmount:N0} VND" + Environment.NewLine +
                $"- Thưởng loại C: {detail.RankingCBonusAmount:N0} VND" + Environment.NewLine + Environment.NewLine +

                "KHÁC" + Environment.NewLine +
                $"- Xếp loại chọn: {(string.IsNullOrWhiteSpace(detail.SelectedRating) ? "(không chọn)" : detail.SelectedRating)}" + Environment.NewLine +
                $"- Recognize: {detail.RecognizeCount}" + Environment.NewLine +
                $"- Bonus khác: {detail.OtherBonus:N0} VND";

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
