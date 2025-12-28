using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using SalaryCalculator;

namespace SalaryCalculator
{
    public partial class SalaryCalculatorForm : Form
    {
            // Đúng vị trí bên trong class
            private string currentUsername;
            private UserDataManager userDataManager = new UserDataManager();

        public SalaryCalculatorForm(string username = "")
        {
            currentUsername = username;
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
                rankingGrid.BorderStyle = BorderStyle.FixedSingle;
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
                rankingGrid.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
                rankingGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
                rankingGrid.EnableHeadersVisualStyles = false;
                rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightSteelBlue;
                rankingGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.RowsDefaultCellStyle.BackColor = System.Drawing.Color.White;
                rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.AliceBlue;
                rankingGrid.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
                rankingGrid.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
                // Tắt chức năng sort khi click vào tiêu đề
                foreach (DataGridViewColumn col in rankingGrid.Columns)
                {
                    col.SortMode = DataGridViewColumnSortMode.NotSortable;
                    if (col.Index == 0)
                    {
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    }
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
