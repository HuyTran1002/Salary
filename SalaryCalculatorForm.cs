using System;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SalaryCalculator
{
    public partial class SalaryCalculatorForm : Form
    {
        private string currentUsername;
        private UserDataManager userDataManager = new UserDataManager();

        public SalaryCalculatorForm(string username = "")
        {
            currentUsername = username;
            InitializeComponent();
            // Để LoginForm kiểm soát quay lại khi form này đóng
        }

        private void InitializeComponent()
        {
            if (currentUsername == "admin")
            {
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                this.Text = $"BẢNG XẾP HẠNG LƯƠNG THÁNG {month:D2}/{year}";
                this.Width = 900;
                this.Height = 600;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Font = new System.Drawing.Font("Arial", 9);
                this.AutoScroll = false;

                // Title Label
                Label titleLabel = new Label();
                titleLabel.Text = $"🏆 BẢNG XẾP HẠNG LƯƠNG THÁNG {month:D2}/{year} 🏆";
                titleLabel.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                titleLabel.ForeColor = System.Drawing.Color.DarkBlue;
                titleLabel.Dock = DockStyle.Top;
                titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                titleLabel.Height = 50;
                titleLabel.Padding = new Padding(0, 10, 0, 0);
                this.Controls.Add(titleLabel);

                // Tạo DataGridView bảng xếp hạng
                DataGridView rankingGrid = new DataGridView();
                rankingGrid.Name = "rankingGrid";
                rankingGrid.Location = new System.Drawing.Point(40, 70);
                rankingGrid.Width = 800;
                // Tính chiều cao tối thiểu cho 20 dòng (mỗi dòng ~22px + header)
                int minRows = 20;
                int rowHeight = 22;
                int headerHeight = 36;
                rankingGrid.Height = headerHeight + minRows * rowHeight;
                rankingGrid.ReadOnly = true;
                rankingGrid.AllowUserToAddRows = false;
                rankingGrid.AllowUserToDeleteRows = false;
                rankingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                rankingGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
                rankingGrid.RowHeadersVisible = false;
                rankingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                rankingGrid.MultiSelect = false;
                rankingGrid.Font = new System.Drawing.Font("Segoe UI", 10);
                rankingGrid.EnableHeadersVisualStyles = false;
                rankingGrid.ColumnCount = 4;
                rankingGrid.Columns[0].Name = "Hạng";
                rankingGrid.Columns[1].Name = "Tên nhân viên";
                rankingGrid.Columns[2].Name = "Lương Net (VND)";
                rankingGrid.Columns[3].Name = "Vinh danh";
                rankingGrid.Columns[3].Width = 220;
                rankingGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                rankingGrid.Columns[0].Width = 70;
                rankingGrid.Columns[2].DefaultCellStyle.ForeColor = System.Drawing.Color.DarkGreen;
                rankingGrid.Columns[2].DefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold);
                rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightSkyBlue;
                rankingGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold);

                // 50 câu khen/cảm thán về mức lương (top 10)
                string[] compliments = new string[] {
                    "Quá xuất sắc!", "Đỉnh của chóp!", "Lương mơ ước!", "Tuyệt vời ông mặt trời!", "Đáng ngưỡng mộ!", "Làm việc như siêu nhân!", "Thu nhập cực khủng!", "Cố gắng phát huy!", "Làm việc hết mình!", "Chuyên gia tiết kiệm!",
                    "Lương cao ngất ngưởng!", "Đồng nghiệp ngưỡng mộ!", "Sếp cũng phải nể!", "Làm việc chăm chỉ!", "Tấm gương sáng!", "Công thần của công ty!", "Bậc thầy tài chính!", "Làm việc hiệu quả!", "Thành tích tuyệt vời!", "Lương tăng vèo vèo!",
                    "Được thưởng nóng!", "Làm việc không biết mệt!", "Cỗ máy kiếm tiền!", "Người truyền cảm hứng!", "Làm việc siêu tốc!", "Đỉnh cao nghề nghiệp!", "Lương vượt chỉ tiêu!", "Chuyên gia tăng ca!", "Làm việc chuẩn chỉnh!", "Được lòng sếp lớn!",
                    "Làm việc như robot!", "Không ai sánh bằng!", "Lương tháng này quá đã!", "Được vinh danh toàn công ty!", "Làm việc xuất thần!", "Công nhận tài năng!", "Làm việc không ngừng nghỉ!", "Lương như mơ!", "Được đồng nghiệp yêu quý!", "Làm việc cực kỳ hiệu quả!",
                    "Làm việc siêu năng suất!", "Lương tăng đều đều!", "Được thưởng lớn!", "Làm việc tận tâm!", "Làm việc sáng tạo!", "Làm việc chuyên nghiệp!", "Làm việc gương mẫu!", "Làm việc xuất sắc!", "Làm việc nhiệt huyết!", "Làm việc tận tụy!"
                };
                // 20 câu động viên/chê cho hạng ngoài top 10
                string[] encouragements = new string[] {
                    "Cố gắng hơn nữa nhé!", "Đừng nản lòng!", "Sắp vào top rồi!", "Nỗ lực sẽ được đền đáp!", "Chỉ cần cố thêm chút nữa!", "Đừng bỏ cuộc!", "Cơ hội vẫn còn phía trước!", "Hãy kiên trì!", "Cần bứt phá mạnh mẽ hơn!", "Đừng để lương tháng sau thấp hơn nhé!",
                    "Cần chăm chỉ hơn!", "Hãy hỏi bí quyết từ top trên!", "Đừng để bị bỏ lại phía sau!", "Cố lên, bạn làm được!", "Hãy xem lại mục tiêu!", "Đừng để sếp nhắc nhở!", "Cần cải thiện hiệu suất!", "Đừng để đồng nghiệp vượt mặt!", "Hãy tự tin hơn!", "Lương thấp không phải mãi mãi!"
                };
                var rand = new Random();
                // Tối ưu random không lặp lại cho đến khi hết danh sách
                List<string> complimentPool = compliments.ToList();
                List<string> encouragementPool = encouragements.ToList();
                int complimentIndex = 0, encouragementIndex = 0;
                complimentPool = complimentPool.OrderBy(x => rand.Next()).ToList();
                encouragementPool = encouragementPool.OrderBy(x => rand.Next()).ToList();

                string GetNextCompliment()
                {
                    if (complimentIndex >= complimentPool.Count)
                    {
                        complimentPool = compliments.OrderBy(x => rand.Next()).ToList();
                        complimentIndex = 0;
                    }
                    return complimentPool[complimentIndex++];
                }
                string GetNextEncouragement()
                {
                    if (encouragementIndex >= encouragementPool.Count)
                    {
                        encouragementPool = encouragements.OrderBy(x => rand.Next()).ToList();
                        encouragementIndex = 0;
                    }
                    return encouragementPool[encouragementIndex++];
                }

                // Lấy dữ liệu xếp hạng từ UserDataManager, chỉ lấy lương tháng hiện tại
                var users = userDataManager.GetAllUsers();
                var sorted = users.OrderByDescending(u => u.LastNetSalary).ToList();
                int rank = 1;
                foreach (var u in sorted)
                {
                    string rankDisplay = rank.ToString();
                    if (rank == 1) rankDisplay = "1 👑";
                    else if (rank == 2) rankDisplay = "2 🥈";
                    else if (rank == 3) rankDisplay = "3 🏅";
                    // Chỉ khen nếu có lương tháng hiện tại, còn lại động viên/chê
                    string message;
                    if (u.LastCalculatedMonth == month && u.LastCalculatedYear == year && u.LastNetSalary > 0)
                    {
                        message = rank <= 10 ? GetNextCompliment() : GetNextEncouragement();
                    }
                    else
                    {
                        message = GetNextEncouragement();
                    }
                    rankingGrid.Rows.Add(rankDisplay, u.FullName, u.LastNetSalary.ToString("N0"), message);
                    rank++;
                }
                // Thêm dòng trống nếu ít hơn 20 hạng
                for (int i = sorted.Count + 1; i <= minRows; i++)
                {
                    rankingGrid.Rows.Add(i.ToString(), "", "", "");
                }
                rankingGrid.RowsDefaultCellStyle.BackColor = System.Drawing.Color.White;
                rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.AliceBlue;
                rankingGrid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
                rankingGrid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
                // Thêm sự kiện click vào tên nhân viên để hiện chi tiết
                rankingGrid.CellClick += (s, e) =>
                {
                    // Chỉ xử lý khi click vào cột tên nhân viên (cột 1)
                    if (e.RowIndex >= 0 && e.ColumnIndex == 1)
                    {
                        string fullName = rankingGrid.Rows[e.RowIndex].Cells[1].Value?.ToString();
                        if (!string.IsNullOrWhiteSpace(fullName))
                        {
                            // Tìm user theo tên đầy đủ (FullName)
                            var user = users.FirstOrDefault(u => u.FullName == fullName);
                            if (user != null)
                            {
                                var detailForm = new UserDetailForm(user);
                                detailForm.ShowDialog(this);
                            }
                        }
                    }
                };
                this.Controls.Add(rankingGrid);
                return;
            }

            // Giao diện tính lương cho user thường
            this.Text = "Tính Lương - Salary Calculator";
            this.Width = 900;
            this.Height = 740;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Arial", 9);
            this.AutoScroll = false;

            // Title Label
            Label titleLabelUser = new Label();
            titleLabelUser.Text = "TÍNH LƯƠNG NHÂN VIÊN";
            titleLabelUser.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            titleLabelUser.ForeColor = System.Drawing.Color.DarkBlue;
            titleLabelUser.Dock = DockStyle.Top;
            titleLabelUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titleLabelUser.Height = 32;
            titleLabelUser.Padding = new Padding(0, 5, 0, 0);
            this.Controls.Add(titleLabelUser);

            // Main Panel with Auto Scroll
            Panel mainPanel = new Panel();
            mainPanel.Location = new System.Drawing.Point((this.Width - 885) / 2 - 8, 32);
            mainPanel.Width = 885;
            mainPanel.Height = 680;
            mainPanel.AutoScroll = true;

            // Left/Right Column Panels (balanced and centered)
            Panel leftPanel = new Panel();
            Panel rightPanel = new Panel();
            leftPanel.Width = 420;
            rightPanel.Width = 420;
            leftPanel.Height = 360;
            rightPanel.Height = 290;
            int columnsTotalWidth = leftPanel.Width + rightPanel.Width + 25;
            int columnsStartX = (mainPanel.Width - columnsTotalWidth) / 2;
            leftPanel.Location = new System.Drawing.Point(columnsStartX, 5);
            rightPanel.Location = new System.Drawing.Point(columnsStartX + leftPanel.Width + 25, 5);

            int leftY = 10;
            int rightY = 10;
            int rowGap = 24;
            int sectionGap = 20;

            // LEFT COLUMN - Employee Info & Basic Salary
            Label nameLabel = new Label();
            nameLabel.Text = "Tên Nhân Viên:";
            nameLabel.Location = new System.Drawing.Point(10, leftY);
            nameLabel.Width = 120;
            nameLabel.Height = 20;

            TextBox nameTextBox = new TextBox();
            nameTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            nameTextBox.Width = 230;
            nameTextBox.Height = 20;
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Font = new System.Drawing.Font("Arial", 9);
            nameTextBox.TextAlign = HorizontalAlignment.Left;
            nameTextBox.BorderStyle = BorderStyle.Fixed3D;
            nameTextBox.ReadOnly = true;
            nameTextBox.BackColor = System.Drawing.Color.LightGray;

            int nameY = leftY; // Save for edit button positioning

            leftY += rowGap;

            // Phone/Zalo
            Label phoneLabel = new Label();
            phoneLabel.Text = "SĐT/Zalo:";
            phoneLabel.Location = new System.Drawing.Point(10, leftY);
            phoneLabel.Width = 120;
            phoneLabel.Height = 20;

            TextBox phoneTextBox = new TextBox();
            phoneTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            phoneTextBox.Width = 230;
            phoneTextBox.Height = 20;
            phoneTextBox.Name = "phoneTextBox";
            phoneTextBox.Font = new System.Drawing.Font("Arial", 9);
            phoneTextBox.TextAlign = HorizontalAlignment.Left;
            phoneTextBox.BorderStyle = BorderStyle.Fixed3D;
            phoneTextBox.ReadOnly = true;
            phoneTextBox.BackColor = System.Drawing.Color.LightGray;

            leftY += rowGap;

            // Age
            Label ageLabel = new Label();
            ageLabel.Text = "Tuổi:";
            ageLabel.Location = new System.Drawing.Point(10, leftY);
            ageLabel.Width = 120;
            ageLabel.Height = 20;

            TextBox ageTextBox = new TextBox();
            ageTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            ageTextBox.Width = 230;
            ageTextBox.Height = 20;
            ageTextBox.Name = "ageTextBox";
            ageTextBox.Font = new System.Drawing.Font("Arial", 9);
            ageTextBox.TextAlign = HorizontalAlignment.Left;
            ageTextBox.BorderStyle = BorderStyle.Fixed3D;
            ageTextBox.ReadOnly = true;
            ageTextBox.BackColor = System.Drawing.Color.LightGray;

            leftY += rowGap;

            Label monthLabel = new Label();
            monthLabel.Text = "Tháng:";
            monthLabel.Location = new System.Drawing.Point(10, leftY);
            monthLabel.Width = 50;
            monthLabel.Height = 18;

            TextBox monthTextBox = new TextBox();
            monthTextBox.Location = new System.Drawing.Point(65, leftY);
            monthTextBox.Width = 35;
            monthTextBox.Height = 20;
            monthTextBox.Name = "monthTextBox";
            monthTextBox.Font = new System.Drawing.Font("Arial", 8);
            monthTextBox.Text = DateTime.Now.Month.ToString();

            Label yearLabel = new Label();
            yearLabel.Text = "Năm:";
            yearLabel.Location = new System.Drawing.Point(125, leftY);
            yearLabel.Width = 35;
            yearLabel.Height = 18;

            TextBox yearTextBox = new TextBox();
            yearTextBox.Location = new System.Drawing.Point(165, leftY);
            yearTextBox.Width = 40;
            yearTextBox.Height = 20;
            yearTextBox.Name = "yearTextBox";
            yearTextBox.Font = new System.Drawing.Font("Arial", 8);
            yearTextBox.Text = DateTime.Now.Year.ToString();

            leftY += rowGap;

            Label salaryLabel = new Label();
            salaryLabel.Text = "Lương Cơ Bản:";
            salaryLabel.Location = new System.Drawing.Point(10, leftY);
            salaryLabel.Width = 110;
            salaryLabel.Height = 18;

            TextBox salaryTextBox = new TextBox();
            salaryTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            salaryTextBox.Width = 275;
            salaryTextBox.Height = 20;
            salaryTextBox.Tag = "salary";
            salaryTextBox.Name = "salaryTextBox";
            salaryTextBox.Font = new System.Drawing.Font("Arial", 8);
            salaryTextBox.ReadOnly = true;
            salaryTextBox.BackColor = System.Drawing.Color.LightGray;
            NumberFormatter.FormatNumberInput(salaryTextBox);

            leftY += rowGap;

            Label mealLabel = new Label();
            mealLabel.Text = "Tiền Ăn/Tháng:";
            mealLabel.Location = new System.Drawing.Point(10, leftY);
            mealLabel.Width = 110;
            mealLabel.Height = 18;

            TextBox mealTextBox = new TextBox();
            mealTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            mealTextBox.Width = 275;
            mealTextBox.Height = 20;
            mealTextBox.Name = "mealTextBox";
            mealTextBox.Font = new System.Drawing.Font("Arial", 8);
            mealTextBox.Text = "0";
            mealTextBox.ReadOnly = true;
            mealTextBox.BackColor = System.Drawing.Color.LightGray;
            NumberFormatter.FormatNumberInput(mealTextBox);

            // Edit Button
            Button editInfoBtn = new Button();
            editInfoBtn.Text = "✏️";
            editInfoBtn.Location = new System.Drawing.Point(365, nameY);
            editInfoBtn.Width = 40;
            editInfoBtn.Height = 22;
            editInfoBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            editInfoBtn.BackColor = System.Drawing.Color.Orange;
            editInfoBtn.ForeColor = System.Drawing.Color.White;
            editInfoBtn.Click += (s, e) => OpenEditForm(nameTextBox, salaryTextBox, mealTextBox);

            leftY += 28;

            Label workingDaysLabel = new Label();
            workingDaysLabel.Text = "Số Ngày Công:";
            workingDaysLabel.Location = new System.Drawing.Point(10, leftY);
            workingDaysLabel.Width = 110;
            workingDaysLabel.Height = 18;

            TextBox workingDaysTextBox = new TextBox();
            workingDaysTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            workingDaysTextBox.Width = 275;
            workingDaysTextBox.Height = 20;
            workingDaysTextBox.Name = "workingDaysTextBox";
            workingDaysTextBox.ReadOnly = true;
            workingDaysTextBox.BackColor = System.Drawing.Color.LightGray;
            workingDaysTextBox.Font = new System.Drawing.Font("Arial", 8);
            workingDaysTextBox.Text = "0";

            // Auto-calculate working days when month/year changes
            monthTextBox.Leave += (s, e) => CalculateWorkingDays(monthTextBox, yearTextBox, workingDaysTextBox);
            yearTextBox.Leave += (s, e) => CalculateWorkingDays(monthTextBox, yearTextBox, workingDaysTextBox);

            leftY += 28;

            Label daysOffLabel = new Label();
            daysOffLabel.Text = "Số Ngày Nghỉ:";
            daysOffLabel.Location = new System.Drawing.Point(10, leftY);
            daysOffLabel.Width = 110;
            daysOffLabel.Height = 18;

            TextBox daysOffTextBox = new TextBox();
            daysOffTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            daysOffTextBox.Width = 275;
            daysOffTextBox.Height = 20;
            daysOffTextBox.Name = "daysOffTextBox";
            daysOffTextBox.Font = new System.Drawing.Font("Arial", 8);
            daysOffTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(daysOffTextBox);
            daysOffTextBox.Leave += (s, e) => UpdateDailyRate(salaryTextBox, mealTextBox, workingDaysTextBox, daysOffTextBox);

            leftY += 28;

            // Divider
            Label divider1 = new Label();
            divider1.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
            divider1.Location = new System.Drawing.Point(10, leftY);
            divider1.Width = 400;
            divider1.Height = 18;
            divider1.ForeColor = System.Drawing.Color.Gray;

            leftY += 22;

            Label insuranceLabel = new Label();
            insuranceLabel.Text = "Bảo Hiểm (%):";
            insuranceLabel.Location = new System.Drawing.Point(10, leftY);
            insuranceLabel.Width = 110;
            insuranceLabel.Height = 18;

            TextBox insuranceTextBox = new TextBox();
            insuranceTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            insuranceTextBox.Width = 275;
            insuranceTextBox.Height = 20;
            insuranceTextBox.Name = "insuranceTextBox";
            insuranceTextBox.Font = new System.Drawing.Font("Arial", 8);
            insuranceTextBox.Text = "10.5";
            NumberFormatter.FormatNumberInput(insuranceTextBox);

            leftY += 28;

            Label taxLabel = new Label();
            taxLabel.Text = "Thuế (%)";
            taxLabel.Location = new System.Drawing.Point(10, leftY);
            taxLabel.Width = 110;
            taxLabel.Height = 18;

            TextBox taxTextBox = new TextBox();
            taxTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            taxTextBox.Width = 275;
            taxTextBox.Height = 20;
            taxTextBox.Name = "taxTextBox";
            taxTextBox.Font = new System.Drawing.Font("Arial", 8);
            taxTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(taxTextBox);

            // RIGHT COLUMN - Overtime, Meal, Incentive - NEW STRUCTURE (3 SECTIONS)
            // SECTION 1: TIỀN TĂNG CA (Overtime Money)
            Label otTitle = new Label();
            otTitle.Text = "TIỀN TĂNG CA";
            otTitle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            otTitle.Location = new System.Drawing.Point(10, rightY);
            otTitle.Width = 400;
            otTitle.Height = 20;
            otTitle.ForeColor = System.Drawing.Color.DarkGreen;

            rightY += sectionGap;

            Label overtime2xLabel = new Label();
            overtime2xLabel.Text = "Số Giờ (x2):";
            overtime2xLabel.Location = new System.Drawing.Point(10, rightY);
            overtime2xLabel.Width = 90;
            overtime2xLabel.Height = 18;

            TextBox overtime2xTextBox = new TextBox();
            overtime2xTextBox.Location = new System.Drawing.Point(105, rightY);
            overtime2xTextBox.Width = 80;
            overtime2xTextBox.Height = 22;
            overtime2xTextBox.Name = "overtime2xTextBox";
            overtime2xTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(overtime2xTextBox);

            Label overtime2xResultLabel = new Label();
            overtime2xResultLabel.Text = "→ 0 VND";
            overtime2xResultLabel.Location = new System.Drawing.Point(190, rightY);
            overtime2xResultLabel.Width = 210;
            overtime2xResultLabel.Height = 18;
            overtime2xResultLabel.Name = "overtime2xResultLabel";
            overtime2xResultLabel.ForeColor = System.Drawing.Color.DarkOrange;
            overtime2xResultLabel.Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);

            rightY += rowGap;

            Label overtime15xLabel = new Label();
            overtime15xLabel.Text = "Số Giờ (x1.5):";
            overtime15xLabel.Location = new System.Drawing.Point(10, rightY);
            overtime15xLabel.Width = 90;
            overtime15xLabel.Height = 18;

            TextBox overtime15xTextBox = new TextBox();
            overtime15xTextBox.Location = new System.Drawing.Point(105, rightY);
            overtime15xTextBox.Width = 80;
            overtime15xTextBox.Height = 22;
            overtime15xTextBox.Name = "overtime15xTextBox";
            overtime15xTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(overtime15xTextBox);

            Label overtime15xResultLabel = new Label();
            overtime15xResultLabel.Text = "→ 0 VND";
            overtime15xResultLabel.Location = new System.Drawing.Point(190, rightY);
            overtime15xResultLabel.Width = 210;
            overtime15xResultLabel.Height = 18;
            overtime15xResultLabel.Name = "overtime15xResultLabel";
            overtime15xResultLabel.ForeColor = System.Drawing.Color.DarkOrange;
            overtime15xResultLabel.Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);

            rightY += rowGap;

            // Divider
            Label divider2 = new Label();
            divider2.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
            divider2.Location = new System.Drawing.Point(10, rightY);
            divider2.Width = 400;
            divider2.Height = 18;
            divider2.ForeColor = System.Drawing.Color.Gray;

            rightY += sectionGap;

            // SECTION 2: TIỀN ĂN TĂNG CA (OT Meal Money)
            Label mealOTTitle = new Label();
            mealOTTitle.Text = "TIỀN ĂN TĂNG CA";
            mealOTTitle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            mealOTTitle.Location = new System.Drawing.Point(10, rightY);
            mealOTTitle.Width = 400;
            mealOTTitle.Height = 20;
            mealOTTitle.ForeColor = System.Drawing.Color.DarkBlue;

            rightY += 25;

            Label otDays12Label = new Label();
            otDays12Label.Text = "Số ngày OT 8/12h:";
            otDays12Label.Location = new System.Drawing.Point(10, rightY);
            otDays12Label.Width = 115;
            otDays12Label.Height = 18;

            TextBox otDays12TextBox = new TextBox();
            otDays12TextBox.Location = new System.Drawing.Point(130, rightY);
            otDays12TextBox.Width = 55;
            otDays12TextBox.Height = 22;
            otDays12TextBox.Name = "otDays12TextBox";
            otDays12TextBox.Text = "0";
            NumberFormatter.FormatNumberInput(otDays12TextBox);

            Label meal12DisplayLabel = new Label();
            meal12DisplayLabel.Text = "× 30k";
            meal12DisplayLabel.Location = new System.Drawing.Point(190, rightY);
            meal12DisplayLabel.Width = 60;
            meal12DisplayLabel.Height = 18;
            meal12DisplayLabel.ForeColor = System.Drawing.Color.DarkGreen;
            meal12DisplayLabel.Name = "meal12DisplayLabel";

            Button editMeal12Btn = new Button();
            editMeal12Btn.Text = "✏️";
            editMeal12Btn.Location = new System.Drawing.Point(255, rightY - 2);
            editMeal12Btn.Width = 28;
            editMeal12Btn.Height = 22;
            editMeal12Btn.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            editMeal12Btn.BackColor = System.Drawing.Color.LightBlue;
            editMeal12Btn.Name = "editMeal12Btn";
            editMeal12Btn.Tag = "30000"; // Store default amount

            rightY += rowGap;

            Label otDays8Label = new Label();
            otDays8Label.Text = "Số ngày OT +4h:";
            otDays8Label.Location = new System.Drawing.Point(10, rightY);
            otDays8Label.Width = 115;
            otDays8Label.Height = 18;

            TextBox otDays8TextBox = new TextBox();
            otDays8TextBox.Location = new System.Drawing.Point(130, rightY);
            otDays8TextBox.Width = 55;
            otDays8TextBox.Height = 22;
            otDays8TextBox.Name = "otDays8TextBox";
            otDays8TextBox.Text = "0";
            NumberFormatter.FormatNumberInput(otDays8TextBox);

            Label meal8DisplayLabel = new Label();
            meal8DisplayLabel.Text = "× 20k";
            meal8DisplayLabel.Location = new System.Drawing.Point(190, rightY);
            meal8DisplayLabel.Width = 60;
            meal8DisplayLabel.Height = 18;
            meal8DisplayLabel.ForeColor = System.Drawing.Color.DarkGreen;
            meal8DisplayLabel.Name = "meal8DisplayLabel";

            Button editMeal8Btn = new Button();
            editMeal8Btn.Text = "✏️";
            editMeal8Btn.Location = new System.Drawing.Point(255, rightY - 2);
            editMeal8Btn.Width = 28;
            editMeal8Btn.Height = 22;
            editMeal8Btn.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            editMeal8Btn.BackColor = System.Drawing.Color.LightBlue;
            editMeal8Btn.Name = "editMeal8Btn";
            editMeal8Btn.Tag = "20000"; // Store default amount

            rightY += rowGap;

            // Divider
            Label divider3 = new Label();
            divider3.Text = "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";
            divider3.Location = new System.Drawing.Point(10, rightY);
            divider3.Width = 400;
            divider3.Height = 18;
            divider3.ForeColor = System.Drawing.Color.Gray;

            rightY += sectionGap;

            // SECTION 3: TIỀN INCENTIVE (Incentive Money)
            Label incentiveTitle = new Label();
            incentiveTitle.Text = "TIỀN INCENTIVE";
            incentiveTitle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            incentiveTitle.Location = new System.Drawing.Point(10, rightY);
            incentiveTitle.Width = 400;
            incentiveTitle.Height = 20;
            incentiveTitle.ForeColor = System.Drawing.Color.DarkOrange;

            rightY += 25;

            Label attendanceLabel = new Label();
            attendanceLabel.Text = "Tiền Chuyên Cần:";
            attendanceLabel.Location = new System.Drawing.Point(10, rightY);
            attendanceLabel.Width = 110;
            attendanceLabel.Height = 18;

            TextBox attendanceTextBox = new TextBox();
            attendanceTextBox.Location = new System.Drawing.Point(125, rightY);
            attendanceTextBox.Width = 220;
            attendanceTextBox.Height = 20;
            attendanceTextBox.Name = "attendanceTextBox";
            attendanceTextBox.Font = new System.Drawing.Font("Arial", 8);
            attendanceTextBox.ReadOnly = true;
            attendanceTextBox.BackColor = System.Drawing.Color.LightGray;
            attendanceTextBox.Text = "0";

            rightY += 28;

            Label otherBonusLabel = new Label();
            otherBonusLabel.Text = "Tiền Bonus Khác:";
            otherBonusLabel.Location = new System.Drawing.Point(10, rightY);
            otherBonusLabel.Width = 110;
            otherBonusLabel.Height = 18;

            TextBox otherBonusTextBox = new TextBox();
            otherBonusTextBox.Location = new System.Drawing.Point(125, rightY);
            otherBonusTextBox.Width = 220;
            otherBonusTextBox.Height = 20;
            otherBonusTextBox.Name = "otherBonusTextBox";
            otherBonusTextBox.Font = new System.Drawing.Font("Arial", 8);
            otherBonusTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(otherBonusTextBox);
            otherBonusTextBox.ReadOnly = false;
            otherBonusTextBox.BackColor = System.Drawing.Color.White;

            // Placeholder TextBox for recognize (hidden but kept for compatibility)
            TextBox recognizeTextBox = new TextBox();
            recognizeTextBox.Name = "recognizeTextBox";
            recognizeTextBox.Text = "0";
            recognizeTextBox.Visible = false;

            // Add all controls to left panel
            leftPanel.Controls.AddRange(new Control[] {
                nameLabel, nameTextBox, editInfoBtn,
                phoneLabel, phoneTextBox,
                ageLabel, ageTextBox,
                monthLabel, monthTextBox, yearLabel, yearTextBox,
                salaryLabel, salaryTextBox,
                mealLabel, mealTextBox,
                workingDaysLabel, workingDaysTextBox,
                daysOffLabel, daysOffTextBox,
                divider1,
                insuranceLabel, insuranceTextBox,
                taxLabel, taxTextBox
            });

            // Add all controls to right panel
            rightPanel.Controls.AddRange(new Control[] {
                otTitle,
                overtime2xLabel, overtime2xTextBox, overtime2xResultLabel,
                overtime15xLabel, overtime15xTextBox, overtime15xResultLabel,
                divider2,
                mealOTTitle,
                otDays12Label, otDays12TextBox, meal12DisplayLabel, editMeal12Btn,
                otDays8Label, otDays8TextBox, meal8DisplayLabel, editMeal8Btn,
                divider3,
                incentiveTitle,
                attendanceLabel, attendanceTextBox,
                otherBonusLabel, otherBonusTextBox,
                recognizeTextBox
            });

            mainPanel.Controls.Add(leftPanel);
            mainPanel.Controls.Add(rightPanel);
            this.Controls.Add(mainPanel);

            // Calculate Button
            int panelsBottom = Math.Max(leftPanel.Bottom, rightPanel.Bottom);

            // Center action buttons as a group under both columns
            int actionY = panelsBottom + 10;
            int calcWidth = 180;
            int logoutWidth = 175;
            int actionGap = 25;
            int totalActionWidth = calcWidth + actionGap + logoutWidth;
            int actionStartX = (mainPanel.Width - totalActionWidth) / 2;

            Button calculateBtn = new Button();
            calculateBtn.Text = "⚡ TÍNH LƯƠNG";
            calculateBtn.Location = new System.Drawing.Point(actionStartX, actionY);
            calculateBtn.Width = 180;
            calculateBtn.Height = 40;
            calculateBtn.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            calculateBtn.BackColor = System.Drawing.Color.Green;
            calculateBtn.ForeColor = System.Drawing.Color.White;
            calculateBtn.Click += (s, e) => CalculateSalary(nameTextBox, monthTextBox, yearTextBox, salaryTextBox, mealTextBox, workingDaysTextBox, daysOffTextBox, overtime2xTextBox, otDays12TextBox, otDays8TextBox, overtime15xTextBox, insuranceTextBox, taxTextBox, attendanceTextBox, recognizeTextBox, otherBonusTextBox);
            mainPanel.Controls.Add(calculateBtn);

            // Logout Button
            Button logoutBtn = new Button();
            logoutBtn.Text = "🚪 ĐĂNG XUẤT";
            logoutBtn.Location = new System.Drawing.Point(actionStartX + calcWidth + actionGap, actionY);
            logoutBtn.Width = 175;
            logoutBtn.Height = 40;
            logoutBtn.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            logoutBtn.BackColor = System.Drawing.Color.Red;
            logoutBtn.ForeColor = System.Drawing.Color.White;
            logoutBtn.Click += (s, e) => {
                // Đóng form tính lương; LoginForm sẽ hiện lại
                this.Close();
            };
            mainPanel.Controls.Add(logoutBtn);

            // Result Panel
            Panel resultPanel = new Panel();
            int resultX = (mainPanel.Width - 855) / 2;
            resultPanel.Location = new System.Drawing.Point(resultX, panelsBottom + 60);
            resultPanel.Width = 855;
            resultPanel.Height = 205;
            resultPanel.Padding = new Padding(5);
            resultPanel.BorderStyle = BorderStyle.Fixed3D;

            Label resultTitleLabel = new Label();
            resultTitleLabel.Text = "KẾT QUẢ";
            resultTitleLabel.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
            resultTitleLabel.Location = new System.Drawing.Point(10, 5);
            resultTitleLabel.Width = 200;
            resultTitleLabel.Height = 20;

            // Left Column Results
            Label empNameLabel = new Label();
            empNameLabel.Text = "Nhân Viên:";
            empNameLabel.Location = new System.Drawing.Point(10, 30);
            empNameLabel.Width = 400;
            empNameLabel.Height = 18;
            empNameLabel.Name = "empNameLabel";
            empNameLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            empNameLabel.ForeColor = System.Drawing.Color.DarkBlue;

            Label dayRateLabel = new Label();
            dayRateLabel.Text = "Lương 1 ngày:";
            dayRateLabel.Location = new System.Drawing.Point(10, 52);
            dayRateLabel.Width = 400;
            dayRateLabel.Height = 18;
            dayRateLabel.Name = "dayRateLabel";
            dayRateLabel.Font = new System.Drawing.Font("Arial", 9);

            Label grossLabel = new Label();
            grossLabel.Text = "Lương Brutto:";
            grossLabel.Location = new System.Drawing.Point(10, 74);
            grossLabel.Width = 400;
            grossLabel.Height = 18;
            grossLabel.Name = "grossLabel";
            grossLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            grossLabel.ForeColor = System.Drawing.Color.DarkGreen;

            Label insuranceDeductLabel = new Label();
            insuranceDeductLabel.Text = "Khấu Trừ Bảo Hiểm:";
            insuranceDeductLabel.Location = new System.Drawing.Point(10, 96);
            insuranceDeductLabel.Width = 400;
            insuranceDeductLabel.Height = 18;
            insuranceDeductLabel.Name = "insuranceDeductLabel";
            insuranceDeductLabel.Font = new System.Drawing.Font("Arial", 9);

            Label taxDeductLabel = new Label();
            taxDeductLabel.Text = "Khấu Trừ Thuế:";
            taxDeductLabel.Location = new System.Drawing.Point(10, 118);
            taxDeductLabel.Width = 400;
            taxDeductLabel.Height = 18;
            taxDeductLabel.Name = "taxDeductLabel";
            taxDeductLabel.Font = new System.Drawing.Font("Arial", 9);

            Label netLabel = new Label();
            netLabel.Text = "Lương Net (Lương Thực Nhận):";
            netLabel.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            netLabel.ForeColor = System.Drawing.Color.DarkGreen;
            netLabel.Location = new System.Drawing.Point(10, 140);
            netLabel.Width = 400;
            netLabel.Height = 20;
            netLabel.Name = "netLabel";

            // Right Column - Detail breakdown
            Label detailTitleLabel = new Label();
            detailTitleLabel.Text = "CHI TIẾT:";
            detailTitleLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            detailTitleLabel.Location = new System.Drawing.Point(430, 30);
            detailTitleLabel.Width = 400;
            detailTitleLabel.Height = 18;
            detailTitleLabel.ForeColor = System.Drawing.Color.DarkBlue;

            Label detailLabel = new Label();
            detailLabel.Text = "...";
            detailLabel.Location = new System.Drawing.Point(430, 52);
            detailLabel.Width = 410;
            detailLabel.Height = 165;
            detailLabel.Height = 140;
            detailLabel.Name = "detailLabel";
            detailLabel.Font = new System.Drawing.Font("Arial", 8);
            detailLabel.AutoSize = false;

            resultPanel.Controls.AddRange(new Control[] { 
                resultTitleLabel, empNameLabel, dayRateLabel, grossLabel, insuranceDeductLabel, taxDeductLabel, netLabel,
                detailTitleLabel, detailLabel
            });

            mainPanel.Controls.Add(resultPanel);
            this.Controls.Add(mainPanel);

            // Auto-load user data if logged in
            if (!string.IsNullOrEmpty(currentUsername))
            {
                LoadUserData(nameTextBox, salaryTextBox, mealTextBox);
                // Auto-calculate working days for current month
                CalculateWorkingDays(monthTextBox, yearTextBox, workingDaysTextBox);
            }

            // Setup edit button handlers
            Button editMeal12BtnRef = rightPanel.Controls["editMeal12Btn"] as Button;
            Button editMeal8BtnRef = rightPanel.Controls["editMeal8Btn"] as Button;
            Label meal12DisplayLabelRef = rightPanel.Controls.Find("meal12DisplayLabel", false).FirstOrDefault() as Label;
            Label meal8DisplayLabelRef = rightPanel.Controls.Find("meal8DisplayLabel", false).FirstOrDefault() as Label;
            
            if (editMeal12BtnRef != null)
            {
                editMeal12BtnRef.Click += (s, e) => OpenMealEditForm(editMeal12BtnRef, meal12DisplayLabelRef, "Tiền ăn OT 8/12h (mặc định 30,000 VND)");
            }
            
            if (editMeal8BtnRef != null)
            {
                editMeal8BtnRef.Click += (s, e) => OpenMealEditForm(editMeal8BtnRef, meal8DisplayLabelRef, "Tiền ăn OT +4h (mặc định 20,000 VND)");
            }
        }

        private void LoadUserData(TextBox nameTextBox, TextBox salaryTextBox, TextBox mealTextBox)
        {
            var user = userDataManager.Login(currentUsername);
            if (user != null)
            {
                nameTextBox.Text = user.FullName;
                salaryTextBox.Text = NumberFormatter.FormatNumberDisplay(user.BasicSalary.ToString());
                mealTextBox.Text = NumberFormatter.FormatNumberDisplay(user.MealAllowance.ToString());
                
                // Load phone and age
                Control[] phoneFound = this.Controls.Find("phoneTextBox", true);
                if (phoneFound.Length > 0 && phoneFound[0] is TextBox phoneTextBox)
                {
                    phoneTextBox.Text = user.Phone;
                }
                
                Control[] ageFound = this.Controls.Find("ageTextBox", true);
                if (ageFound.Length > 0 && ageFound[0] is TextBox ageTextBox)
                {
                    ageTextBox.Text = user.Age.ToString();
                }
                
                // Load incentive data - find in form controls
                Control[] found = this.Controls.Find("attendanceTextBox", true);
                if (found.Length > 0 && found[0] is TextBox attendanceTextBox)
                {
                    attendanceTextBox.Text = NumberFormatter.FormatNumberDisplay(user.AttendanceIncentive.ToString());
                }
                
                // Don't auto-load recognize count - let user input monthly
            }
        }

        private void OpenEditForm(TextBox nameTextBox, TextBox salaryTextBox, TextBox mealTextBox)
        {
            if (string.IsNullOrEmpty(currentUsername))
            {
                MessageBox.Show("Không có thông tin người dùng để chỉnh sửa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = userDataManager.Login(currentUsername);
            if (user == null)
            {
                MessageBox.Show("Không tìm thấy thông tin người dùng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Create edit dialog
            Form editForm = new Form();
            editForm.Text = "Chỉnh Sửa Thông Tin Nhân Viên";
            editForm.Width = 450;
            editForm.Height = 380;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;

            // Full Name
            Label nameLabel = new Label();
            nameLabel.Text = "Tên đầy đủ:";
            nameLabel.Location = new System.Drawing.Point(30, 30);
            nameLabel.Width = 120;
            editForm.Controls.Add(nameLabel);

            TextBox nameEditBox = new TextBox();
            nameEditBox.Location = new System.Drawing.Point(160, 27);
            nameEditBox.Width = 250;
            nameEditBox.Text = user.FullName;
            editForm.Controls.Add(nameEditBox);

            // Phone/Zalo
            Label phoneLabel = new Label();
            phoneLabel.Text = "SĐT/Zalo:";
            phoneLabel.Location = new System.Drawing.Point(30, 80);
            phoneLabel.Width = 120;
            editForm.Controls.Add(phoneLabel);

            TextBox phoneEditBox = new TextBox();
            phoneEditBox.Location = new System.Drawing.Point(160, 77);
            phoneEditBox.Width = 250;
            phoneEditBox.Text = user.Phone;
            editForm.Controls.Add(phoneEditBox);

            // Age
            Label ageLabel = new Label();
            ageLabel.Text = "Tuổi:";
            ageLabel.Location = new System.Drawing.Point(30, 130);
            ageLabel.Width = 120;
            editForm.Controls.Add(ageLabel);

            TextBox ageEditBox = new TextBox();
            ageEditBox.Location = new System.Drawing.Point(160, 127);
            ageEditBox.Width = 250;
            ageEditBox.Text = user.Age.ToString();
            NumberFormatter.FormatNumberInput(ageEditBox);
            editForm.Controls.Add(ageEditBox);

            // Basic Salary
            Label salaryLabel = new Label();
            salaryLabel.Text = "Lương cơ bản:";
            salaryLabel.Location = new System.Drawing.Point(30, 180);
            salaryLabel.Width = 120;
            editForm.Controls.Add(salaryLabel);

            TextBox salaryEditBox = new TextBox();
            salaryEditBox.Location = new System.Drawing.Point(160, 177);
            salaryEditBox.Width = 250;
            salaryEditBox.Text = NumberFormatter.FormatNumberDisplay(user.BasicSalary.ToString());
            NumberFormatter.FormatNumberInput(salaryEditBox);
            editForm.Controls.Add(salaryEditBox);

            // Meal Allowance
            Label mealLabel = new Label();
            mealLabel.Text = "Tiền ăn/ngày:";
            mealLabel.Location = new System.Drawing.Point(30, 230);
            mealLabel.Width = 120;
            editForm.Controls.Add(mealLabel);

            TextBox mealEditBox = new TextBox();
            mealEditBox.Location = new System.Drawing.Point(160, 227);
            mealEditBox.Width = 250;
            mealEditBox.Text = NumberFormatter.FormatNumberDisplay(user.MealAllowance.ToString());
            NumberFormatter.FormatNumberInput(mealEditBox);
            editForm.Controls.Add(mealEditBox);

            // Attendance Incentive
            Label attendanceLabel = new Label();
            attendanceLabel.Text = "Tiền chuyên cần:";
            attendanceLabel.Location = new System.Drawing.Point(30, 280);
            attendanceLabel.Width = 120;
            editForm.Controls.Add(attendanceLabel);

            TextBox attendanceEditBox = new TextBox();
            attendanceEditBox.Location = new System.Drawing.Point(160, 277);
            attendanceEditBox.Width = 250;
            attendanceEditBox.Text = NumberFormatter.FormatNumberDisplay(user.AttendanceIncentive.ToString());
            NumberFormatter.FormatNumberInput(attendanceEditBox);
            editForm.Controls.Add(attendanceEditBox);

            // Save Button
            Button saveBtn = new Button();
            saveBtn.Text = "💾 Lưu ";
            saveBtn.Location = new System.Drawing.Point(100, 310);
            saveBtn.Width = 120;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            saveBtn.Click += (s, e) =>
            {
                if (UpdateUserData(nameEditBox.Text, phoneEditBox.Text, ageEditBox.Text, salaryEditBox.Text, mealEditBox.Text, attendanceEditBox.Text))
                {
                    LoadUserData(nameTextBox, salaryTextBox, mealTextBox);
                    MessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    editForm.Close();
                }
            };
            editForm.Controls.Add(saveBtn);

            // Cancel Button
            Button cancelBtn = new Button();
            cancelBtn.Text = "❌ Hủy";
            cancelBtn.Location = new System.Drawing.Point(230, 310);
            cancelBtn.Width = 120;
            cancelBtn.Height = 35;
            cancelBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            cancelBtn.BackColor = System.Drawing.Color.Gray;
            cancelBtn.ForeColor = System.Drawing.Color.White;
            cancelBtn.Click += (s, e) => editForm.Close();
            editForm.Controls.Add(cancelBtn);

            editForm.ShowDialog();
        }

        private bool UpdateUserData(string fullName, string phone, string age, string salary, string meal, string attendance)
        {
            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(age) || 
                    string.IsNullOrEmpty(salary) || string.IsNullOrEmpty(meal) || string.IsNullOrEmpty(attendance))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!int.TryParse(age, out int userAge) || !decimal.TryParse(salary, out decimal basicSalary) || 
                    !decimal.TryParse(meal, out decimal mealAllowance) ||
                    !decimal.TryParse(attendance, out decimal attendanceIncentive))
                {
                    MessageBox.Show("Tuổi phải là số, Lương, tiền ăn và tiền chuyên cần phải là số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return userDataManager.Register(currentUsername, fullName, phone, userAge, basicSalary, mealAllowance, attendanceIncentive, 0);
            }
            catch
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CalculateWorkingDays(TextBox monthTextBox, TextBox yearTextBox, TextBox workingDaysTextBox)
        {
            try
            {
                int month = int.Parse(monthTextBox.Text);
                int year = int.Parse(yearTextBox.Text);

                // Calculate working days from 21st of previous month to 20th of current month
                int startMonth = month == 1 ? 12 : month - 1;
                int startYear = month == 1 ? year - 1 : year;
                DateTime startDate = new DateTime(startYear, startMonth, 21);
                DateTime endDate = new DateTime(year, month, 20);

                int workingDays = 0;
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Count only weekdays (Monday to Friday)
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        workingDays++;
                    }
                }

                workingDaysTextBox.Text = workingDays.ToString();
            }
            catch
            {
                // Silently ignore errors during auto-calculation (invalid month/year input)
            }
        }

        private void UpdateDailyRate(TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox daysOffTextBox)
        {
            try
            {
                decimal basicSalary = decimal.Parse(salaryTextBox.Text);
                decimal mealAllowancePerDay = decimal.Parse(mealTextBox.Text);
                decimal workingDays = decimal.Parse(workingDaysTextBox.Text);

                if (workingDays > 0)
                {
                    // Lương 1 ngày tính từ số ngày công ban đầu (không trừ ngày xin nghỉ)
                    decimal basicDailySalary = basicSalary / workingDays;
                    decimal mealDailySalary = mealAllowancePerDay / workingDays;
                    decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;

                    Label dayRateLabel = this.Controls.Find("dayRateLabel", true)[0] as Label;
                    dayRateLabel.Text = $"Lương 1 ngày: {dailySalaryForMeal:C0} VND";
                }
            }
            catch
            {
                // Silently ignore errors during auto-update
            }
        }

        private void CalculateSalary(TextBox nameTextBox, TextBox monthTextBox, TextBox yearTextBox, TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox daysOffTextBox,
                                      TextBox overtime2xTextBox, TextBox otDays12TextBox, TextBox otDays8TextBox, TextBox overtime15xTextBox, TextBox insuranceTextBox, TextBox taxTextBox,
                                      TextBox attendanceTextBox, TextBox recognizeTextBox, TextBox otherBonusTextBox)
        {
            try
            {
                // Parse inputs
                string employeeName = nameTextBox.Text;
                decimal basicSalary = decimal.Parse(salaryTextBox.Text);
                decimal mealAllowancePerDay = decimal.Parse(mealTextBox.Text);
                decimal workingDays = decimal.Parse(workingDaysTextBox.Text);
                decimal daysOff = decimal.Parse(daysOffTextBox.Text);
                decimal overtime2xHours = decimal.Parse(overtime2xTextBox.Text);  // Làm thêm x2 lương
                decimal otDays12 = decimal.Parse(otDays12TextBox.Text);  // Số ngày OT 8/12h
                decimal otDays8 = decimal.Parse(otDays8TextBox.Text);    // Số ngày OT +4h
                decimal overtime15xHours = decimal.Parse(overtime15xTextBox.Text); // Làm thêm x1.5 lương
                decimal insuranceRate = decimal.Parse(insuranceTextBox.Text) / 100;
                decimal taxRate = decimal.Parse(taxTextBox.Text) / 100;
                decimal attendanceIncentive = decimal.Parse(attendanceTextBox.Text); // Tiền chuyên cần
                int recognizeCount = int.Parse(recognizeTextBox.Text); // Số lượng Recognize
                decimal otherBonus = decimal.Parse(otherBonusTextBox.Text); // Tiền bonus khác

                // Get editable meal amounts from edit button Tags
                Button editMeal12Btn = this.Controls.Find("editMeal12Btn", true).FirstOrDefault() as Button;
                Button editMeal8Btn = this.Controls.Find("editMeal8Btn", true).FirstOrDefault() as Button;
                
                decimal meal12Amount = editMeal12Btn != null && decimal.TryParse(editMeal12Btn.Tag.ToString(), out decimal m12) ? m12 : 30000;
                decimal meal8Amount = editMeal8Btn != null && decimal.TryParse(editMeal8Btn.Tag.ToString(), out decimal m8) ? m8 : 20000;

                // Calculate actual working days after deducting days off
                decimal actualWorkingDays = workingDays - daysOff;

                // Calculate total meal allowance for the month
                // Tiền ăn = tiền ăn hàng ngày × số ngày công thực tế
                decimal totalMealAllowance = mealAllowancePerDay * actualWorkingDays;

                // Add bonus meal allowance based on OT days (using editable amounts)
                decimal bonusMealAllowance = 0;
                if (otDays12 > 0)
                {
                    bonusMealAllowance += otDays12 * meal12Amount;
                }
                if (otDays8 > 0)
                {
                    bonusMealAllowance += otDays8 * meal8Amount;
                }

                totalMealAllowance += bonusMealAllowance;

                // Calculate daily salary components (based on original working days, NOT after days off)
                // Lương cơ bản 1 ngày = Lương cơ bản / Số ngày công ban đầu
                decimal basicDailySalary = basicSalary / workingDays;
                
                // Tiền ăn 1 ngày = Tiền ăn hàng ngày / Số ngày công ban đầu
                decimal mealDailySalary = mealAllowancePerDay / workingDays;
                
                // Lương 1 ngày = Lương cơ bản 1 ngày + Tiền ăn 1 ngày (FIXED - không thay đổi khi xin nghỉ)
                decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;

                // Calculate hourly rate based on BASIC SALARY only (for OT calculation)
                // Lương giờ = Lương cơ bản / Số ngày công ban đầu / 8 giờ/ngày
                decimal hourlyRate = basicDailySalary / 8;

                // Calculate gross salary components:
                // 1. Lương từ ngày công thực tế (đã trừ ngày xin nghỉ): actualWorkingDays * dailySalaryForMeal
                // 2. Lương từ tiếng OT x2: overtime2xHours * hourlyRate * 2
                // 3. Lương từ tiếng OT x1.5: overtime15xHours * hourlyRate * 1.5
                decimal regularSalary = actualWorkingDays * dailySalaryForMeal;
                decimal overtime2xSalary = overtime2xHours * hourlyRate * 2;
                decimal overtime15xSalary = overtime15xHours * hourlyRate * 1.5m;

                // Calculate Incentive
                decimal totalIncentive = attendanceIncentive + (recognizeCount * 50000) + otherBonus;

                // Lương Brutto bao gồm tiền ăn bonus và incentive
                decimal grossSalary = regularSalary + overtime2xSalary + overtime15xSalary + bonusMealAllowance + totalIncentive;

                // Calculate deductions - Bảo hiểm chỉ đóng 10.5% lương cơ bản
                decimal insuranceDeduction = basicSalary * 0.105m;
                decimal taxableAmount = grossSalary - insuranceDeduction;
                decimal taxDeduction = taxableAmount * taxRate;

                // Calculate net salary
                decimal netSalary = grossSalary - insuranceDeduction - taxDeduction;

                // Save calculation to user data
                int month = int.Parse(monthTextBox.Text);
                int year = int.Parse(yearTextBox.Text);
                userDataManager.UpdateLastCalculation(currentUsername, month, year, netSalary);

                // Update OT result labels
                Label overtime2xResultLabel = this.Controls.Find("overtime2xResultLabel", true)[0] as Label;
                Label overtime15xResultLabel = this.Controls.Find("overtime15xResultLabel", true)[0] as Label;
                overtime2xResultLabel.Text = $"→ {overtime2xSalary:C0} VND";
                overtime15xResultLabel.Text = $"→ {overtime15xSalary:C0} VND";

                // Display results
                Label empNameLabel = this.Controls.Find("empNameLabel", true)[0] as Label;
                Label grossLabel = this.Controls.Find("grossLabel", true)[0] as Label;
                Label insuranceDeductLabel = this.Controls.Find("insuranceDeductLabel", true)[0] as Label;
                Label taxDeductLabel = this.Controls.Find("taxDeductLabel", true)[0] as Label;
                Label netLabel = this.Controls.Find("netLabel", true)[0] as Label;
                Label detailLabel = this.Controls.Find("detailLabel", true)[0] as Label;
                Label dayRateLabel = this.Controls.Find("dayRateLabel", true)[0] as Label;

                empNameLabel.Text = $"Nhân Viên: {employeeName}";
                grossLabel.Text = $"Lương Brutto: {grossSalary:C0} VND";
                insuranceDeductLabel.Text = $"Khấu Trừ Bảo Hiểm (10.5% lương cơ bản): {insuranceDeduction:C0} VND";
                taxDeductLabel.Text = $"Khấu Trừ Thuế: {taxDeduction:C0} VND";
                netLabel.Text = $"Lương Net (Thực Nhận): {netSalary:C0} VND";
                dayRateLabel.Text = $"Lương 1 ngày: {dailySalaryForMeal:C0} VND";
                
                // Show detail breakdown
                string bonusInfo = "";
                if (otDays12 > 0)
                {
                    bonusInfo += $"\n  • Tiền ăn OT 8/12h ({otDays12:F0} ngày × {meal12Amount:C0}): {otDays12 * meal12Amount:C0} VND";
                }
                if (otDays8 > 0)
                {
                    bonusInfo += $"\n  • Tiền ăn OT +4h ({otDays8:F0} ngày × {meal8Amount:C0}): {otDays8 * meal8Amount:C0} VND";
                }
                
                string incentiveInfo = $"\n  • Tiền chuyên cần: {attendanceIncentive:C0} VND";
                if (recognizeCount > 0)
                {
                    incentiveInfo += $"\n  • Recognize ({recognizeCount} × 50k): {recognizeCount * 50000:C0} VND";
                }
                if (otherBonus > 0)
                {
                    incentiveInfo += $"\n  • Tiền bonus khác: {otherBonus:C0} VND";
                }
                
                string detail = $"Chi Tiết:\n" +
                    $"  • Lương ngày công ({actualWorkingDays:F1} ngày × {dailySalaryForMeal:C0}): {regularSalary:C0} VND\n" +
                    $"  • OT x2 ({overtime2xHours:F1} tiếng × {hourlyRate:C0} × 2): {overtime2xSalary:C0} VND\n" +
                    $"  • OT x1.5 ({overtime15xHours:F1} tiếng × {hourlyRate:C0} × 1.5): {overtime15xSalary:C0} VND{bonusInfo}{incentiveInfo}";
                detailLabel.Text = detail;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Vui lòng nhập các giá trị số hợp lệ!\n" + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenMealEditForm(Button button, Label displayLabel, string title)
        {
            Form editForm = new Form();
            editForm.Text = "Chỉnh Sửa " + title;
            editForm.Width = 350;
            editForm.Height = 150;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;
            editForm.BackColor = System.Drawing.Color.White;

            Label label = new Label();
            label.Text = "Số tiền (VND):";
            label.Location = new System.Drawing.Point(30, 30);
            label.Width = 120;
            editForm.Controls.Add(label);

            TextBox amountBox = new TextBox();
            amountBox.Location = new System.Drawing.Point(160, 27);
            amountBox.Width = 150;
            amountBox.Height = 20;
            amountBox.Font = new System.Drawing.Font("Arial", 9);
            amountBox.TextAlign = HorizontalAlignment.Left;
            amountBox.BorderStyle = BorderStyle.Fixed3D;
            amountBox.BackColor = System.Drawing.Color.White;
            amountBox.Text = button.Tag.ToString();
            NumberFormatter.FormatNumberInput(amountBox);
            editForm.Controls.Add(amountBox);

            Button saveBtn = new Button();
            saveBtn.Text = "💾 Lưu";
            saveBtn.Location = new System.Drawing.Point(80, 80);
            saveBtn.Width = 100;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            saveBtn.Click += (s, e) =>
            {
                if (decimal.TryParse(amountBox.Text, out decimal amount))
                {
                    button.Tag = amount;
                    // Update display label with k format
                    if (displayLabel != null)
                    {
                        decimal k = amount / 1000;
                        displayLabel.Text = $"× {k:F0}k";
                    }
                    editForm.Close();
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số tiền hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            editForm.Controls.Add(saveBtn);

            editForm.ShowDialog();
        }


    }
}
