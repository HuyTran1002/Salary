using System;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace SalaryCalculator
{
    public partial class LoginForm : Form
    {
        private UserDataManager userDataManager = new UserDataManager();
        private bool isRegistering = false;
        private static bool hasCheckedUpdateThisSession = false;
        private static bool hasShownUpdateDialogThisSession = false;
        private bool isCheckingForUpdate = false;

        public LoginForm()
        {
            InitializeComponent();
            this.Shown += (s, e) => CheckForUpdate();
        }

        private async void CheckForUpdate()
        {
            if (hasCheckedUpdateThisSession || isCheckingForUpdate)
            {
                return;
            }

            isCheckingForUpdate = true;
            hasCheckedUpdateThisSession = true;

            var result = await UpdateChecker.CheckForUpdateAsync();
            if (result.hasUpdate && !hasShownUpdateDialogThisSession)
            {
                string exeUrl = await UpdateChecker.GetLatestExeDownloadUrlAsync();
                UpdateChecker.ShowManualUpdateDialog(result.latestVersion, exeUrl);
                hasShownUpdateDialogThisSession = true;
            }

            isCheckingForUpdate = false;
        }

        private void InitializeComponent()
        {
            this.Text = "💼 Tính Lương Nhân Viên - Đăng Nhập";
            this.Width = 520;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Font = new Font("Arial", 10);

            int formInputsY = 20;
            int formWidth = this.Width;
            int contentWidth = 420;
            int contentStartX = (formWidth - contentWidth) / 2 - 8;

            // Declare all controls first
            Label titleLabel = new Label();
            Label subtitleLabel = new Label();
            Label usernameLabel = new Label();
            TextBox usernameTextBox = new TextBox();
            Label fullNameLabel = new Label();
            TextBox fullNameTextBox = new TextBox();
            Label salaryLabel = new Label();
            TextBox salaryTextBox = new TextBox();
            Label mealLabel = new Label();
            TextBox mealTextBox = new TextBox();
            Label attendancePerDayLabel = new Label();
            TextBox attendancePerDayTextBox = new TextBox();
            Label travelPerDayLabel = new Label();
            TextBox travelPerDayTextBox = new TextBox();
            Label housingAllowanceLabel = new Label();
            TextBox housingAllowanceTextBox = new TextBox();
            Label phoneLabel = new Label();
            TextBox phoneTextBox = new TextBox();
            Label ageLabel = new Label();
            TextBox ageTextBox = new TextBox();
            Button loginBtn = new Button();
            Button toggleBtn = new Button();
            Label taxThresholdLabel = new Label();
            TextBox taxThresholdTextBox = new TextBox();

            // Title
            titleLabel.Text = "💼 TÍNH LƯƠNG NHÂN VIÊN";
            titleLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.DarkBlue;
            titleLabel.Location = new Point(contentStartX, formInputsY);
            titleLabel.Width = contentWidth;
            titleLabel.Height = 32;
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(titleLabel);

            formInputsY += 40;

            // Subtitle
            subtitleLabel.Text = "Vui lòng đăng nhập hoặc đăng ký";
            subtitleLabel.Font = new Font("Arial", 9);
            subtitleLabel.Location = new Point(contentStartX, formInputsY);
            subtitleLabel.Width = contentWidth;
            subtitleLabel.Height = 18;
            subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(subtitleLabel);

            formInputsY += 25;

            // Declare register-only bonus variables for use in KeyDown handler
            TextBox certTextBox = new TextBox();

            // Username
            usernameLabel.Text = "Tên đăng nhập:";
            usernameLabel.Location = new Point(contentStartX, formInputsY);
            usernameLabel.Width = 150;
            usernameLabel.Height = 18;
            this.Controls.Add(usernameLabel);

            formInputsY += 22;

            usernameTextBox.Location = new Point(contentStartX, formInputsY);
            usernameTextBox.Width = contentWidth;
            usernameTextBox.Height = 24;
            usernameTextBox.Name = "usernameTextBox";
            usernameTextBox.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) {
                    HandleLogin(usernameTextBox.Text, fullNameTextBox.Text, phoneTextBox.Text, ageTextBox.Text, salaryTextBox.Text, mealTextBox.Text, certTextBox.Text, attendancePerDayTextBox.Text, travelPerDayTextBox.Text, housingAllowanceTextBox.Text);
                }
            };
            this.Controls.Add(usernameTextBox);

            formInputsY += 30;

            // Full Name (hidden by default)
            fullNameLabel.Text = "Tên đầy đủ:";
            fullNameLabel.Location = new Point(contentStartX, formInputsY);
            fullNameLabel.Width = 150;
            fullNameLabel.Height = 18;
            fullNameLabel.Name = "fullNameLabel";
            fullNameLabel.Visible = false;
            this.Controls.Add(fullNameLabel);

            fullNameTextBox.Location = new Point(contentStartX, formInputsY + 22);
            fullNameTextBox.Width = contentWidth;
            fullNameTextBox.Height = 24;
            fullNameTextBox.Name = "fullNameTextBox";
            fullNameTextBox.Visible = false;
            this.Controls.Add(fullNameTextBox);

            // Basic Salary (hidden by default)
            salaryLabel.Text = "Lương cơ bản:";
            salaryLabel.Location = new Point(contentStartX, formInputsY + 52);
            salaryLabel.Width = 150;
            salaryLabel.Height = 18;
            salaryLabel.Name = "salaryLabel";
            salaryLabel.Visible = false;
            this.Controls.Add(salaryLabel);

            salaryTextBox.Location = new Point(contentStartX, formInputsY + 74);
            salaryTextBox.Width = contentWidth;
            salaryTextBox.Height = 24;
            salaryTextBox.Name = "salaryTextBox";
            salaryTextBox.Visible = false;
            NumberFormatter.FormatNumberInput(salaryTextBox);
            this.Controls.Add(salaryTextBox);

            // Meal Allowance (hidden by default)
            mealLabel.Text = "Tiền ăn/Tháng:";
            mealLabel.Location = new Point(contentStartX, formInputsY + 104);
            mealLabel.Width = 150;
            mealLabel.Height = 18;
            mealLabel.Name = "mealLabel";
            mealLabel.Visible = false;
            this.Controls.Add(mealLabel);

            mealTextBox.Location = new Point(contentStartX, formInputsY + 126);
            mealTextBox.Width = contentWidth;
            mealTextBox.Height = 24;
            mealTextBox.Name = "mealTextBox";
            mealTextBox.Visible = false;
            NumberFormatter.FormatNumberInput(mealTextBox);
            this.Controls.Add(mealTextBox);

            // Attendance per month (hidden by default)
            attendancePerDayLabel.Text = "Tiền chuyên cần/tháng:";
            attendancePerDayLabel.Location = new Point(contentStartX, formInputsY + 156);
            attendancePerDayLabel.Width = 180;
            attendancePerDayLabel.Height = 18;
            attendancePerDayLabel.Name = "attendancePerDayLabel";
            attendancePerDayLabel.Visible = false;
            this.Controls.Add(attendancePerDayLabel);

            attendancePerDayTextBox.Location = new Point(contentStartX, formInputsY + 178);
            attendancePerDayTextBox.Width = contentWidth;
            attendancePerDayTextBox.Height = 24;
            attendancePerDayTextBox.Name = "attendancePerDayTextBox";
            attendancePerDayTextBox.Visible = false;
            attendancePerDayTextBox.Text = "195500";
            NumberFormatter.FormatNumberInput(attendancePerDayTextBox);
            this.Controls.Add(attendancePerDayTextBox);

            // Travel per month (hidden by default)
            travelPerDayLabel.Text = "Tiền đi lại/tháng:";
            travelPerDayLabel.Location = new Point(contentStartX, formInputsY + 208);
            travelPerDayLabel.Width = 180;
            travelPerDayLabel.Height = 18;
            travelPerDayLabel.Name = "travelPerDayLabel";
            travelPerDayLabel.Visible = false;
            this.Controls.Add(travelPerDayLabel);

            travelPerDayTextBox.Location = new Point(contentStartX, formInputsY + 230);
            travelPerDayTextBox.Width = contentWidth;
            travelPerDayTextBox.Height = 24;
            travelPerDayTextBox.Name = "travelPerDayTextBox";
            travelPerDayTextBox.Visible = false;
            travelPerDayTextBox.Text = "195500";
            NumberFormatter.FormatNumberInput(travelPerDayTextBox);
            this.Controls.Add(travelPerDayTextBox);

            // Housing allowance (hidden by default)
            housingAllowanceLabel.Text = "Tiền nhà ở:";
            housingAllowanceLabel.Location = new Point(contentStartX, formInputsY + 260);
            housingAllowanceLabel.Width = 180;
            housingAllowanceLabel.Height = 18;
            housingAllowanceLabel.Name = "housingAllowanceLabel";
            housingAllowanceLabel.Visible = false;
            this.Controls.Add(housingAllowanceLabel);

            housingAllowanceTextBox.Location = new Point(contentStartX, formInputsY + 282);
            housingAllowanceTextBox.Width = contentWidth;
            housingAllowanceTextBox.Height = 24;
            housingAllowanceTextBox.Name = "housingAllowanceTextBox";
            housingAllowanceTextBox.Visible = false;
            housingAllowanceTextBox.Text = "100000";
            NumberFormatter.FormatNumberInput(housingAllowanceTextBox);
            this.Controls.Add(housingAllowanceTextBox);

            // Certificate Bonus (hidden by default)
            Label certLabel = new Label();
            certLabel.Text = "Tiền chứng chỉ:";
            certLabel.Location = new Point(contentStartX, formInputsY + 312);
            certLabel.Width = 150;
            certLabel.Height = 18;
            certLabel.Name = "certLabel";
            certLabel.Visible = false;
            this.Controls.Add(certLabel);

            certTextBox.Location = new Point(contentStartX, formInputsY + 334);
            certTextBox.Width = contentWidth;
            certTextBox.Height = 24;
            certTextBox.Name = "certTextBox";
            certTextBox.Visible = false;
            NumberFormatter.FormatNumberInput(certTextBox);
            this.Controls.Add(certTextBox);

            // Phone/Zalo (hidden by default)
            phoneLabel.Text = "Số điện thoại/Zalo:";
            phoneLabel.Location = new Point(contentStartX, formInputsY + 364);
            phoneLabel.Width = 150;
            phoneLabel.Height = 18;
            phoneLabel.Name = "phoneLabel";
            phoneLabel.Visible = false;
            this.Controls.Add(phoneLabel);

            phoneTextBox.Location = new Point(contentStartX, formInputsY + 386);
            phoneTextBox.Width = contentWidth;
            phoneTextBox.Height = 24;
            phoneTextBox.Name = "phoneTextBox";
            phoneTextBox.Visible = false;
            this.Controls.Add(phoneTextBox);

            // Age (hidden by default)
            ageLabel.Text = "Tuổi:";
            ageLabel.Location = new Point(contentStartX, formInputsY + 416);
            ageLabel.Width = 150;
            ageLabel.Height = 18;
            ageLabel.Name = "ageLabel";
            ageLabel.Visible = false;
            this.Controls.Add(ageLabel);

            ageTextBox.Location = new Point(contentStartX, formInputsY + 438);
            ageTextBox.Width = contentWidth;
            ageTextBox.Height = 24;
            ageTextBox.Name = "ageTextBox";
            ageTextBox.Visible = false;
            NumberFormatter.FormatNumberInput(ageTextBox);
            this.Controls.Add(ageTextBox);

            // Tax Threshold (hidden by default)
            taxThresholdLabel.Text = "Mốc lương tính thuế:";
            taxThresholdLabel.Location = new Point(contentStartX, formInputsY + 468);
            taxThresholdLabel.Width = 150;
            taxThresholdLabel.Height = 18;
            taxThresholdLabel.Name = "taxThresholdLabel";
            taxThresholdLabel.Visible = false;
            this.Controls.Add(taxThresholdLabel);

            taxThresholdTextBox.Location = new Point(contentStartX, formInputsY + 490);
            taxThresholdTextBox.Width = contentWidth;
            taxThresholdTextBox.Height = 24;
            taxThresholdTextBox.Name = "taxThresholdTextBox";
            taxThresholdTextBox.Visible = false;
            taxThresholdTextBox.Text = "";
            taxThresholdTextBox.ReadOnly = false;
            taxThresholdTextBox.BackColor = System.Drawing.Color.White;
            NumberFormatter.FormatNumberInput(taxThresholdTextBox);
            this.Controls.Add(taxThresholdTextBox);

            // Login Button - positioned at bottom of form (fixed position)
            // Center action buttons as a group
            int actionYLogin = 170;
            int calcWidth = 196; // login width
            int toggleWidth = 236;
            int actionGap = 12;
            int totalActionWidth = calcWidth + actionGap + toggleWidth;
            int actionStartXLogin = (formWidth - totalActionWidth) / 2 - 8;

            loginBtn.Text = "🔐 Đăng Nhập";
            loginBtn.Location = new Point(actionStartXLogin, actionYLogin);
            loginBtn.Width = calcWidth;
            loginBtn.Height = 36;
            loginBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            loginBtn.BackColor = Color.FromArgb(255, 90, 0);
            loginBtn.ForeColor = Color.White;
            loginBtn.TextAlign = ContentAlignment.MiddleCenter;
            loginBtn.Name = "loginBtn";
            loginBtn.Click += (s, e) => {
                HandleLogin(usernameTextBox.Text, fullNameTextBox.Text, phoneTextBox.Text, ageTextBox.Text, salaryTextBox.Text, mealTextBox.Text, certTextBox.Text, attendancePerDayTextBox.Text, travelPerDayTextBox.Text, housingAllowanceTextBox.Text);
            };
            this.Controls.Add(loginBtn);

            // Register Toggle Button - positioned at bottom of form (fixed position)
            toggleBtn.Text = "📝 Chuyển sang Đăng Ký";
            toggleBtn.Location = new Point(actionStartXLogin + calcWidth + actionGap, actionYLogin);
            toggleBtn.Width = toggleWidth;
            toggleBtn.Height = 36;
            toggleBtn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            toggleBtn.BackColor = Color.DodgerBlue;
            toggleBtn.ForeColor = Color.White;
            toggleBtn.TextAlign = ContentAlignment.MiddleCenter;
            toggleBtn.Name = "toggleBtn";
            toggleBtn.Click += (s, e) => {
                ToggleRegisterMode(usernameTextBox, fullNameTextBox, phoneTextBox, ageTextBox, salaryTextBox, mealTextBox, certTextBox, attendancePerDayTextBox, travelPerDayTextBox, housingAllowanceTextBox,
                    fullNameLabel, phoneLabel, ageLabel, salaryLabel, mealLabel, certLabel, attendancePerDayLabel, travelPerDayLabel, housingAllowanceLabel, loginBtn, toggleBtn);
                // Show/hide tax threshold controls theo isRegistering
                taxThresholdLabel.Visible = isRegistering;
                taxThresholdTextBox.Visible = isRegistering;
            };
            this.Controls.Add(toggleBtn);
            // Apply infinity glass theme tweaks
            try { Theme.ApplyInfinityGlassTheme(this); } catch { }
        }

        private void ToggleRegisterMode(TextBox usernameTextBox, TextBox fullNameTextBox, TextBox phoneTextBox, TextBox ageTextBox, TextBox salaryTextBox,
             TextBox mealTextBox, TextBox certTextBox, TextBox attendancePerDayTextBox, TextBox travelPerDayTextBox, TextBox housingAllowanceTextBox,
             Label fullNameLabel, Label phoneLabel, Label ageLabel, Label salaryLabel, Label mealLabel, Label certLabel, Label attendancePerDayLabel, Label travelPerDayLabel, Label housingAllowanceLabel,
               Button loginBtn, Button toggleBtn)
        {
            isRegistering = !isRegistering;

            fullNameLabel.Visible = isRegistering;
            fullNameTextBox.Visible = isRegistering;
            phoneLabel.Visible = isRegistering;
            phoneTextBox.Visible = isRegistering;
            ageLabel.Visible = isRegistering;
            ageTextBox.Visible = isRegistering;
            salaryLabel.Visible = isRegistering;
            salaryTextBox.Visible = isRegistering;
            mealLabel.Visible = isRegistering;
            mealTextBox.Visible = isRegistering;
            attendancePerDayLabel.Visible = isRegistering;
            attendancePerDayTextBox.Visible = isRegistering;
            travelPerDayLabel.Visible = isRegistering;
            travelPerDayTextBox.Visible = isRegistering;
            housingAllowanceLabel.Visible = isRegistering;
            housingAllowanceTextBox.Visible = isRegistering;
            certLabel.Visible = isRegistering;
            certTextBox.Visible = isRegistering;
            // taxThresholdLabel.Visible và taxThresholdTextBox.Visible được điều khiển ở ngoài toggleBtn.Click

            if (isRegistering)
            {
                this.Height = 760;
                loginBtn.Text = "✔️ Đăng Ký";
                toggleBtn.Text = "🔐 Quay lại Đăng Nhập";
                loginBtn.BackColor = Color.FromArgb(255, 90, 0);
                int formWidthLocal = this.Width;
                int calcWidthLocal = 196;
                int toggleWidthLocal = 236;
                int actionGapLocal = 12;
                int totalActionWidthLocal = calcWidthLocal + actionGapLocal + toggleWidthLocal;
                int actionStartXRegister = (formWidthLocal - totalActionWidthLocal) / 2 - 8;
                int actionYRegister = 680;
                loginBtn.Width = calcWidthLocal;
                toggleBtn.Width = toggleWidthLocal;
                loginBtn.Height = 36;
                toggleBtn.Height = 36;
                loginBtn.Location = new Point(actionStartXRegister, actionYRegister);
                toggleBtn.Location = new Point(actionStartXRegister + calcWidthLocal + actionGapLocal, actionYRegister);
                this.CenterToScreen();
            }
            else
            {
                this.Height = 300;
                loginBtn.Text = "🔐 Đăng Nhập";
                toggleBtn.Text = "📝 Chuyển sang Đăng Ký";
                loginBtn.BackColor = Color.FromArgb(255, 90, 0);
                int formWidthLocal = this.Width;
                int calcWidthLocal = 196;
                int toggleWidthLocal = 236;
                int actionGapLocal = 12;
                int totalActionWidthLocal = calcWidthLocal + actionGapLocal + toggleWidthLocal;
                int actionStartXLoginLocal = (formWidthLocal - totalActionWidthLocal) / 2 - 8;
                int actionYLoginLocal = 170;
                loginBtn.Width = calcWidthLocal;
                toggleBtn.Width = toggleWidthLocal;
                loginBtn.Height = 36;
                toggleBtn.Height = 36;
                loginBtn.Location = new Point(actionStartXLoginLocal, actionYLoginLocal);
                toggleBtn.Location = new Point(actionStartXLoginLocal + calcWidthLocal + actionGapLocal, actionYLoginLocal);
                this.CenterToScreen();
                fullNameTextBox.Clear();
                phoneTextBox.Clear();
                ageTextBox.Clear();
                salaryTextBox.Clear();
                mealTextBox.Clear();
                certTextBox.Clear();
                attendancePerDayTextBox.Text = "195500";
                travelPerDayTextBox.Text = "195500";
                housingAllowanceTextBox.Text = "100000";
            }
        }

        private void HandleLogin(string username, string fullName, string phone, string age, string salary, string meal, string cert, string attendancePerDay, string travelPerDay, string housingAllowance)
        {
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (isRegistering)
            {
                // Register mode
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(age))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Tên, SĐT, Tuổi)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Default empty fields to "0"
                if (string.IsNullOrWhiteSpace(salary)) salary = "0";
                if (string.IsNullOrWhiteSpace(meal)) meal = "0";
                if (string.IsNullOrWhiteSpace(cert)) cert = "0";
                if (string.IsNullOrWhiteSpace(attendancePerDay)) attendancePerDay = "195500";
                if (string.IsNullOrWhiteSpace(travelPerDay)) travelPerDay = "195500";
                if (string.IsNullOrWhiteSpace(housingAllowance)) housingAllowance = "100000";

                // Validate tax threshold
                decimal taxThreshold = 0;
                Control[] taxThresholdFound = this.Controls.Find("taxThresholdTextBox", true);
                if (taxThresholdFound.Length > 0 && taxThresholdFound[0] is TextBox taxThresholdTextBox)
                {
                    string taxText = taxThresholdTextBox.Text.Replace(",", "");
                    if (string.IsNullOrWhiteSpace(taxText)) taxText = "0";
                    decimal.TryParse(taxText, out taxThreshold);
                }
                if (taxThreshold <= 0)
                {
                    MessageBox.Show("Vui lòng nhập mốc lương tính thuế (phải > 0)!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate phone number: must be exactly 10 digits
                string phoneDigits = new string(phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length != 10)
                {
                    MessageBox.Show("Vui lòng điền đúng số điện thoại đang sử dụng (10 số)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(age, out int userAge) || !decimal.TryParse(salary, out decimal basicSalary) ||
                    !decimal.TryParse(meal, out decimal mealAllowance) ||
                    !decimal.TryParse(cert, out decimal certBonus) ||
                    !decimal.TryParse(attendancePerDay, out decimal attendancePerDayValue) ||
                    !decimal.TryParse(travelPerDay, out decimal travelPerDayValue) ||
                    !decimal.TryParse(housingAllowance, out decimal housingAllowanceValue))
                {
                    MessageBox.Show("Tuổi phải là số, Lương, tiền ăn, tiền chứng chỉ, chuyên cần/tháng, đi lại/tháng và tiền nhà ở phải là số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (attendancePerDayValue < 0 || travelPerDayValue < 0 || housingAllowanceValue < 0)
                {
                    MessageBox.Show("Tiền chuyên cần/tháng, tiền đi lại/tháng và tiền nhà ở không được âm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!userDataManager.IsNewUser(username))
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int taxThresholdInt = (int)taxThreshold;
                if (userDataManager.Register(username, fullName, phone, userAge, basicSalary, mealAllowance, 0, 0, 0, taxThreshold, "", certBonus, attendancePerDayValue, travelPerDayValue, housingAllowanceValue))
                {
                    // Bỏ popup chào mừng, vào thẳng form tính lương
                    OpenCalculatorForm(username);
                }
                else
                {
                    MessageBox.Show("Có lỗi xảy ra khi đăng ký!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Login mode
                // Cho phép admin đăng nhập như user thường để xem bảng xếp hạng
                if (username == "admin")
                {
                    // Bỏ popup đăng nhập thành công, vào thẳng form tính lương
                    OpenCalculatorForm("admin");
                    return;
                }
                var user = userDataManager.Login(username);
                if (user != null)
                {
                    // Bỏ popup đăng nhập thành công, vào thẳng form tính lương
                    OpenCalculatorForm(username);
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập không tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenCalculatorForm(string username)
        {
            SalaryCalculatorForm calculatorForm = new SalaryCalculatorForm(username);
            // Ẩn Login và hiển thị form tính lương không modal
            this.Hide();
            // Khi form tính lương đóng (bấm X hoặc Đăng xuất) thì hiện lại LoginForm
            calculatorForm.FormClosed += (s, e) =>
            {
                // Luôn quay về chế độ Đăng Nhập (không phải đăng ký)
                isRegistering = false;
                ForceLoginModeLayout();
                this.Show();
                this.CenterToScreen();
            };
            calculatorForm.Show();
        }

        // Đưa UI về chế độ đăng nhập, ẩn các trường đăng ký và đặt lại vị trí nút
        private void ForceLoginModeLayout()
        {
            // Tìm các control theo Name đã đặt trong InitializeComponent
            Control Find(string name) => this.Controls.Find(name, false).FirstOrDefault();

            var fullNameLabel = Find("fullNameLabel");
            var fullNameTextBox = Find("fullNameTextBox");
            var phoneLabel = Find("phoneLabel");
            var phoneTextBox = Find("phoneTextBox");
            var ageLabel = Find("ageLabel");
            var ageTextBox = Find("ageTextBox");
            var salaryLabel = Find("salaryLabel");
            var salaryTextBox = Find("salaryTextBox");
            var mealLabel = Find("mealLabel");
            var mealTextBox = Find("mealTextBox");
            var attendancePerDayLabel = Find("attendancePerDayLabel");
            var attendancePerDayTextBox = Find("attendancePerDayTextBox") as TextBox;
            var travelPerDayLabel = Find("travelPerDayLabel");
            var travelPerDayTextBox = Find("travelPerDayTextBox") as TextBox;
            var housingAllowanceLabel = Find("housingAllowanceLabel");
            var housingAllowanceTextBox = Find("housingAllowanceTextBox") as TextBox;
            var loginBtn = Find("loginBtn");
            var toggleBtn = Find("toggleBtn");

            // Ẩn các trường đăng ký
            foreach (var c in new[] { fullNameLabel, fullNameTextBox, phoneLabel, phoneTextBox, ageLabel, ageTextBox, salaryLabel, salaryTextBox, mealLabel, mealTextBox, attendancePerDayLabel, attendancePerDayTextBox, travelPerDayLabel, travelPerDayTextBox, housingAllowanceLabel, housingAllowanceTextBox })
            {
                if (c != null) c.Visible = false;
            }

            // Đặt lại kích thước form
            this.Height = 300;

            // Cập nhật nút và vị trí theo chế độ đăng nhập
            if (loginBtn != null) {
                (loginBtn as Button).Text = "🔐 Đăng Nhập";
                (loginBtn as Button).BackColor = System.Drawing.Color.FromArgb(255, 90, 0);
                (loginBtn as Button).ForeColor = System.Drawing.Color.White;
            }
            if (toggleBtn != null) {
                (toggleBtn as Button).Text = "📝 Chuyển sang Đăng Ký";
            }

            // Tính toán vị trí nhóm nút theo layout mặc định
            int formWidthLocal = this.Width;
            int calcWidthLocal = 196;
            int toggleWidthLocal = 236;
            int actionGapLocal = 12;
            int totalActionWidthLocal = calcWidthLocal + actionGapLocal + toggleWidthLocal;
            int actionStartXLoginLocal = (formWidthLocal - totalActionWidthLocal) / 2 - 8;
            int actionYLoginLocal = 170;

            if (loginBtn != null) loginBtn.Width = calcWidthLocal;
            if (toggleBtn != null) toggleBtn.Width = toggleWidthLocal;
            if (loginBtn != null) loginBtn.Height = 36;
            if (toggleBtn != null) toggleBtn.Height = 36;
            if (loginBtn != null) loginBtn.Location = new System.Drawing.Point(actionStartXLoginLocal, actionYLoginLocal);
            if (toggleBtn != null) toggleBtn.Location = new System.Drawing.Point(actionStartXLoginLocal + calcWidthLocal + actionGapLocal, actionYLoginLocal);
        }
    }
}
