using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Media;
using SalaryCalculator;

namespace SalaryCalculator
{
    public partial class SalaryCalculatorForm : Form
    {
            // Đúng vị trí bên trong class
            private string currentUsername;
            private UserDataManager userDataManager = new UserDataManager();
            private bool isCustomTaxRate = false;  // Flag để theo dõi người dùng nhập % thủ công
            private const decimal BaseTaxThreshold = 16230000m; // Mốc lương tính thuế cơ bản mặc định
            private const decimal FixedThresholdAddon = 730000m; // Khoản cố định cộng vào mốc thuế

        public SalaryCalculatorForm(string username = "")
        {
            currentUsername = username;
            isCustomTaxRate = false;  // Reset lại trạng thái % thuế mỗi khi đăng nhập lại
            InitializeComponent();
            // Để LoginForm kiểm soát quay lại khi form này đóng

            // Kiểm tra cập nhật tự động khi khởi động form
            CheckForUpdate();
        }

        private async void CheckForUpdate()
        {
            var result = await UpdateChecker.CheckForUpdateAsync();
            if (result.hasUpdate)
            {
                string exeUrl = await UpdateChecker.GetLatestExeDownloadUrlAsync();
                UpdateChecker.ShowManualUpdateDialog(result.latestVersion, exeUrl);
            }
        }

        private void InitializeComponent()
        {
            if (currentUsername == "admin")
            {
                // Khởi tạo bảng xếp hạng mới, không dùng panel lồng ghép
                int month = DateTime.Now.Month;
                int year = DateTime.Now.Year;
                int formPadding = 32;
                int gridWidth = 820;
                int gridHeight = 470;
                int formWidth = gridWidth + formPadding * 2;
                int formHeight = gridHeight + 100;
                this.Text = $"BẢNG XẾP HẠNG LƯƠNG THÁNG {month:D2}/{year}";
                this.Width = formWidth;
                this.Height = formHeight;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.Font = new System.Drawing.Font("Arial", 9);
                this.AutoScroll = false;

                // Tiêu đề lớn trên cùng
                Label rankingTitle = new Label();
                rankingTitle.Text = $"BẢNG XẾP HẠNG LƯƠNG THÁNG {month:D2}/{year}";
                rankingTitle.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                rankingTitle.ForeColor = System.Drawing.Color.DarkBlue;
                rankingTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                rankingTitle.Width = gridWidth;
                rankingTitle.Height = 38;
                rankingTitle.Location = new System.Drawing.Point((formWidth - gridWidth) / 2, 20);
                this.Controls.Add(rankingTitle);

                // DataGridView 4 cột, fill chiều rộng, border đẹp, header rõ ràng
                DataGridView rankingGrid = new DataGridView();
                rankingGrid.Location = new System.Drawing.Point((formWidth - gridWidth) / 2, rankingTitle.Bottom + 10);
                rankingGrid.Width = gridWidth;
                rankingGrid.Height = gridHeight;
                rankingGrid.BorderStyle = BorderStyle.None;
                rankingGrid.GridColor = Color.FromArgb(230, 230, 230);
                rankingGrid.BackgroundColor = Color.White;
                rankingGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                rankingGrid.ColumnCount = 4;
                rankingGrid.Columns[0].Name = "Hạng";
                rankingGrid.Columns[1].Name = "Tên Nhân Viên";
                rankingGrid.Columns[2].Name = "Lương Thực Nhận";
                rankingGrid.Columns[3].Name = "Nhận Xét";
                // Tối ưu độ rộng cột: cột Hạng nhỏ, cột Nhận Xét lớn nhất
                rankingGrid.Columns[0].Width = 50;
                rankingGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                rankingGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                rankingGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                rankingGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                rankingGrid.Columns[1].FillWeight = 1.2f;
                rankingGrid.Columns[2].FillWeight = 1.1f;
                rankingGrid.Columns[3].FillWeight = 2.7f;
                rankingGrid.ReadOnly = true;
                rankingGrid.AllowUserToAddRows = false;
                rankingGrid.AllowUserToDeleteRows = false;
                rankingGrid.RowHeadersVisible = false;
                rankingGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                // Prevent users from resizing or reordering
                rankingGrid.AllowUserToResizeColumns = false;
                rankingGrid.AllowUserToResizeRows = false;
                rankingGrid.AllowUserToOrderColumns = false;
                rankingGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
                // Prevent header text from wrapping and lock header height to avoid stretching
                rankingGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
                rankingGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                rankingGrid.ColumnHeadersHeight = 40; // fixed header height
                rankingGrid.EnableHeadersVisualStyles = false;
                // Header style (orange primary like ecommerce)
                rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 90, 0);
                rankingGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                rankingGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                rankingGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.RowsDefaultCellStyle.BackColor = Color.White;
                rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.White; // no zebra
                rankingGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 235, 205);
                rankingGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(34, 34, 34);
                rankingGrid.DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                rankingGrid.RowTemplate.Height = 36;
                rankingGrid.RowTemplate.Resizable = DataGridViewTriState.False;
                rankingGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                rankingGrid.EnableHeadersVisualStyles = false;
                rankingGrid.Margin = new Padding(8);
                // Tắt chức năng sort khi click vào tiêu đề và khóa từng cột
                foreach (DataGridViewColumn col in rankingGrid.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    if (col.Index == 0)
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
                    col.Resizable = DataGridViewTriState.False;
                }

                // ...existing code...
                int minRows = 20;

                string[] compliments = new string[] {
                    "Quá xuất sắc!", "Đỉnh của chóp!", "Lương mơ ước!", "Tuyệt vời ông mặt trời!", "Đáng ngưỡng mộ!", "Làm việc như siêu nhân!", "Thu nhập cực khủng!", "Cố gắng phát huy!", "Làm việc hết mình!", "Chuyên gia tiết kiệm!",
                    "Lương cao ngất ngưởng!", "Đồng nghiệp ngưỡng mộ!", "Sếp cũng phải nể!", "Làm việc chăm chỉ!", "Tấm gương sáng!", "Công thần của công ty!", "Bậc thầy tài chính!", "Làm việc hiệu quả!", "Thành tích tuyệt vời!", "Lương tăng vèo vèo!",
                    "Được thưởng nóng!", "Làm việc không biết mệt!", "Cỗ máy kiếm tiền!", "Người truyền cảm hứng!", "Làm việc siêu tốc!", "Đỉnh cao nghề nghiệp!", "Lương vượt chỉ tiêu!", "Chuyên gia tăng ca!", "Làm việc chuẩn chỉnh!", "Được lòng sếp lớn!",
                    "Làm việc như robot!", "Không ai sánh bằng!", "Lương tháng này quá đã!", "Được vinh danh toàn công ty!", "Làm việc xuất thần!", "Công nhận tài năng!", "Làm việc không ngừng nghỉ!", "Lương như mơ!", "Được đồng nghiệp yêu quý!", "Làm việc cực kỳ hiệu quả!",
                    "Làm việc siêu năng suất!", "Lương tăng đều đều!", "Được thưởng lớn!", "Làm việc tận tâm!", "Làm việc sáng tạo!", "Làm việc chuyên nghiệp!", "Làm việc gương mẫu!", "Làm việc xuất sắc!", "Làm việc nhiệt huyết!", "Làm việc tận tụy!"
                };
                string[] encouragements = new string[] {
                    "Cố gắng hơn nữa nhé!", "Đừng nản lòng!", "Sắp vào top rồi!", "Nỗ lực sẽ được đền đáp!", "Chỉ cần cố thêm chút nữa!", "Đừng bỏ cuộc!", "Cơ hội vẫn còn phía trước!", "Hãy kiên trì!", "Cần bứt phá mạnh mẽ hơn!", "Đừng để lương tháng sau thấp hơn nhé!",
                    "Cần chăm chỉ hơn!", "Hãy hỏi bí quyết từ top trên!", "Đừng để bị bỏ lại phía sau!", "Cố lên, bạn làm được!", "Hãy xem lại mục tiêu!", "Đừng để sếp nhắc nhở!", "Cần cải thiện hiệu suất!", "Đừng để đồng nghiệp vượt mặt!", "Hãy tự tin hơn!", "Lương thấp không phải mãi mãi!"
                };
                var rand = new Random();
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
                // Luôn sắp xếp lương từ cao xuống thấp
                var sortedBySalary = users.OrderByDescending(u => u.LastCalculatedYear == year && u.LastCalculatedMonth == month ? u.LastNetSalary : 0).ToList();
                foreach (var u in sortedBySalary)
                {
                    string rankDisplay = rank.ToString();
                    if (rank == 1) rankDisplay = "🏆1"; // Cúp vàng
                    else if (rank == 2) rankDisplay = "🥈2"; // Huy chương bạc
                    else if (rank == 3) rankDisplay = "🥉3"; // Huy chương đồng
                    // Chỉ khen nếu có lương tháng hiện tại, còn lại động viên/chê
                    string message;
                    if (u.LastCalculatedMonth == month && u.LastCalculatedYear == year && u.LastNetSalary > 0)
                    {
                        message = rank <= 7 ? GetNextCompliment() : GetNextEncouragement();
                    }
                    else
                    {
                        message = GetNextEncouragement();
                    }
                    int rowIdx = rankingGrid.Rows.Add(rankDisplay, u.FullName, u.LastNetSalary.ToString("N0"), message);
                    // Làm nổi bật 3 hạng đầu — chỉ override background cho 3 hàng này
                    // Apply bold fonts for top 3 and a subtle light-orange background to make them stand out
                    if (rank == 1)
                    {
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(255, 244, 230);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                    }
                    else if (rank == 2)
                    {
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 230);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                    }
                    else if (rank == 3)
                    {
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 10.5f, System.Drawing.FontStyle.Bold);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 236);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                    }
                    else
                    {
                        // Regular rows: consistent white background and dark text
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9.5f, System.Drawing.FontStyle.Regular);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.White;
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                    }
                    rank++;
                }
                // Thêm dòng trống nếu ít hơn 20 hạng
                for (int i = sorted.Count + 1; i <= minRows; i++)
                {
                    int idx = rankingGrid.Rows.Add(i.ToString(), "", "", "");
                    // Make sure blank rows are white like the rest
                    rankingGrid.Rows[idx].DefaultCellStyle.BackColor = Color.White;
                }
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
                // Apply ecommerce theme so admin ranking uses the same design system
                try { Theme.ApplyEcommerceTheme(this); } catch { }

                // Theme.ApplyEcommerceTheme may apply a DataGridView style intended for other themes.
                // Re-assert the ranking grid's white styling so numbers remain dark and readable.
                try
                {
                    rankingGrid.BackgroundColor = Color.White;
                    rankingGrid.RowsDefaultCellStyle.BackColor = Color.White;
                    rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                    rankingGrid.DefaultCellStyle.BackColor = Color.White;
                    rankingGrid.DefaultCellStyle.ForeColor = Color.FromArgb(34, 34, 34);
                    rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 90, 0);
                    rankingGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    rankingGrid.EnableHeadersVisualStyles = false;
                }
                catch { }

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

            // Small Calculator Launcher Button (bottom-right inside mainPanel)
            Button calculatorLauncher = new Button();
            // Use a calculator-like glyph; fallback to text if emoji not available
            calculatorLauncher.Text = "🔢";
            calculatorLauncher.Width = 44;
            calculatorLauncher.Height = 28;
            calculatorLauncher.Font = new System.Drawing.Font("Segoe UI Emoji", 12);
            calculatorLauncher.BackColor = Color.FromArgb(255, 90, 0);
            calculatorLauncher.ForeColor = Color.White;
            calculatorLauncher.FlatStyle = FlatStyle.Flat;
            calculatorLauncher.FlatAppearance.BorderSize = 0;
            calculatorLauncher.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            calculatorLauncher.Click += (s, e) => {
                try
                {
                    var calc = new CalculatorForm();
                    // Center the calculator over the current form to avoid off-screen placement
                    calc.StartPosition = FormStartPosition.Manual;
                    var parentCenter = new Point(this.Location.X + this.Width / 2, this.Location.Y + this.Height / 2);
                    var calcLocation = new Point(parentCenter.X - calc.Width / 2, parentCenter.Y - calc.Height / 2);
                    // Ensure calculator is within primary screen bounds
                    var screenBounds = Screen.PrimaryScreen.WorkingArea;
                    if (calcLocation.X < screenBounds.Left) calcLocation.X = screenBounds.Left + 16;
                    if (calcLocation.Y < screenBounds.Top) calcLocation.Y = screenBounds.Top + 16;
                    if (calcLocation.X + calc.Width > screenBounds.Right) calcLocation.X = screenBounds.Right - calc.Width - 16;
                    if (calcLocation.Y + calc.Height > screenBounds.Bottom) calcLocation.Y = screenBounds.Bottom - calc.Height - 16;
                    calc.Location = calcLocation;
                    calc.Show(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể mở máy tính: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            // We'll add this button into mainPanel after mainPanel is created so anchoring works

            // Main Panel with Auto Scroll
            Panel mainPanel = new Panel();
            mainPanel.Location = new System.Drawing.Point((this.Width - 885) / 2 - 8, 32);
            mainPanel.Width = 885;
            mainPanel.Height = 740; // increase height to show all content without scrolling
            mainPanel.AutoScroll = false; // disable scroll as requested

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
            taxLabel.Width = 60;
            taxLabel.Height = 18;

            TextBox taxTextBox = new TextBox();
            taxTextBox.Location = new System.Drawing.Point(75, leftY + 1);
            taxTextBox.Width = 60;
            taxTextBox.Height = 20;
            taxTextBox.Name = "taxTextBox";
            taxTextBox.Font = new System.Drawing.Font("Arial", 8);
            taxTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(taxTextBox);
            // Thêm event để phát hiện khi người dùng nhập % thủ công
            taxTextBox.TextChanged += (s, e) => { isCustomTaxRate = true; };

            Label taxThresholdLabel = new Label();
            taxThresholdLabel.Text = "Mốc lương tính thuế:";
            taxThresholdLabel.Location = new System.Drawing.Point(145, leftY);
            taxThresholdLabel.Width = 120;
            taxThresholdLabel.Height = 18;

            TextBox taxThresholdTextBox = new TextBox();
            taxThresholdTextBox.Location = new System.Drawing.Point(270, leftY + 1);
            taxThresholdTextBox.Width = 135;
            taxThresholdTextBox.Height = 20;
            taxThresholdTextBox.Name = "taxThresholdTextBox";
            taxThresholdTextBox.Font = new System.Drawing.Font("Arial", 8);
            taxThresholdTextBox.Text = "";
            NumberFormatter.FormatNumberInput(taxThresholdTextBox);
            taxThresholdTextBox.ReadOnly = true;
            taxThresholdTextBox.BackColor = System.Drawing.Color.LightGray;

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

            // OT x3
            Label overtime3xLabel = new Label();
            overtime3xLabel.Text = "Số Giờ (x3):";
            overtime3xLabel.Location = new System.Drawing.Point(10, rightY);
            overtime3xLabel.Width = 90;
            overtime3xLabel.Height = 18;

            TextBox overtime3xTextBox = new TextBox();
            overtime3xTextBox.Location = new System.Drawing.Point(105, rightY);
            overtime3xTextBox.Width = 80;
            overtime3xTextBox.Height = 22;
            overtime3xTextBox.Name = "overtime3xTextBox";
            overtime3xTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(overtime3xTextBox);

            Label overtime3xResultLabel = new Label();
            overtime3xResultLabel.Text = "→ 0 VND";
            overtime3xResultLabel.Location = new System.Drawing.Point(190, rightY);
            overtime3xResultLabel.Width = 210;
            overtime3xResultLabel.Height = 18;
            overtime3xResultLabel.Name = "overtime3xResultLabel";
            overtime3xResultLabel.ForeColor = System.Drawing.Color.DarkOrange;
            overtime3xResultLabel.Font = new System.Drawing.Font("Arial", 8, System.Drawing.FontStyle.Bold);

            // Thêm vào panel hoặc Controls
            rightPanel.Controls.Add(overtime3xLabel);
            rightPanel.Controls.Add(overtime3xTextBox);
            rightPanel.Controls.Add(overtime3xResultLabel);

            // Thêm vào danh sách controls để truyền vào hàm tính lương
            // (Chèn đúng vị trí, cập nhật các chỗ gọi CalculateSalary)

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
                taxLabel, taxTextBox, taxThresholdLabel, taxThresholdTextBox
            });

            // Add all controls to right panel
            rightPanel.Controls.AddRange(new Control[] {
                otTitle,
                overtime2xLabel, overtime2xTextBox, overtime2xResultLabel,
                overtime3xLabel, overtime3xTextBox, overtime3xResultLabel,
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
            calculateBtn.Click += (s, e) =>
            {
                // Disable calculate button during calculation
                calculateBtn.Enabled = false;

                // Perform calculation
                CalculateSalary(nameTextBox, monthTextBox, yearTextBox, salaryTextBox, mealTextBox, workingDaysTextBox, daysOffTextBox, overtime2xTextBox, overtime3xTextBox, otDays12TextBox, otDays8TextBox, overtime15xTextBox, insuranceTextBox, taxTextBox, attendanceTextBox, recognizeTextBox, otherBonusTextBox, taxThresholdTextBox);

                // Re-enable button after calculation
                calculateBtn.Enabled = true;
            };
            mainPanel.Controls.Add(calculateBtn);

            // Place calculator launcher inside mainPanel near the bottom-right corner
            try
            {
                // Position it to the right of logout button area; anchor keeps it at bottom-right
                calculatorLauncher.Location = new Point(mainPanel.Width - calculatorLauncher.Width - 12, actionY + 2);
                mainPanel.Controls.Add(calculatorLauncher);
            }
            catch { mainPanel.Controls.Add(calculatorLauncher); }

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
            // Lift result panel a bit higher to avoid touching bottom edge
            resultPanel.Location = new System.Drawing.Point(resultX, panelsBottom + 75);
            resultPanel.Width = 855;
            resultPanel.Height = 240;
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
            empNameLabel.Text = "";
            empNameLabel.Location = new System.Drawing.Point(10, 40);
            empNameLabel.Width = 400;
            empNameLabel.Height = 18;
            empNameLabel.Name = "empNameLabel";
            empNameLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            empNameLabel.ForeColor = System.Drawing.Color.DarkBlue;
            int detailY = 40; // bắt đầu từ nhân viên
            // spacing đều 19px, font 8.5pt cho tất cả, riêng Net và Brutto in đậm
            int detailSpacing = 19;
            float detailFont = 8.5f;
            float detailFontBold = 8.5f;

            detailY += detailSpacing;
            Label dayRate8hLabel = new Label();
            dayRate8hLabel.Text = "";
            dayRate8hLabel.Location = new System.Drawing.Point(10, detailY);
            dayRate8hLabel.Width = 400;
            dayRate8hLabel.Height = 18;
            dayRate8hLabel.Name = "dayRate8hLabel";
            dayRate8hLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label mealDayLabel = new Label();
            mealDayLabel.Text = "";
            mealDayLabel.Location = new System.Drawing.Point(10, detailY);
            mealDayLabel.Width = 400;
            mealDayLabel.Height = 18;
            mealDayLabel.Name = "mealDayLabel";
            mealDayLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label dayRateLabel = new Label();
            dayRateLabel.Text = "";
            dayRateLabel.Location = new System.Drawing.Point(10, detailY);
            dayRateLabel.Width = 400;
            dayRateLabel.Height = 18;
            dayRateLabel.Name = "dayRateLabel";
            dayRateLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label grossLabel = new Label();
            grossLabel.Text = "";
            grossLabel.Location = new System.Drawing.Point(10, detailY);
            grossLabel.Width = 400;
            grossLabel.Height = 18;
            grossLabel.Name = "grossLabel";
            grossLabel.Font = new System.Drawing.Font("Arial", detailFontBold, System.Drawing.FontStyle.Bold);
            grossLabel.ForeColor = System.Drawing.Color.DarkGreen;

            detailY += detailSpacing;
            Label insuranceDeductLabel = new Label();
            insuranceDeductLabel.Text = "";
            insuranceDeductLabel.Location = new System.Drawing.Point(10, detailY);
            insuranceDeductLabel.Width = 400;
            insuranceDeductLabel.Height = 18;
            insuranceDeductLabel.Name = "insuranceDeductLabel";
            insuranceDeductLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label taxThresholdResultLabel = new Label();
            taxThresholdResultLabel.Text = "";
            taxThresholdResultLabel.Location = new System.Drawing.Point(10, detailY);
            taxThresholdResultLabel.Width = 400;
            taxThresholdResultLabel.Height = 18;
            taxThresholdResultLabel.Name = "taxThresholdResultLabel";
            taxThresholdResultLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label taxDeductLabel = new Label();
            taxDeductLabel.Text = "";
            taxDeductLabel.Location = new System.Drawing.Point(10, detailY);
            taxDeductLabel.Width = 400;
            taxDeductLabel.Height = 18;
            taxDeductLabel.Name = "taxDeductLabel";
            taxDeductLabel.Font = new System.Drawing.Font("Arial", detailFont);


            detailY += detailSpacing;
            Label netLabel = new Label();
            netLabel.Text = "";
            netLabel.Font = new System.Drawing.Font("Arial", detailFontBold, System.Drawing.FontStyle.Bold);
            netLabel.ForeColor = System.Drawing.Color.DarkGreen;
            netLabel.Location = new System.Drawing.Point(10, detailY);
            netLabel.Width = 400;
            netLabel.Height = 18;
            netLabel.Name = "netLabel";

            detailY += detailSpacing;
            // Right Column - Detail breakdown
            Label detailTitleLabel = new Label();
            detailTitleLabel.Text = "CHI TIẾT:";
            detailTitleLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            detailTitleLabel.Location = new System.Drawing.Point(430, 30);
            detailTitleLabel.Width = 400;
            detailTitleLabel.Height = 18;
            detailTitleLabel.ForeColor = System.Drawing.Color.DarkBlue;

            Label detailLabel = new Label();
            detailLabel.Text = "";
            detailLabel.Location = new System.Drawing.Point(430, 52);
            detailLabel.Width = 410;
            // Increase height so detailed breakdown lines are not clipped by other panels
            detailLabel.Height = 180;
            detailLabel.Name = "detailLabel";
            detailLabel.Font = new System.Drawing.Font("Arial", 8);
            detailLabel.AutoSize = false;

            resultPanel.Controls.AddRange(new Control[] { 
                resultTitleLabel, empNameLabel, dayRate8hLabel, mealDayLabel, dayRateLabel, grossLabel, insuranceDeductLabel, taxDeductLabel, taxThresholdResultLabel, netLabel,
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

            // Apply e-commerce theme tweaks
            try { Theme.ApplyEcommerceTheme(this); } catch { }
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
                // Load tax threshold
                Control[] taxThresholdFound = this.Controls.Find("taxThresholdTextBox", true);
                if (taxThresholdFound.Length > 0 && taxThresholdFound[0] is TextBox taxThresholdTextBox)
                {
                    decimal thresholdToShow = user.TaxThreshold > 0 ? user.TaxThreshold : BaseTaxThreshold;
                    taxThresholdTextBox.Text = NumberFormatter.FormatNumberDisplay(thresholdToShow);
                }
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
            editForm.Height = 440;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;
            try { Theme.ApplyEcommerceTheme(editForm); } catch { }

            // Full Name
            int startY = 30, gapY = 50;
            Label nameLabel = new Label();
            nameLabel.Text = "Tên đầy đủ:";
            nameLabel.Location = new System.Drawing.Point(30, startY);
            nameLabel.Width = 120;
            editForm.Controls.Add(nameLabel);

            TextBox nameEditBox = new TextBox();
            nameEditBox.Location = new System.Drawing.Point(160, startY - 3);
            nameEditBox.Width = 250;
            nameEditBox.Text = user.FullName;
            editForm.Controls.Add(nameEditBox);

            // Phone/Zalo
            Label phoneLabel = new Label();
            phoneLabel.Text = "SĐT/Zalo:";
            phoneLabel.Location = new System.Drawing.Point(30, startY + gapY);
            phoneLabel.Width = 120;
            editForm.Controls.Add(phoneLabel);

            TextBox phoneEditBox = new TextBox();
            phoneEditBox.Location = new System.Drawing.Point(160, startY + gapY - 3);
            phoneEditBox.Width = 250;
            phoneEditBox.Text = user.Phone;
            editForm.Controls.Add(phoneEditBox);

            // Age
            Label ageLabel = new Label();
            ageLabel.Text = "Tuổi:";
            ageLabel.Location = new System.Drawing.Point(30, startY + gapY * 2);
            ageLabel.Width = 120;
            editForm.Controls.Add(ageLabel);

            TextBox ageEditBox = new TextBox();
            ageEditBox.Location = new System.Drawing.Point(160, startY + gapY * 2 - 3);
            ageEditBox.Width = 250;
            ageEditBox.Text = user.Age.ToString();
            NumberFormatter.FormatNumberInput(ageEditBox);
            editForm.Controls.Add(ageEditBox);

            // Basic Salary
            Label salaryLabel = new Label();
            salaryLabel.Text = "Lương cơ bản:";
            salaryLabel.Location = new System.Drawing.Point(30, startY + gapY * 3);
            salaryLabel.Width = 120;
            editForm.Controls.Add(salaryLabel);

            TextBox salaryEditBox = new TextBox();
            salaryEditBox.Location = new System.Drawing.Point(160, startY + gapY * 3 - 3);
            salaryEditBox.Width = 250;
            salaryEditBox.Text = NumberFormatter.FormatNumberDisplay(user.BasicSalary.ToString());
            NumberFormatter.FormatNumberInput(salaryEditBox);
            editForm.Controls.Add(salaryEditBox);

            // Meal Allowance
            Label mealLabel = new Label();
            mealLabel.Text = "Tiền ăn/ngày:";
            mealLabel.Location = new System.Drawing.Point(30, startY + gapY * 4);
            mealLabel.Width = 120;
            editForm.Controls.Add(mealLabel);

            TextBox mealEditBox = new TextBox();
            mealEditBox.Location = new System.Drawing.Point(160, startY + gapY * 4 - 3);
            mealEditBox.Width = 250;
            mealEditBox.Text = NumberFormatter.FormatNumberDisplay(user.MealAllowance.ToString());
            NumberFormatter.FormatNumberInput(mealEditBox);
            editForm.Controls.Add(mealEditBox);

            // Attendance Incentive
            Label attendanceLabel = new Label();
            attendanceLabel.Text = "Tiền chuyên cần:";
            attendanceLabel.Location = new System.Drawing.Point(30, startY + gapY * 5);
            attendanceLabel.Width = 120;
            editForm.Controls.Add(attendanceLabel);

            TextBox attendanceEditBox = new TextBox();
            attendanceEditBox.Location = new System.Drawing.Point(160, startY + gapY * 5 - 3);
            attendanceEditBox.Width = 250;
            attendanceEditBox.Text = NumberFormatter.FormatNumberDisplay(user.AttendanceIncentive.ToString());
            NumberFormatter.FormatNumberInput(attendanceEditBox);
            editForm.Controls.Add(attendanceEditBox);

            // Tax Threshold
            Label taxThresholdLabel = new Label();
            taxThresholdLabel.Text = "Mốc lương tính thuế:";
            taxThresholdLabel.Location = new System.Drawing.Point(30, startY + gapY * 6);
            taxThresholdLabel.Width = 120;
            editForm.Controls.Add(taxThresholdLabel);

            TextBox taxThresholdEditBox = new TextBox();
            taxThresholdEditBox.Location = new System.Drawing.Point(160, startY + gapY * 6 - 3);
            taxThresholdEditBox.Width = 250;
            taxThresholdEditBox.Text = user.TaxThreshold > 0 ? NumberFormatter.FormatNumberDisplay(user.TaxThreshold) : "";
            NumberFormatter.FormatNumberInput(taxThresholdEditBox);
            editForm.Controls.Add(taxThresholdEditBox);

            // Save Button
            Button saveBtn = new Button();
            saveBtn.Text = "💾 Lưu ";
            saveBtn.Width = 120;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            // Căn giữa hai nút, cách đều hai bên
            int btnY = startY + gapY * 7 - 20;
            int btnGap = 30;
            int formWidth = editForm.ClientSize.Width;
            int totalBtnWidth = saveBtn.Width + btnGap + 120;
            int btnStartX = (formWidth - totalBtnWidth) / 2;
            saveBtn.Location = new System.Drawing.Point(btnStartX, btnY);
            saveBtn.Click += (s, e) =>
            {
                if (UpdateUserData(nameEditBox.Text, phoneEditBox.Text, ageEditBox.Text, salaryEditBox.Text, mealEditBox.Text, attendanceEditBox.Text, taxThresholdEditBox.Text))
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
            cancelBtn.Width = 120;
            cancelBtn.Height = 35;
            cancelBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            cancelBtn.BackColor = System.Drawing.Color.Gray;
            cancelBtn.ForeColor = System.Drawing.Color.White;
            cancelBtn.Location = new System.Drawing.Point(btnStartX + saveBtn.Width + btnGap, btnY);
            cancelBtn.Click += (s, e) => editForm.Close();
            editForm.Controls.Add(cancelBtn);

            // Tweak save button to use ecommerce primary color
            try { saveBtn.BackColor = System.Drawing.Color.FromArgb(255, 90, 0); } catch { }

            editForm.ShowDialog();
        }

        private bool UpdateUserData(string fullName, string phone, string age, string salary, string meal, string attendance)
        {
            try
            {
                // Overload: attendance, taxThreshold
                return UpdateUserData(fullName, phone, age, salary, meal, attendance, "0");
            }
            catch
            {
                MessageBox.Show("Có lỗi xảy ra khi cập nhật thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

        }

        private bool UpdateUserData(string fullName, string phone, string age, string salary, string meal, string attendance, string taxThreshold)
        {
            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(age) || 
                    string.IsNullOrEmpty(salary) || string.IsNullOrEmpty(meal) || string.IsNullOrEmpty(attendance) || string.IsNullOrEmpty(taxThreshold))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (!int.TryParse(age, out int userAge) || !decimal.TryParse(salary, out decimal basicSalary) || 
                    !decimal.TryParse(meal, out decimal mealAllowance) ||
                    !decimal.TryParse(attendance, out decimal attendanceIncentive) || !decimal.TryParse(taxThreshold, out decimal taxThresholdValue))
                {
                    MessageBox.Show("Tuổi phải là số, Lương, tiền ăn, tiền chuyên cần và mốc lương tính thuế phải là số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (taxThresholdValue <= 0)
                {
                    MessageBox.Show("Mốc lương tính thuế phải lớn hơn 0!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return userDataManager.Register(currentUsername, fullName, phone, userAge, basicSalary, mealAllowance, attendanceIncentive, 0, taxThresholdValue);
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
                    dayRateLabel.Text = $"Tổng lương 1 ngày công: {dailySalaryForMeal:C0} VND";
                }
            }
            catch
            {
                // Silently ignore errors during auto-update
            }
        }

        private decimal ComputeTaxThreshold(decimal baseThreshold, decimal hourlyRate, decimal overtime2xHours, decimal overtime3xHours, decimal insuranceDeduction)
        {
            // Tính tiền OT miễn thuế với giới hạn 28 tiếng
            const decimal maxTaxExemptHours = 28m;
            decimal taxExemptOT = 0m;
            
            // Ưu tiên số tiếng x3 trước
            if (overtime3xHours > 0)
            {
                decimal x3HoursForExempt = Math.Min(overtime3xHours, maxTaxExemptHours);
                taxExemptOT += hourlyRate * x3HoursForExempt * 2m;
                
                // Nếu còn giới hạn, tính thêm từ x2
                decimal remainingHours = maxTaxExemptHours - x3HoursForExempt;
                if (remainingHours > 0 && overtime2xHours > 0)
                {
                    decimal x2HoursForExempt = Math.Min(overtime2xHours, remainingHours);
                    taxExemptOT += hourlyRate * x2HoursForExempt * 1m;
                }
            }
            else if (overtime2xHours > 0)
            {
                // Nếu không có x3, chỉ tính x2
                decimal x2HoursForExempt = Math.Min(overtime2xHours, maxTaxExemptHours);
                taxExemptOT += hourlyRate * x2HoursForExempt * 1m;
            }
            
            return baseThreshold + FixedThresholdAddon + taxExemptOT + insuranceDeduction;
        }

        private void CalculateSalary(TextBox nameTextBox, TextBox monthTextBox, TextBox yearTextBox, TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox daysOffTextBox,
                                      TextBox overtime2xTextBox, TextBox overtime3xTextBox, TextBox otDays12TextBox, TextBox otDays8TextBox, TextBox overtime15xTextBox, TextBox insuranceTextBox, TextBox taxTextBox,
                                      TextBox attendanceTextBox, TextBox recognizeTextBox, TextBox otherBonusTextBox, TextBox taxThresholdTextBox)
        {
            try
            {
                // Clear previous results before calculating new ones
                try
                {
                    var resultLabels = new[] 
                    { 
                        "empNameLabel", "dayRate8hLabel", "mealDayLabel", "dayRateLabel", "grossLabel", 
                        "insuranceDeductLabel", "taxThresholdResultLabel", "taxDeductLabel", "netLabel", "detailLabel"
                    };
                    foreach (var labelName in resultLabels)
                    {
                        var foundLabels = this.Controls.Find(labelName, true);
                        if (foundLabels.Length > 0 && foundLabels[0] is Label label)
                        {
                            label.Text = "";
                        }
                    }
                }
                catch { }

                // Reset custom tax rate flag to always auto-calculate based on salary brackets
                isCustomTaxRate = false;
                
                // Ensure numeric fields are not empty to avoid parse errors
                EnsureNumericDefaults(salaryTextBox, mealTextBox, workingDaysTextBox, daysOffTextBox, overtime2xTextBox, overtime3xTextBox, otDays12TextBox, otDays8TextBox, overtime15xTextBox, insuranceTextBox, taxTextBox, attendanceTextBox, recognizeTextBox, otherBonusTextBox, taxThresholdTextBox);
                // Validate required info before calculation
                if (string.IsNullOrEmpty(nameTextBox.Text) || string.IsNullOrEmpty(salaryTextBox.Text) || string.IsNullOrEmpty(mealTextBox.Text) ||
                    string.IsNullOrEmpty(workingDaysTextBox.Text) || string.IsNullOrEmpty(attendanceTextBox.Text))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin nhân viên trước khi tính lương!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate tax threshold
                if (string.IsNullOrEmpty(taxThresholdTextBox.Text) || !decimal.TryParse(taxThresholdTextBox.Text.Replace(",", ""), out decimal taxThresholdValue) || taxThresholdValue <= 0)
                {
                    MessageBox.Show("Vui lòng nhập mốc lương tính thuế (phải > 0) trong phần chỉnh sửa thông tin!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Parse inputs
                string employeeName = nameTextBox.Text;
                decimal basicSalary = decimal.Parse(salaryTextBox.Text);
                decimal mealAllowancePerDay = decimal.Parse(mealTextBox.Text);
                decimal workingDays = decimal.Parse(workingDaysTextBox.Text);
                decimal daysOff = decimal.Parse(daysOffTextBox.Text);
                decimal overtime2xHours = decimal.Parse(overtime2xTextBox.Text);  // Làm thêm x2 lương
                decimal overtime3xHours = decimal.Parse(overtime3xTextBox.Text);  // Làm thêm x3 lương
                decimal otDays12 = decimal.Parse(otDays12TextBox.Text);  // Số ngày OT 8/12h
                decimal otDays8 = decimal.Parse(otDays8TextBox.Text);    // Số ngày OT +4h
                decimal overtime15xHours = decimal.Parse(overtime15xTextBox.Text); // Làm thêm x1.5 lương
                decimal attendanceIncentive = decimal.Parse(attendanceTextBox.Text); // Tiền chuyên cần
                int recognizeCount = int.Parse(recognizeTextBox.Text); // Số lượng Recognize
                decimal otherBonus = decimal.Parse(otherBonusTextBox.Text); // Tiền bonus khác
                decimal baseTaxThresholdInput = BaseTaxThreshold;
                if (taxThresholdTextBox != null && decimal.TryParse(taxThresholdTextBox.Text.Replace(",", ""), out decimal userInputThreshold) && userInputThreshold > 0)
                {
                    baseTaxThresholdInput = userInputThreshold;
                }

                // Get editable meal amounts from edit button Tags
                Button editMeal12Btn = this.Controls.Find("editMeal12Btn", true).FirstOrDefault() as Button;
                Button editMeal8Btn = this.Controls.Find("editMeal8Btn", true).FirstOrDefault() as Button;
                decimal meal12Amount = editMeal12Btn != null && decimal.TryParse(editMeal12Btn.Tag.ToString(), out decimal m12) ? m12 : 30000;
                decimal meal8Amount = editMeal8Btn != null && decimal.TryParse(editMeal8Btn.Tag.ToString(), out decimal m8) ? m8 : 20000;

                // Calculate actual working days after deducting days off
                decimal actualWorkingDays = workingDays - daysOff;

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

                // Calculate daily salary components (based on original working days, NOT after days off)
                decimal basicDailySalary = basicSalary / workingDays;
                decimal mealDailySalary = mealAllowancePerDay / workingDays;
                decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;

                // Calculate hourly rate based on BASIC SALARY only (for OT calculation)
                // Round to 3 decimal places for exact calculation
                decimal hourlyRate = Math.Round(basicDailySalary / 8, 3, MidpointRounding.AwayFromZero);

                // Calculate deductions - Bảo hiểm chỉ đóng 10.5% lương cơ bản
                decimal insuranceDeduction = basicSalary * 0.105m;

                // Dynamic tax threshold = base (user input) + 730,000 + phần chênh lệch OT x2/x3 + khấu trừ BH
                decimal taxThreshold = ComputeTaxThreshold(baseTaxThresholdInput, hourlyRate, overtime2xHours, overtime3xHours, insuranceDeduction);

                // Calculate gross salary components:
                decimal regularSalary = actualWorkingDays * dailySalaryForMeal;
                decimal overtime2xSalary = Math.Round(overtime2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal overtime3xSalary = Math.Round(overtime3xHours * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                decimal overtime15xSalary = Math.Round(overtime15xHours * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                // Calculate Incentive
                decimal totalIncentive = attendanceIncentive + (recognizeCount * 50000) + otherBonus;

                // Lương Brutto bao gồm tiền ăn bonus và incentive
                decimal grossSalary = regularSalary + overtime2xSalary + overtime3xSalary + overtime15xSalary + bonusMealAllowance + totalIncentive;

                // Calculate net salary before tax
                decimal netSalaryBeforeTax = grossSalary - insuranceDeduction;

                // Tax logic - Tính thuế dựa trên chênh lệch giữa Lương Brutto và Mốc thuế
                decimal taxBase = grossSalary - taxThreshold;
                decimal taxRate = 0;
                
                // Nếu người dùng chưa nhập % thủ công, tự động tính theo mốc lương
                if (!isCustomTaxRate)
                {
                    if (taxBase <= 0)
                    {
                        taxRate = 0;
                    }
                    else if (taxBase > 0 && taxBase <= 10000000)
                    {
                        taxRate = 0.05m;
                    }
                    else if (taxBase > 10000000 && taxBase <= 30000000)
                    {
                        taxRate = 0.10m;
                    }
                    else if (taxBase > 30000000 && taxBase <= 60000000)
                    {
                        taxRate = 0.20m;
                    }
                    else if (taxBase > 60000000 && taxBase <= 100000000)
                    {
                        taxRate = 0.30m;
                    }
                    else if (taxBase > 100000000)
                    {
                        taxRate = 0.35m;
                    }
                    // Update taxTextBox to show correct % (always integer, no decimal)
                    taxTextBox.Text = ((int)(taxRate * 100)).ToString();
                }
                else
                {
                    // Người dùng đã nhập % thủ công, dùng % từ taxTextBox
                    if (decimal.TryParse(taxTextBox.Text, out decimal customTaxPercent))
                    {
                        taxRate = customTaxPercent / 100;
                    }
                }

                // Round tax down (Floor) for calculation, but round up for display
                decimal taxDeduction = taxBase > 0 ? Math.Floor(taxBase * taxRate) : 0;
                decimal taxDeductionDisplay = taxBase > 0 ? Math.Round(taxBase * taxRate, 0, MidpointRounding.AwayFromZero) : 0;

                // Calculate net salary = Gross - Insurance - Tax
                decimal netSalary = grossSalary - insuranceDeduction - taxDeduction;

                // Save calculation to user data
                int month = int.Parse(monthTextBox.Text);
                int year = int.Parse(yearTextBox.Text);
                userDataManager.UpdateLastCalculation(currentUsername, month, year, netSalary);

                // Update OT result labels
                Label overtime2xResultLabel = this.Controls.Find("overtime2xResultLabel", true)[0] as Label;
                Label overtime3xResultLabel = this.Controls.Find("overtime3xResultLabel", true)[0] as Label;
                Label overtime15xResultLabel = this.Controls.Find("overtime15xResultLabel", true)[0] as Label;
                overtime2xResultLabel.Text = $"→ {overtime2xSalary:C0} VND";
                overtime3xResultLabel.Text = $"→ {overtime3xSalary:C0} VND";
                overtime15xResultLabel.Text = $"→ {overtime15xSalary:C0} VND";

                // Get label references for typing effect
                Label empNameLabel = this.Controls.Find("empNameLabel", true)[0] as Label;
                Label grossLabel = this.Controls.Find("grossLabel", true)[0] as Label;
                Label insuranceDeductLabel = this.Controls.Find("insuranceDeductLabel", true)[0] as Label;
                Label taxDeductLabel = this.Controls.Find("taxDeductLabel", true)[0] as Label;
                Label taxThresholdResultLabel = this.Controls.Find("taxThresholdResultLabel", true)[0] as Label;
                Label netLabel = this.Controls.Find("netLabel", true)[0] as Label;
                Label detailLabel = this.Controls.Find("detailLabel", true)[0] as Label;
                Label dayRate8hLabel = this.Controls.Find("dayRate8hLabel", true)[0] as Label;
                Label mealDayLabel = this.Controls.Find("mealDayLabel", true)[0] as Label;
                Label dayRateLabel = this.Controls.Find("dayRateLabel", true)[0] as Label;

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
                    $"  • OT x3 ({overtime3xHours:F1} tiếng × {hourlyRate:C0} × 3): {overtime3xSalary:C0} VND\n" +
                    $"  • OT x1.5 ({overtime15xHours:F1} tiếng × {hourlyRate:C0} × 1.5): {overtime15xSalary:C0} VND{bonusInfo}{incentiveInfo}";
                
                // Apply typing effect to result labels with staggered timing
                var typingTimer = new System.Windows.Forms.Timer();
                var labels = new[] { empNameLabel, dayRate8hLabel, mealDayLabel, dayRateLabel, grossLabel, insuranceDeductLabel, taxThresholdResultLabel, taxDeductLabel, netLabel };
                var labelTexts = new[] 
                { 
                    $"Nhân Viên: {employeeName}",
                    $"Lương cơ bản 1 ngày: {basicDailySalary:C0} VND",
                    $"Tiền ăn 1 ngày: {mealDailySalary:C0} VND",
                    $"Tổng lương 1 ngày công: {dailySalaryForMeal:C0} VND",
                    $"Lương Brutto: {grossSalary:C0} VND",
                    $"Khấu Trừ Bảo Hiểm (10.5% lương cơ bản): {insuranceDeduction:C0} VND",
                    $"Mốc thuế áp dụng: {taxThreshold:C0} VND",
                    $"Khấu Trừ Thuế: {(taxDeductionDisplay > 0 ? taxDeductionDisplay.ToString("C0") + " VND" : "0 VND")}",
                    $"Lương Net (Thực Nhận): {netSalary:C0} VND"
                };
                
                int labelIndex = 0;
                typingTimer.Interval = 300; // Stagger each label by 300ms
                typingTimer.Tick += (s, e) =>
                {
                    if (labelIndex < labels.Length)
                    {
                        AnimateTypingEffect(labels[labelIndex], labelTexts[labelIndex], 10);
                        labelIndex++;
                    }
                    else
                    {
                        typingTimer.Stop();
                        try { typingTimer.Dispose(); } catch { }
                        
                        // Display detail label all at once after main labels complete
                        // Use a new timer to wait for all main labels typing to complete
                        var detailDisplayTimer = new System.Windows.Forms.Timer();
                        detailDisplayTimer.Interval = 50;
                        int detailWaitCounter = 0;
                        
                        detailDisplayTimer.Tick += (ts, te) =>
                        {
                            // Wait for main labels typing to complete
                            detailWaitCounter++;
                            // Last label triggered at (labels.Length-1) * 6 intervals, plus ~10 intervals for typing = ~50 total
                            int estimatedDuration = (labels.Length - 1) * 6 + 10;
                            
                            if (detailWaitCounter >= estimatedDuration)
                            {
                                detailDisplayTimer.Stop();
                                try { detailDisplayTimer.Dispose(); } catch { }
                                
                                // Display detail label all at once (no typing effect)
                                detailLabel.Text = detail;
                                
                                // Play applause sound when net salary exceeds 15 million
                                if (netSalary > 15000000)
                                {
                                    try
                                    {
                                        PlayApplauseEmbedded();
                                    }
                                    catch { }
                                }
                            }
                        };
                        detailDisplayTimer.Start();
                    }
                };
                typingTimer.Start();
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

        // Typing effect animation for label text
        private void AnimateTypingEffect(Label label, string fullText, int delayPerChar = 15)
        {
            label.Text = "";
            var typingTimer = new System.Windows.Forms.Timer();
            int charIndex = 0;
            
            typingTimer.Interval = delayPerChar;
            typingTimer.Tick += (s, e) =>
            {
                if (charIndex < fullText.Length)
                {
                    label.Text += fullText[charIndex];
                    charIndex++;
                }
                else
                {
                    typingTimer.Stop();
                    try { typingTimer.Dispose(); } catch { }
                }
            };
            typingTimer.Start();
        }

        // Compute net salary using the same logic as CalculateSalary but return the net value (no UI changes)
        private decimal ComputeNetSalary(TextBox nameTextBox, TextBox monthTextBox, TextBox yearTextBox, TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox daysOffTextBox,
                                      TextBox overtime2xTextBox, TextBox overtime3xTextBox, TextBox otDays12TextBox, TextBox otDays8TextBox, TextBox overtime15xTextBox, TextBox insuranceTextBox, TextBox taxTextBox,
                                      TextBox attendanceTextBox, TextBox recognizeTextBox, TextBox otherBonusTextBox, TextBox taxThresholdTextBox)
        {
            try
            {
                decimal basicSalary = decimal.Parse(salaryTextBox.Text);
                decimal mealAllowancePerDay = decimal.Parse(mealTextBox.Text);
                decimal workingDays = decimal.Parse(workingDaysTextBox.Text);
                decimal daysOff = decimal.Parse(daysOffTextBox.Text);
                decimal overtime2xHours = decimal.Parse(overtime2xTextBox.Text);
                decimal overtime3xHours = decimal.Parse(overtime3xTextBox.Text);
                decimal otDays12 = decimal.Parse(otDays12TextBox.Text);
                decimal otDays8 = decimal.Parse(otDays8TextBox.Text);
                decimal overtime15xHours = decimal.Parse(overtime15xTextBox.Text);
                decimal attendanceIncentive = decimal.Parse(attendanceTextBox.Text);
                int recognizeCount = int.Parse(recognizeTextBox.Text);
                decimal otherBonus = decimal.Parse(otherBonusTextBox.Text);
                decimal baseTaxThresholdInput = BaseTaxThreshold;
                if (taxThresholdTextBox != null && decimal.TryParse(taxThresholdTextBox.Text.Replace(",", ""), out decimal userInputThreshold) && userInputThreshold > 0)
                {
                    baseTaxThresholdInput = userInputThreshold;
                }

                Button editMeal12Btn = this.Controls.Find("editMeal12Btn", true).FirstOrDefault() as Button;
                Button editMeal8Btn = this.Controls.Find("editMeal8Btn", true).FirstOrDefault() as Button;
                decimal meal12Amount = editMeal12Btn != null && decimal.TryParse(editMeal12Btn.Tag.ToString(), out decimal m12) ? m12 : 30000;
                decimal meal8Amount = editMeal8Btn != null && decimal.TryParse(editMeal8Btn.Tag.ToString(), out decimal m8) ? m8 : 20000;

                decimal actualWorkingDays = workingDays - daysOff;
                decimal bonusMealAllowance = 0;
                if (otDays12 > 0) bonusMealAllowance += otDays12 * meal12Amount;
                if (otDays8 > 0) bonusMealAllowance += otDays8 * meal8Amount;

                decimal basicDailySalary = basicSalary / workingDays;
                decimal mealDailySalary = mealAllowancePerDay / workingDays;
                decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;
                // Round to 3 decimal places for exact calculation
                decimal hourlyRate = Math.Round(basicDailySalary / 8, 3, MidpointRounding.AwayFromZero);

                decimal insuranceDeduction = basicSalary * 0.105m;
                decimal taxThreshold = ComputeTaxThreshold(baseTaxThresholdInput, hourlyRate, overtime2xHours, overtime3xHours, insuranceDeduction);

                decimal regularSalary = actualWorkingDays * dailySalaryForMeal;
                decimal overtime2xSalary = Math.Round(overtime2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal overtime3xSalary = Math.Round(overtime3xHours * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                decimal overtime15xSalary = Math.Round(overtime15xHours * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                decimal totalIncentive = attendanceIncentive + (recognizeCount * 50000) + otherBonus;
                decimal grossSalary = regularSalary + overtime2xSalary + overtime3xSalary + overtime15xSalary + bonusMealAllowance + totalIncentive;
                decimal netSalaryBeforeTax = grossSalary - insuranceDeduction;

                // Tax logic - Tính thuế dựa trên chênh lệch giữa Lương Brutto và Mốc thuế
                decimal taxBase = grossSalary - taxThreshold;
                decimal taxRate = 0;
                if (taxBase <= 0) taxRate = 0;
                else if (taxBase > 0 && taxBase <= 10000000) taxRate = 0.05m;
                else if (taxBase > 10000000 && taxBase <= 30000000) taxRate = 0.10m;
                else if (taxBase > 30000000 && taxBase <= 60000000) taxRate = 0.20m;
                else if (taxBase > 60000000 && taxBase <= 100000000) taxRate = 0.30m;
                else taxRate = 0.30m;

                decimal taxDeduction = taxBase > 0 ? Math.Floor(taxBase * taxRate) : 0;
                decimal netSalary = grossSalary - insuranceDeduction - taxDeduction;
                return netSalary;
            }
            catch
            {
                return 0m;
            }
        }

        // Ensure textboxes that should contain numeric values are not empty
        private void EnsureNumericDefaults(params TextBox[] boxes)
        {
            if (boxes == null) return;
            foreach (var tb in boxes)
            {
                try
                {
                    if (tb == null) continue;
                    if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "0";
                }
                catch { }
            }
        }

        


        // Play embedded applause.wav (or fallback to external Assets/audio/applause.wav)
        private void PlayApplauseEmbedded()
        {
            try
            {
                bool played = false;
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceCandidates = new[]
                {
                    asm.GetName().Name + ".Assets.audio.clap.wav",   // new file
                    asm.GetName().Name + ".Assets.audio.applause.wav" // legacy name
                };

                foreach (var resName in resourceCandidates)
                {
                    using (var rs = asm.GetManifestResourceStream(resName))
                    {
                        if (rs == null) continue;
                        try
                        {
                            var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "applause_" + System.Guid.NewGuid().ToString() + ".wav");
                            using (var fs = System.IO.File.Create(tmp)) { rs.CopyTo(fs); }
                            try
                            {
                                var sp = new System.Media.SoundPlayer(tmp);
                                sp.Play();
                                System.Threading.Tasks.Task.Run(async () => { await System.Threading.Tasks.Task.Delay(12000); try { System.IO.File.Delete(tmp); } catch { } });
                                played = true;
                                break;
                            }
                            catch { try { System.IO.File.Delete(tmp); } catch { } played = false; }
                        }
                        catch { played = false; }
                    }
                    if (played) break;
                }

                if (!played)
                {
                    var appDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fileCandidates = new[]
                    {
                        System.IO.Path.Combine(appDir, "Assets", "audio", "clap.wav"),
                        System.IO.Path.Combine(appDir, "Assets", "audio", "applause.wav")
                    };

                    foreach (var path in fileCandidates)
                    {
                        try
                        {
                            if (System.IO.File.Exists(path))
                            {
                                using (var sp = new System.Media.SoundPlayer(path)) { sp.Play(); }
                                played = true;
                                break;
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

    }
}
