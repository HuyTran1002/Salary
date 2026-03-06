using System;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using SalaryCalculator;
using System.Globalization;
using System.Drawing.Drawing2D;

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
            private Timer marqueeTimer = null; // Timer for scrolling marquee
            private Label marqueeLabel = null; // Marquee label for top rankings
            private int marqueeX = 0; // Current X position of marquee text
            private string marqueeText = ""; // Current marquee text content
            private int marqueeColorIndex = 0; // Current color index for rainbow effect
            private Timer stickFigureTimer = null;
            private Panel stickFigureHostPanel = null;
            private Panel stickFigureOverlayPanel = null;
            private PointF stickFigurePosition = PointF.Empty;
            private PointF stickFigureDestination = PointF.Empty;
            private Control stickActiveTarget = null;
            private int stickInteractionTicks = 0;
            private float stickWalkPhase = 0f;
            private float stickGesturePhase = 0f;
            private float stickHandReach = 0f;
            private string stickCurrentSpeech = "";
            private int stickSpeechTicks = 0;
            private float stickVelocityX = 0f;
            private float stickVelocityY = 0f;
            private bool stickOnGround = true;
            private bool stickWantsClimb = false;
            private PointF stickClimbTarget = PointF.Empty;
            private int stickJumpCooldown = 0;
            private int stickMoodTicks = 0;
            private string stickMood = "Tinh nghịch";
            private string stickAction = "Bò";
            private float stickPrevStrideSign = 0f;
            private float stickLeftFootPlantX = 0f;
            private float stickRightFootPlantX = 0f;
            private int stickParkourTicks = 0;
            private int stickSpeechCooldown = 0;
            private int stickSlipTicks = 0;
            private int stickAnnoyedTicks = 0;
            private int stickDisturbCooldown = 0;
            private int stickWallClingTicks = 0;
            private Rectangle stickLastDrawBounds = Rectangle.Empty;
            private readonly Dictionary<string, int> stickVisitCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            private readonly Random stickRandom = new Random();
            private readonly string[] stickMessages = new[]
            {
                "Xin chào!",
                "Ui xinh quá ✨",
                "Để mình xem nào",
                "Ô này quan trọng nè",
                "Con số đẹp đó!",
                "Tiếp tục nào 💙",
                "Nhập đúng là ra kết quả chuẩn nè",
                "Mình đang scan toàn bộ form nhé",
                "Mục này ảnh hưởng net đó",
                "Điền đủ ô đi, để mình tối ưu giúp",
                "Tính nhanh gọn, khỏi đau đầu",
                "Mình canh thuế giúp cho",
                "Lương đẹp thì mood cũng đẹp",
                "OT vừa phải thôi, giữ sức nha",
                "Mình theo dõi chi tiết từng mục luôn"
            };
            private readonly string[] stickExtraMessages = new[]
            {
                "Hôm nay nhập số mượt ghê!",
                "Mình đi tuần form một vòng đây",
                "Nhịp này tính lương cực nhanh luôn",
                "Bạn nhập chắc tay thật đó",
                "Form này đang chạy rất ổn",
                "Con số nào cũng có câu chuyện riêng",
                "Mình thấy dữ liệu đang khá đẹp",
                "Tiếp tục đi, gần ra kết quả rồi",
                "Giữ nhịp nhập liệu này là chuẩn",
                "Mình đang canh từng ô cho bạn",
                "Hôm nay năng suất cao nha",
                "Nét chữ số hôm nay rất tự tin",
                "Đang vào guồng rồi nè",
                "Nhập thêm chút nữa là đủ bộ",
                "Nhìn số liệu thấy yên tâm quá",
                "Bài toán này mình cân được",
                "Nhịp làm việc này đáng khen",
                "Cứ đều tay vậy là đẹp",
                "Mục tiêu net đang ở phía trước",
                "Bấm tính là có bất ngờ đó",
                "Mình thích cách bạn điền rất rõ ràng",
                "Data sạch thì kết quả sáng",
                "Mọi thứ đang đúng quỹ đạo",
                "Tinh thần làm lương lên cao nào",
                "Mình vừa rà lại, ổn áp nhé",
                "Bạn làm mình muốn chạy parkour luôn",
                "Con số đang kể chuyện tăng trưởng",
                "Đừng lo, mình giữ nhịp cho",
                "Nhìn bảng này thấy chuyên nghiệp ghê",
                "Tối ưu từng chút sẽ thành nhiều",
                "Mỗi ô nhập đúng là bớt lỗi ngay",
                "Tiến độ hôm nay rất xanh",
                "Bạn điền nhanh mà vẫn chính xác",
                "Tín hiệu tốt đang tăng dần",
                "Mình theo sát nên bạn cứ yên tâm",
                "Đang ở trạng thái tập trung cao",
                "Chuẩn bị chạm mốc net đẹp rồi",
                "Form này hợp tác tốt ghê",
                "Mình thích những con số rõ ràng",
                "Bình tĩnh là thắng bài toán này",
                "Nhập liệu đẹp như code sạch vậy",
                "Ô nào cũng quan trọng cả đó",
                "Mình đang săn lỗi giúp bạn",
                "Làm lương mà nhịp thế này là đỉnh",
                "Sắp đủ dữ liệu để chốt rồi",
                "Chút nữa xem net có cười không nha",
                "Càng rõ ràng càng dễ tối ưu",
                "Tiếp tục, bạn đang làm rất tốt",
                "Mình thấy trend đang đi lên",
                "Mốc lương tháng này khá hứa hẹn",
                "Động lực hôm nay đầy pin",
                "Cứ thế này thì kết quả sáng đẹp",
                "Bạn nhập kỹ nên mình xử lý rất mượt",
                "Một cú bấm tính là ra ngay",
                "Bảng lương đang dần hoàn chỉnh",
                "Hôm nay không bỏ sót chi tiết nào",
                "Mình canh thuế và OT song song",
                "Bạn điền tới đâu mình hiểu tới đó",
                "Giữ phong độ, sắp chốt xong",
                "Con số đang đứng đúng vị trí",
                "Kết quả chuẩn bắt đầu từ dữ liệu chuẩn",
                "Mình thích sự gọn gàng này",
                "Nhịp làm việc cân bằng quá tốt",
                "Bạn làm nhanh mà vẫn chắc",
                "Dữ liệu có vẻ đáng tin cậy",
                "Cú này net sẽ khá ổn nè",
                "Mình chuẩn bị nhận xét sâu hơn nhé",
                "Trạng thái: theo dõi thông minh",
                "Không khí tính lương hôm nay xịn quá",
                "Mọi đường dẫn đều về net",
                "Cứ điền tiếp, đừng ngắt nhịp",
                "Số liệu sạch là vua",
                "Bạn nhập đúng là mình vui liền",
                "Từng khoản đang khớp dần",
                "Mình thấy tháng này có tín hiệu tốt",
                "Ổn rồi, tiến thêm một bước nữa",
                "Mình giữ cho kết quả không lệch",
                "Năng lượng tích cực đang tăng",
                "Bài này giải được rất êm",
                "Bạn thao tác rất có nghề",
                "Mọi thứ đang vào đúng chỗ",
                "Mình thích form chạy mượt như này",
                "Cứ đều đặn là ra thành quả",
                "Đây là nhịp làm việc của người chuyên nghiệp",
                "Bạn làm tốt lắm, tiếp tục nha",
                "Đang gần đến đoạn hấp dẫn rồi",
                "Kết quả đẹp thường đến từ sự tỉ mỉ",
                "Sắp có câu trả lời cho tháng này",
                "Mình vẫn canh từng tham số cho bạn",
                "Bình tĩnh nhập, mình lo phần còn lại",
                "Mình sẵn sàng đưa nhận xét lương",
                "Con số hôm nay có thần thái ghê",
                "Dữ liệu càng đầy đủ càng thông minh",
                "Đang có đà tăng trưởng tốt",
                "Bạn và mình phối hợp khá ăn ý",
                "Mọi phép tính đang chạy ổn định",
                "Độ chính xác đang ở mức cao",
                "Chúng ta sắp chốt được bức tranh lương",
                "Tiến lên, còn vài bước là xong",
                "Mình ở đây để giữ mọi thứ chuẩn"
            };
            private readonly string[] stickSalaryComments = new[]
            {
                "Net tầm này khá ổn, đúng vùng mục tiêu 8-15 triệu rồi",
                "Mức này cân bằng tốt giữa thu nhập và chi tiêu",
                "Net đang trong dải an toàn, giữ nhịp đều là đẹp",
                "Thu nhập này ổn để duy trì quỹ dự phòng mỗi tháng",
                "Mức net này khá thực tế và bền vững",
                "Lương tháng này nhìn ổn áp đó nha",
                "Con số này phản ánh hiệu suất làm việc tốt",
                "Tối ưu thêm thuế/phụ cấp là sẽ nhích thêm",
                "Mức này đủ tốt để lên kế hoạch tài chính tháng",
                "Net tầm này tự thưởng nhẹ là hợp lý",
                "Con số này có đà tăng ổn định",
                "Mức thu nhập này đang đi đúng hướng",
                "Net như này là đáng ghi nhận rồi đó",
                "Giữ phong độ này vài tháng là rất đẹp",
                "Mức này phù hợp để tích lũy đều mỗi tháng",
                "Thu nhập hiện tại khá chắc và thực dụng",
                "Con số này cho thấy bạn đang tiến bộ",
                "Lương tầm này ổn để cân bằng cuộc sống",
                "Net này hợp lý, chỉ cần giữ sức bền",
                "Mức này nếu giảm chi phí nhỏ sẽ càng thoải mái",
                "Thu nhập đang ổn định, tiếp tục phát huy nha",
                "Lương tháng này chất lượng, không bị hụt nhịp",
                "Net trong khoảng này là khá tốt rồi",
                "Mức này đủ để đặt mục tiêu tăng nhẹ tháng sau",
                "Con số này đẹp theo kiểu bền vững",
                "Thu nhập tầm này quản lý tốt là rất khỏe",
                "Mức net hiện tại đang đúng kỳ vọng",
                "Lương này nhìn gọn gàng và đáng tin cậy",
                "Net đang ở vùng ổn, ưu tiên giữ đều",
                "Mức này ổn để vừa chi tiêu vừa tiết kiệm"
            };
            private readonly string[] stickAnnoyedLines = new[]
            {
                "Ê nhẹ tay thôi nha 😤",
                "Đang chạy mà bấm chi dữ vậy trời",
                "Từ từ thôi, trượt tay rồi nè!",
                "Ui, mém té luôn đó!",
                "Đừng troll mình nữa nha 😑",
                "Bấm kiểu này là mình cọc đó nha",
                "Trời ơi, xém rớt luôn!",
                "Cho mình làm việc yên ổn coi 😮‍💨"
            };
            private readonly string[] stickTaxTips = new[]
            {
                "Thuế cao thì ưu tiên khoản miễn/giảm hợp lệ nhé",
                "Giữ hồ sơ rõ ràng để tính thuế chuẩn hơn",
                "Kiểm tra mốc thuế trước khi chốt lương",
                "Mức thuế này cần theo dõi sát đó"
            };
            private readonly string[] stickOtTips = new[]
            {
                "OT x3 tăng nhanh nhưng nhớ giữ sức nha",
                "OT nhiều quá thì cân đối lại nghỉ ngơi",
                "Có OT thì net thường nhích rõ rệt",
                "Điền OT chính xác để tránh lệch kết quả"
            };
            private readonly string[] stickNetTips = new[]
            {
                "Net đẹp quá, đúng bài luôn ✨",
                "Kết quả này khá ổn để chốt tháng",
                "Mức net này đang trong vùng an toàn",
                "Net tăng đều là tín hiệu tốt đó"
            };
            private readonly string[] stickAmbientLines = new[]
            {
                "Mình đang bám và quét form liên tục nè",
                "Nhịp này mượt, để mình kiểm tra sâu hơn",
                "Mình vừa rà thêm vài ô quan trọng rồi",
                "Cứ nhập tiếp đi, mình đang theo rất sát",
                "Đang bò tuần tra toàn form cho bạn đây",
                "Mình sẽ ghé các ô cần chú ý ngay",
                "Nhịp thao tác ổn lắm, giữ đều nha",
                "Mình vẫn đang bám tường và quan sát số liệu"
            };
            private Color[] marqueeColors = new Color[] 
            {
                Color.FromArgb(180, 220, 255),
                Color.FromArgb(172, 242, 255),
                Color.FromArgb(202, 216, 255),
                Color.FromArgb(221, 207, 255),
                Color.FromArgb(191, 234, 255)
            };

        public SalaryCalculatorForm(string username = "")
        {
            currentUsername = username;
            isCustomTaxRate = false;  // Reset lại trạng thái % thuế mỗi khi đăng nhập lại
            InitializeComponent();
            // Để LoginForm kiểm soát quay lại khi form này đóng
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
                rankingTitle.ForeColor = Color.FromArgb(214, 236, 255);
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
                rankingGrid.GridColor = Color.FromArgb(189, 211, 245);
                rankingGrid.BackgroundColor = Color.FromArgb(243, 249, 255);
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
                rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(68, 118, 255);
                rankingGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                rankingGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                rankingGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                rankingGrid.RowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
                rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(237, 246, 255);
                rankingGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(162, 196, 255);
                rankingGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(24, 47, 89);
                rankingGrid.DefaultCellStyle.ForeColor = Color.FromArgb(40, 62, 104);
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
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(208, 230, 255);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(24, 47, 89);
                    }
                    else if (rank == 2)
                    {
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(221, 224, 255);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(34, 59, 99);
                    }
                    else if (rank == 3)
                    {
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 10.5f, System.Drawing.FontStyle.Bold);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(215, 240, 255);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(38, 64, 108);
                    }
                    else
                    {
                        // Regular rows: dark background and clear light text
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9.5f, System.Drawing.FontStyle.Regular);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
                        rankingGrid.Rows[rowIdx].DefaultCellStyle.ForeColor = Color.FromArgb(40, 62, 104);
                    }
                    rank++;
                }
                // Thêm dòng trống nếu ít hơn 20 hạng
                for (int i = sorted.Count + 1; i <= minRows; i++)
                {
                    int idx = rankingGrid.Rows.Add(i.ToString(), "", "", "");
                    rankingGrid.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(237, 246, 255);
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
                // Apply infinity web theme for admin ranking as well
                try { Theme.ApplyInfinityGlassTheme(this); } catch { }

                // Theme.ApplyEcommerceTheme may apply a DataGridView style intended for other themes.
                // Re-assert the ranking grid's white styling so numbers remain dark and readable.
                try
                {
                    rankingGrid.BackgroundColor = Color.FromArgb(243, 249, 255);
                    rankingGrid.RowsDefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
                    rankingGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(237, 246, 255);
                    rankingGrid.DefaultCellStyle.BackColor = Color.FromArgb(248, 252, 255);
                    rankingGrid.DefaultCellStyle.ForeColor = Color.FromArgb(40, 62, 104);
                    rankingGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(106, 162, 255);
                    rankingGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    rankingGrid.EnableHeadersVisualStyles = false;
                }
                catch { }

                return;
            }

            // Giao diện tính lương cho user thường
            this.Text = "Tính Lương - Salary Calculator";
            this.ClientSize = new Size(900, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new System.Drawing.Font("Arial", 9);
            this.AutoScroll = false;

            // Title Label
            Label titleLabelUser = new Label();
            titleLabelUser.Text = "TÍNH LƯƠNG NHÂN VIÊN";
            titleLabelUser.Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold);
            titleLabelUser.ForeColor = Color.FromArgb(214, 236, 255);
            titleLabelUser.Dock = DockStyle.Top;
            titleLabelUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            titleLabelUser.Height = 32;
            titleLabelUser.Padding = new Padding(0, 5, 0, 0);
            this.Controls.Add(titleLabelUser);

            // Small Calculator Launcher Button (bottom-right inside mainPanel)
            Button calculatorLauncher = new Button();
            // Use a calculator-like glyph; fallback to text if emoji not available
            calculatorLauncher.Text = "🧮";
            calculatorLauncher.Width = 44;
            calculatorLauncher.Height = 28;
            calculatorLauncher.Font = new System.Drawing.Font("Segoe UI Emoji", 12);
            calculatorLauncher.BackColor = Color.FromArgb(57, 122, 255);
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
            mainPanel.Location = new System.Drawing.Point((this.ClientSize.Width - 885) / 2, titleLabelUser.Height + 4);
            mainPanel.Width = 885;
            mainPanel.Height = this.ClientSize.Height - mainPanel.Top - 8;
            mainPanel.AutoScroll = false; // disable scroll as requested

            // Left/Right Column Panels (balanced and centered)
            Panel leftPanel = new Panel();
            Panel rightPanel = new Panel();
            leftPanel.Width = 420;
            rightPanel.Width = 420;
            leftPanel.Height = 380;
            rightPanel.Height = 380;
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

            Label slDaysOffLabel = new Label();
            slDaysOffLabel.Text = "Nghỉ SL (ngày):";
            slDaysOffLabel.Location = new System.Drawing.Point(10, leftY);
            slDaysOffLabel.Width = 110;
            slDaysOffLabel.Height = 18;

            TextBox slDaysOffTextBox = new TextBox();
            slDaysOffTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            slDaysOffTextBox.Width = 275;
            slDaysOffTextBox.Height = 20;
            slDaysOffTextBox.Name = "slDaysOffTextBox";
            slDaysOffTextBox.Font = new System.Drawing.Font("Arial", 8);
            slDaysOffTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(slDaysOffTextBox);

            leftY += 28;

            Label alDaysOffLabel = new Label();
            alDaysOffLabel.Text = "Nghỉ AL/PH (ngày):";
            alDaysOffLabel.Location = new System.Drawing.Point(10, leftY);
            alDaysOffLabel.Width = 120;
            alDaysOffLabel.Height = 18;

            TextBox alDaysOffTextBox = new TextBox();
            alDaysOffTextBox.Location = new System.Drawing.Point(130, leftY + 1);
            alDaysOffTextBox.Width = 275;
            alDaysOffTextBox.Height = 20;
            alDaysOffTextBox.Name = "alDaysOffTextBox";
            alDaysOffTextBox.Font = new System.Drawing.Font("Arial", 8);
            alDaysOffTextBox.Text = "0";
            NumberFormatter.FormatNumberInput(alDaysOffTextBox);

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
            insuranceTextBox.Width = 245; // leave room for edit button
            insuranceTextBox.Height = 20;
            insuranceTextBox.Name = "insuranceTextBox";
            insuranceTextBox.Font = new System.Drawing.Font("Arial", 8);
            insuranceTextBox.Text = "10.5";

            // Edit insurance percent button
            Button editInsuranceBtn = new Button();
            editInsuranceBtn.Text = "✏️";
            editInsuranceBtn.Location = new System.Drawing.Point(380, leftY - 2);
            editInsuranceBtn.Width = 28;
            editInsuranceBtn.Height = 22;
            editInsuranceBtn.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            editInsuranceBtn.BackColor = System.Drawing.Color.LightBlue;
            editInsuranceBtn.Name = "editInsuranceBtn";
            editInsuranceBtn.Click += (s, e) => OpenInsuranceEditForm(insuranceTextBox);

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
            taxThresholdTextBox.ReadOnly = true;
            taxThresholdTextBox.BackColor = System.Drawing.Color.LightGray;
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

            // SECTION 3: TIỀN PHỤ CẤP
            Label incentiveTitle = new Label();
            incentiveTitle.Text = "TIỀN PHỤ CẤP";
            incentiveTitle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            incentiveTitle.Location = new System.Drawing.Point(10, rightY);
            incentiveTitle.Width = 400;
            incentiveTitle.Height = 20;
            incentiveTitle.ForeColor = System.Drawing.Color.DarkOrange;

            rightY += 25;

            // Single total allowance (sum of all incentive components)
            Label allowanceLabel = new Label();
            allowanceLabel.Text = "Tiền Phụ Cấp:";
            allowanceLabel.Location = new System.Drawing.Point(10, rightY);
            allowanceLabel.Width = 110;
            allowanceLabel.Height = 18;
            allowanceLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);

            TextBox allowanceTextBox = new TextBox();
            allowanceTextBox.Location = new System.Drawing.Point(125, rightY);
            allowanceTextBox.Width = 220;
            allowanceTextBox.Height = 20;
            allowanceTextBox.Name = "allowanceTextBox";
            allowanceTextBox.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            allowanceTextBox.ReadOnly = true;
            allowanceTextBox.BackColor = System.Drawing.Color.FromArgb(220, 220, 255);
            allowanceTextBox.Text = "Tự động tính và hiển thị";

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

            rightY += rowGap;

            Label rankingPickLabel = new Label();
            rankingPickLabel.Text = "Xếp Loại:";
            rankingPickLabel.Location = new System.Drawing.Point(10, rightY);
            rankingPickLabel.Width = 110;
            rankingPickLabel.Height = 18;

            CheckBox rankingACheckBox = new CheckBox();
            rankingACheckBox.Text = "A (300k)";
            rankingACheckBox.Location = new System.Drawing.Point(125, rightY - 1);
            rankingACheckBox.Width = 75;
            rankingACheckBox.Height = 20;
            rankingACheckBox.Name = "rankingACheckBox";

            CheckBox rankingBCheckBox = new CheckBox();
            rankingBCheckBox.Text = "B (275k)";
            rankingBCheckBox.Location = new System.Drawing.Point(205, rightY - 1);
            rankingBCheckBox.Width = 75;
            rankingBCheckBox.Height = 20;
            rankingBCheckBox.Name = "rankingBCheckBox";

            CheckBox rankingCCheckBox = new CheckBox();
            rankingCCheckBox.Text = "C (250k)";
            rankingCCheckBox.Location = new System.Drawing.Point(285, rightY - 1);
            rankingCCheckBox.Width = 75;
            rankingCCheckBox.Height = 20;
            rankingCCheckBox.Name = "rankingCCheckBox";

            Button editRankingBtn = new Button();
            editRankingBtn.Text = "✏️";
            editRankingBtn.Location = new System.Drawing.Point(365, rightY - 2);
            editRankingBtn.Width = 28;
            editRankingBtn.Height = 22;
            editRankingBtn.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            editRankingBtn.BackColor = System.Drawing.Color.LightBlue;
            editRankingBtn.Name = "editRankingBtn";
            editRankingBtn.Tag = "300000|275000|250000";

            rankingACheckBox.CheckedChanged += (s, e) => { if (rankingACheckBox.Checked) { rankingBCheckBox.Checked = false; rankingCCheckBox.Checked = false; } };
            rankingBCheckBox.CheckedChanged += (s, e) => { if (rankingBCheckBox.Checked) { rankingACheckBox.Checked = false; rankingCCheckBox.Checked = false; } };
            rankingCCheckBox.CheckedChanged += (s, e) => { if (rankingCCheckBox.Checked) { rankingACheckBox.Checked = false; rankingBCheckBox.Checked = false; } };

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
                slDaysOffLabel, slDaysOffTextBox,
                alDaysOffLabel, alDaysOffTextBox,
                divider1,
                insuranceLabel, insuranceTextBox, editInsuranceBtn,
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
                allowanceLabel, allowanceTextBox,
                otherBonusLabel, otherBonusTextBox,
                rankingPickLabel, rankingACheckBox, rankingBCheckBox, rankingCCheckBox, editRankingBtn,
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

            // Scrolling marquee label for top 5 rankings
            marqueeLabel = new Label();
            marqueeLabel.AutoSize = false;
            marqueeLabel.Width = mainPanel.Width;
            marqueeLabel.Height = 26;
            int marqueeY = panelsBottom - 38;
            marqueeLabel.Location = new Point(0, marqueeY);
            marqueeLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            marqueeLabel.ForeColor = Color.FromArgb(24, 47, 89);
            marqueeLabel.BackColor = Color.FromArgb(180, 220, 255);
            marqueeLabel.TextAlign = ContentAlignment.MiddleLeft;
            marqueeLabel.Text = ""; // Empty, we'll draw manually
            mainPanel.Controls.Add(marqueeLabel);

            // Store the marquee text
            marqueeText = GetTop5RankingText();
            int marqueeHorizontalPadding = 24;

            // Initialize marquee scrolling
            marqueeX = marqueeLabel.Width + marqueeHorizontalPadding;
            marqueeTimer = new Timer();
            marqueeTimer.Interval = 28; // smoother scrolling
            int tickCount = 0;
            marqueeTimer.Tick += (s, e) =>
            {
                // Check if label is disposed
                if (marqueeLabel == null || marqueeLabel.IsDisposed || !marqueeLabel.IsHandleCreated)
                {
                    return;
                }
                
                marqueeX -= 2; // smoother movement
                Size textSize = TextRenderer.MeasureText(marqueeText, marqueeLabel.Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding);
                if (marqueeX < -(textSize.Width + marqueeHorizontalPadding))
                {
                    marqueeX = marqueeLabel.Width + marqueeHorizontalPadding; // Reset to right side with padding
                }
                
                // Smooth color interpolation instead of abrupt jumps
                tickCount++;
                int transitionSpan = 32;
                int cycleLength = transitionSpan * marqueeColors.Length;
                int cyclePos = tickCount % cycleLength;
                int fromIndex = cyclePos / transitionSpan;
                int toIndex = (fromIndex + 1) % marqueeColors.Length;
                float blend = (cyclePos % transitionSpan) / (float)transitionSpan;
                marqueeColorIndex = fromIndex;
                marqueeLabel.BackColor = BlendColor(marqueeColors[fromIndex], marqueeColors[toIndex], blend);
                
                marqueeLabel.Invalidate();
            };
            marqueeLabel.Paint += (s, e) =>
            {
                e.Graphics.Clear(marqueeLabel.BackColor);
                var textRect = new Rectangle(marqueeX, 0, marqueeLabel.Width + marqueeHorizontalPadding * 2, marqueeLabel.Height);
                TextRenderer.DrawText(e.Graphics, marqueeText, marqueeLabel.Font, textRect, marqueeLabel.ForeColor,
                    TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            };
            marqueeTimer.Start();

            // Add form closing handler to cleanup timer
            this.FormClosing += (s, e) =>
            {
                if (marqueeTimer != null)
                {
                    marqueeTimer.Stop();
                    marqueeTimer.Dispose();
                    marqueeTimer = null;
                }
            };

            // Place action buttons under marquee (optimized vertical spacing)
            actionY = marqueeLabel.Bottom + 8;

            Button calculateBtn = new Button();
            calculateBtn.Text = "⚡ TÍNH LƯƠNG";
            calculateBtn.Location = new System.Drawing.Point(actionStartX, actionY);
            calculateBtn.Width = 180;
            calculateBtn.Height = 40;
            calculateBtn.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            calculateBtn.BackColor = Color.FromArgb(0, 198, 255);
            calculateBtn.ForeColor = System.Drawing.Color.White;
            calculateBtn.Click += (s, e) =>
            {
                // before computing, make sure user has filled required profile info
                var missing = GetMissingUserInfo();
                if (missing.Count > 0)
                {
                    string message = "Vui lòng điền đầy đủ các mục sau trước khi tính lương:\n- " +
                                     string.Join("\n- ", missing);
                    var result = MessageBox.Show(message, "Thiếu thông tin", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.OK)
                    {
                        OpenEditForm(nameTextBox, salaryTextBox, mealTextBox);
                    }
                    // abort calculation until user updates info
                    return;
                }

                // Disable calculate button during calculation
                calculateBtn.Enabled = false;

                // Perform calculation
                CalculateSalary(nameTextBox, monthTextBox, yearTextBox, salaryTextBox, mealTextBox, workingDaysTextBox, slDaysOffTextBox, alDaysOffTextBox, overtime2xTextBox, overtime3xTextBox, otDays12TextBox, otDays8TextBox, overtime15xTextBox, insuranceTextBox, taxTextBox, allowanceTextBox, recognizeTextBox, otherBonusTextBox, taxThresholdTextBox);

                // Re-enable button after calculation
                calculateBtn.Enabled = true;
            };
            mainPanel.Controls.Add(calculateBtn);

            // Place calculator launcher inside mainPanel near the bottom-right corner
            try
            {
                // Position it to the right of logout button area; anchor keeps it at bottom-right
                calculatorLauncher.Location = new Point(mainPanel.Width - calculatorLauncher.Width - 12, actionY + 22);
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
            logoutBtn.BackColor = Color.FromArgb(88, 63, 130);
            logoutBtn.ForeColor = System.Drawing.Color.White;
            logoutBtn.Click += (s, e) => {
                // Đóng form tính lương; LoginForm sẽ hiện lại
                this.Close();
            };
            mainPanel.Controls.Add(logoutBtn);

            // Result Panel
            Panel resultPanel = new Panel();
            resultPanel.Name = "resultPanel";
            int resultX = (mainPanel.Width - 855) / 2;
            int resultTop = Math.Max(calculateBtn.Bottom, logoutBtn.Bottom) + 6;
            resultPanel.Location = new System.Drawing.Point(resultX, resultTop);
            resultPanel.Width = 855;
            resultPanel.Height = mainPanel.Height - resultTop - 6;
            resultPanel.Padding = new Padding(10);
            resultPanel.BorderStyle = BorderStyle.None;
            resultPanel.BackColor = Color.FromArgb(238, 247, 255);

            Label resultTitleLabel = new Label();
            resultTitleLabel.Text = "KẾT QUẢ";
            resultTitleLabel.Name = "resultTitleLabel";
            resultTitleLabel.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
            resultTitleLabel.Location = new System.Drawing.Point(10, 5);
            resultTitleLabel.Width = 200;
            resultTitleLabel.Height = 22;
            resultTitleLabel.Visible = false;

            // Left Column Results
            Label empNameLabel = new Label();
            empNameLabel.Text = "";
            empNameLabel.Location = new System.Drawing.Point(10, 40);
            empNameLabel.Width = 400;
            empNameLabel.Height = 20;
            empNameLabel.Name = "empNameLabel";
            empNameLabel.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            empNameLabel.ForeColor = Color.FromArgb(214, 236, 255);
            int detailY = 40; // bắt đầu từ nhân viên
            // spacing đều 21px, font tăng +1 cho phần kết quả
            int detailSpacing = 21;
            float detailFont = 9.5f;
            float detailFontBold = 9.5f;

            detailY += detailSpacing;
            Label dayRate8hLabel = new Label();
            dayRate8hLabel.Text = "";
            dayRate8hLabel.Location = new System.Drawing.Point(10, detailY);
            dayRate8hLabel.Width = 400;
            dayRate8hLabel.Height = 20;
            dayRate8hLabel.Name = "dayRate8hLabel";
            dayRate8hLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label mealDayLabel = new Label();
            mealDayLabel.Text = "";
            mealDayLabel.Location = new System.Drawing.Point(10, detailY);
            mealDayLabel.Width = 400;
            mealDayLabel.Height = 20;
            mealDayLabel.Name = "mealDayLabel";
            mealDayLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label dayRateLabel = new Label();
            dayRateLabel.Text = "";
            dayRateLabel.Location = new System.Drawing.Point(10, detailY);
            dayRateLabel.Width = 400;
            dayRateLabel.Height = 20;
            dayRateLabel.Name = "dayRateLabel";
            dayRateLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label grossLabel = new Label();
            grossLabel.Text = "";
            grossLabel.Location = new System.Drawing.Point(10, detailY);
            grossLabel.Width = 400;
            grossLabel.Height = 20;
            grossLabel.Name = "grossLabel";
            grossLabel.Font = new System.Drawing.Font("Arial", detailFontBold, System.Drawing.FontStyle.Bold);
            grossLabel.ForeColor = Color.FromArgb(132, 233, 255);

            detailY += detailSpacing;
            Label insuranceDeductLabel = new Label();
            insuranceDeductLabel.Text = "";
            insuranceDeductLabel.Location = new System.Drawing.Point(10, detailY);
            insuranceDeductLabel.Width = 400;
            insuranceDeductLabel.Height = 20;
            insuranceDeductLabel.Name = "insuranceDeductLabel";
            insuranceDeductLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label taxThresholdResultLabel = new Label();
            taxThresholdResultLabel.Text = "";
            taxThresholdResultLabel.Location = new System.Drawing.Point(10, detailY);
            taxThresholdResultLabel.Width = 400;
            taxThresholdResultLabel.Height = 20;
            taxThresholdResultLabel.Name = "taxThresholdResultLabel";
            taxThresholdResultLabel.Font = new System.Drawing.Font("Arial", detailFont);

            detailY += detailSpacing;
            Label taxDeductLabel = new Label();
            taxDeductLabel.Text = "";
            taxDeductLabel.Location = new System.Drawing.Point(10, detailY);
            taxDeductLabel.Width = 400;
            taxDeductLabel.Height = 20;
            taxDeductLabel.Name = "taxDeductLabel";
            taxDeductLabel.Font = new System.Drawing.Font("Arial", detailFont);


            detailY += detailSpacing;
            Label netLabel = new Label();
            netLabel.Text = "";
            netLabel.Font = new System.Drawing.Font("Arial", detailFontBold + 1f, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic);
            netLabel.ForeColor = Color.FromArgb(141, 239, 255);
            netLabel.Location = new System.Drawing.Point(10, detailY);
            netLabel.Width = 400;
            netLabel.Height = 20;
            netLabel.Name = "netLabel";

            detailY += detailSpacing;
            // Right Column - Detail breakdown
            Label detailTitleLabel = new Label();
            detailTitleLabel.Text = "CHI TIẾT:";
            detailTitleLabel.Name = "detailTitleLabel";
            detailTitleLabel.Font = new System.Drawing.Font("Arial", 9, System.Drawing.FontStyle.Bold);
            detailTitleLabel.Location = new System.Drawing.Point(430, 5);
            detailTitleLabel.Width = 400;
            detailTitleLabel.Height = 20;
            detailTitleLabel.ForeColor = Color.FromArgb(206, 232, 255);
            detailTitleLabel.Visible = false;

            Label detailLabel = new Label();
            detailLabel.Text = "";
            detailLabel.Location = new System.Drawing.Point(430, 30);
            detailLabel.Width = 410;
            detailLabel.Height = resultPanel.Height - 34;
            detailLabel.Name = "detailLabel";
            detailLabel.Font = new System.Drawing.Font("Arial", 8.5f);
            detailLabel.AutoSize = false;

            resultPanel.Controls.AddRange(new Control[] { 
                resultTitleLabel, empNameLabel, dayRate8hLabel, mealDayLabel, dayRateLabel, grossLabel, insuranceDeductLabel, taxDeductLabel, taxThresholdResultLabel, netLabel,
                detailTitleLabel, detailLabel
            });

            mainPanel.Controls.Add(resultPanel);
            marqueeLabel.BringToFront();
            calculateBtn.BringToFront();
            logoutBtn.BringToFront();
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
            Button editRankingBtnRef = rightPanel.Controls["editRankingBtn"] as Button;
            Label meal12DisplayLabelRef = rightPanel.Controls.Find("meal12DisplayLabel", false).FirstOrDefault() as Label;
            Label meal8DisplayLabelRef = rightPanel.Controls.Find("meal8DisplayLabel", false).FirstOrDefault() as Label;
            CheckBox rankingARef = rightPanel.Controls.Find("rankingACheckBox", false).FirstOrDefault() as CheckBox;
            CheckBox rankingBRef = rightPanel.Controls.Find("rankingBCheckBox", false).FirstOrDefault() as CheckBox;
            CheckBox rankingCRef = rightPanel.Controls.Find("rankingCCheckBox", false).FirstOrDefault() as CheckBox;
            
            if (editMeal12BtnRef != null)
            {
                editMeal12BtnRef.Click += (s, e) => OpenMealEditForm(editMeal12BtnRef, meal12DisplayLabelRef, "Tiền ăn OT 8/12h (mặc định 30,000 VND)");
            }
            
            if (editMeal8BtnRef != null)
            {
                editMeal8BtnRef.Click += (s, e) => OpenMealEditForm(editMeal8BtnRef, meal8DisplayLabelRef, "Tiền ăn OT +4h (mặc định 20,000 VND)");
            }

            if (editRankingBtnRef != null && rankingARef != null && rankingBRef != null && rankingCRef != null)
            {
                editRankingBtnRef.Click += (s, e) => OpenRankingEditForm(editRankingBtnRef, rankingARef, rankingBRef, rankingCRef);
            }

            // Apply Infinity Glass theme tweaks
            try { Theme.ApplyInfinityGlassTheme(this); } catch { }
            // Re-apply gray backgrounds for readonly fields after theme styling
            try { ApplyReadonlyTextboxStyles(); } catch { }
            try { InitializeStickFigureCompanion(mainPanel); } catch { }
        }

        private void InitializeStickFigureCompanion(Panel hostPanel)
        {
            if (hostPanel == null || hostPanel.IsDisposed)
            {
                return;
            }

            stickFigureHostPanel = hostPanel;
            stickFigurePosition = new PointF(28f, Math.Max(80f, hostPanel.Height * 0.35f));
            stickFigureDestination = stickFigurePosition;
            stickLeftFootPlantX = stickFigurePosition.X - 8f;
            stickRightFootPlantX = stickFigurePosition.X + 8f;
            stickPrevStrideSign = 0f;

            EnableDoubleBuffer(hostPanel);
            hostPanel.Paint -= StickFigureHostPanel_Paint;
            hostPanel.Paint += StickFigureHostPanel_Paint;
            WireStickClickHandlers(hostPanel);

            hostPanel.ControlAdded += (s, e) =>
            {
                if (e.Control != null)
                {
                    WireStickClickHandlers(e.Control);
                }
            };

            hostPanel.Resize += (s, e) =>
            {
                if (!hostPanel.IsDisposed)
                {
                    var bounds = GetStickWalkBounds();
                    stickFigurePosition = ClampPoint(stickFigurePosition, bounds);
                    stickFigureDestination = ClampPoint(stickFigureDestination, bounds);
                    hostPanel.Invalidate();
                }
            };

            if (stickFigureTimer != null)
            {
                try
                {
                    stickFigureTimer.Stop();
                    stickFigureTimer.Dispose();
                }
                catch { }
            }

            stickFigureTimer = new Timer();
            stickFigureTimer.Interval = 42;
            stickFigureTimer.Tick += (s, e) => UpdateStickFigureState();
            stickFigureTimer.Start();

            this.FormClosing += (s, e) =>
            {
                try
                {
                    if (stickFigureTimer != null)
                    {
                        stickFigureTimer.Stop();
                        stickFigureTimer.Dispose();
                        stickFigureTimer = null;
                    }

                    if (stickFigureOverlayPanel != null)
                    {
                        if (!stickFigureOverlayPanel.IsDisposed)
                        {
                            stickFigureOverlayPanel.Dispose();
                        }
                        stickFigureOverlayPanel = null;
                    }

                    if (stickFigureHostPanel != null && !stickFigureHostPanel.IsDisposed)
                    {
                        stickFigureHostPanel.Paint -= StickFigureHostPanel_Paint;
                    }
                }
                catch { }
            };
        }

        private void UpdateStickFigureState()
        {
            if (stickFigureHostPanel == null || stickFigureHostPanel.IsDisposed)
            {
                return;
            }

            var bounds = GetStickWalkBounds();
            EnsureStickPhysicsRecovery(bounds);
            stickGesturePhase += 0.2f;
            if (stickJumpCooldown > 0) stickJumpCooldown--;
            if (stickSpeechCooldown > 0) stickSpeechCooldown--;
            if (stickDisturbCooldown > 0) stickDisturbCooldown--;
            if (stickWallClingTicks > 0) stickWallClingTicks--;

            if (stickMoodTicks <= 0)
            {
                UpdateStickMood();
                stickMoodTicks = 45;
            }
            else
            {
                stickMoodTicks--;
            }

            if (stickSpeechTicks > 0)
            {
                stickSpeechTicks--;
            }
            else
            {
                stickCurrentSpeech = "";
            }

            if (stickSpeechTicks == 0 && stickSpeechCooldown <= 0 && stickSlipTicks == 0)
            {
                bool moving = Math.Abs(stickVelocityX) > 0.35f || Math.Abs(stickVelocityY) > 0.2f || stickWantsClimb || stickInteractionTicks > 0;
                if (moving && stickRandom.Next(100) < 12)
                {
                    stickCurrentSpeech = stickAmbientLines[stickRandom.Next(stickAmbientLines.Length)];
                    stickSpeechTicks = 64;
                    stickSpeechCooldown = 20;
                }
            }

            if (stickAnnoyedTicks > 0)
            {
                stickAnnoyedTicks--;
                if (stickAnnoyedTicks % 22 == 0)
                {
                    stickCurrentSpeech = stickAnnoyedLines[stickRandom.Next(stickAnnoyedLines.Length)];
                    stickSpeechTicks = 48;
                }
            }

            if (stickSlipTicks > 0)
            {
                stickSlipTicks--;
                float slipDelta = stickSlipTicks > 6 ? 1.7f : -1.1f;
                stickFigurePosition = new PointF(stickFigurePosition.X, stickFigurePosition.Y + slipDelta);
                stickAction = "Trượt tay";
                if (stickSlipTicks == 0)
                {
                    stickVelocityY = 0f;
                    stickOnGround = true;
                }
            }

            if (stickInteractionTicks > 0)
            {
                stickInteractionTicks--;
                stickWalkPhase += 0.18f;
                stickHandReach = Math.Min(1f, stickHandReach + 0.12f);
                stickAction = "Bám ô";
                stickVelocityX *= 0.75f;
                stickParkourTicks = Math.Max(0, stickParkourTicks - 1);
                if (stickSpeechCooldown <= 0 && stickInteractionTicks % 22 == 0)
                {
                    stickCurrentSpeech = BuildSmartSpeech(stickActiveTarget);
                    stickSpeechTicks = 58;
                    stickSpeechCooldown = 18;
                }
                if (!stickOnGround && stickWallClingTicks > 0)
                {
                    float lockX = stickFigureDestination.X - stickFigurePosition.X;
                    float lockY = stickFigureDestination.Y - stickFigurePosition.Y;
                    stickFigurePosition = new PointF(
                        stickFigurePosition.X + lockX * 0.35f,
                        stickFigurePosition.Y + lockY * 0.35f);
                    stickVelocityY = 0f;
                }
                else if (!stickOnGround)
                {
                    stickVelocityY += 0.28f;
                    stickFigurePosition = new PointF(stickFigurePosition.X, stickFigurePosition.Y + stickVelocityY);
                }
            }
            else
            {
                if (NeedNewDestination(bounds))
                {
                    PickNextStickDestination(bounds);
                }

                stickHandReach = Math.Max(0f, stickHandReach - 0.1f);

                float dx = stickFigureDestination.X - stickFigurePosition.X;
                float dy = stickFigureDestination.Y - stickFigurePosition.Y;

                if (stickParkourTicks > 0)
                {
                    stickParkourTicks--;
                }

                if (stickWantsClimb)
                {
                    float climbSpeedX = 1.7f;
                    float climbSpeedY = 2.2f;

                    float cx = stickClimbTarget.X - stickFigurePosition.X;
                    float cy = stickClimbTarget.Y - stickFigurePosition.Y;

                    float stepX = Math.Sign(cx) * Math.Min(Math.Abs(cx), climbSpeedX);
                    float stepY = Math.Sign(cy) * Math.Min(Math.Abs(cy), climbSpeedY);

                    stickFigurePosition = new PointF(stickFigurePosition.X + stepX, stickFigurePosition.Y + stepY);
                    stickWalkPhase += 0.45f;
                    stickAction = stickParkourTicks > 0 ? "Bám ô" : "Leo trèo";
                    stickOnGround = false;

                    if (Math.Abs(cx) < 2.5f && Math.Abs(cy) < 2.5f)
                    {
                        stickWantsClimb = false;
                        stickFigurePosition = stickClimbTarget;
                        stickOnGround = false;
                        stickVelocityY = 0f;
                        stickWallClingTicks = 72;
                        stickInteractionTicks = 64;
                    }
                }
                else
                {
                    bool shouldParkour = false;
                    if (stickActiveTarget != null && !stickActiveTarget.IsDisposed && stickActiveTarget.Visible)
                    {
                        Rectangle targetRect = GetBoundsRelativeTo(stickActiveTarget, stickFigureHostPanel);
                        float centerX = targetRect.Left + targetRect.Width * 0.5f;
                        bool tallObstacle = targetRect.Top < stickFigurePosition.Y - 58f;
                        bool nearTarget = Math.Abs(centerX - stickFigurePosition.X) < 120f;
                        shouldParkour = stickOnGround && tallObstacle && nearTarget && Math.Abs(dx) > 26f && stickJumpCooldown == 0;
                    }

                    float runTopSpeed = Math.Abs(dx) > 120f ? 5.2f : 3.2f;
                    float desiredVx = Math.Max(-runTopSpeed, Math.Min(runTopSpeed, dx * 0.09f));
                    stickVelocityX += (desiredVx - stickVelocityX) * 0.23f;

                    bool shouldJump = dy < -36f && stickOnGround && stickJumpCooldown == 0;
                    if (shouldParkour)
                    {
                        stickVelocityY = -6.8f;
                        stickVelocityX += Math.Sign(dx) * 1.1f;
                        stickOnGround = false;
                        stickJumpCooldown = 36;
                        stickParkourTicks = 26;
                        stickAction = "Bám ô";
                    }
                    else if (shouldJump)
                    {
                        stickVelocityY = -6.4f;
                        stickOnGround = false;
                        stickJumpCooldown = 42;
                    }

                    if (!stickOnGround)
                    {
                        stickVelocityY += 0.36f;
                    }

                    if (!stickOnGround && stickVelocityY > 0.15f)
                    {
                        TryMaintainWallCling(bounds);
                    }

                    stickFigurePosition = new PointF(stickFigurePosition.X + stickVelocityX, stickFigurePosition.Y + stickVelocityY);

                    float groundY = bounds.Bottom;
                    if (stickFigurePosition.Y >= groundY)
                    {
                        stickFigurePosition = new PointF(stickFigurePosition.X, groundY);
                        stickVelocityY = 0f;
                        stickOnGround = true;
                    }

                    stickWalkPhase += Math.Abs(stickVelocityX) * 0.11f + (stickOnGround ? 0.05f : 0.12f);

                    if (!stickOnGround)
                    {
                        if (stickParkourTicks > 0)
                        {
                            stickAction = "Bám ô";
                        }
                        else
                        {
                            stickAction = "Bám ô";
                        }
                    }
                    else if (Math.Abs(stickVelocityX) > 3.6f)
                    {
                        stickAction = "Bò";
                    }
                    else
                    {
                        stickAction = "Bò";
                    }

                    float planarDistance = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (planarDistance < 10f && stickOnGround)
                    {
                        stickInteractionTicks = 74;
                        stickVelocityX *= 0.4f;
                    }

                    if (stickOnGround)
                    {
                        float strideSign = Math.Sign((float)Math.Sin(stickWalkPhase));
                        if (stickPrevStrideSign <= 0f && strideSign > 0f)
                        {
                            stickRightFootPlantX = stickFigurePosition.X + 8f;
                        }
                        else if (stickPrevStrideSign >= 0f && strideSign < 0f)
                        {
                            stickLeftFootPlantX = stickFigurePosition.X - 8f;
                        }

                        if (Math.Abs(stickVelocityX) < 0.35f)
                        {
                            stickLeftFootPlantX = stickFigurePosition.X - 7.5f;
                            stickRightFootPlantX = stickFigurePosition.X + 7.5f;
                        }

                        stickPrevStrideSign = strideSign;
                    }
                }
            }

            stickFigurePosition = ClampPoint(stickFigurePosition, bounds);
            EnsureStickPhysicsRecovery(bounds);
            Rectangle updatedBounds = ComputeStickDrawBounds();
            Rectangle dirtyRegion = Rectangle.Union(stickLastDrawBounds, updatedBounds);
            if (dirtyRegion.Width <= 0 || dirtyRegion.Height <= 0)
            {
                dirtyRegion = updatedBounds;
            }

            dirtyRegion.Inflate(8, 8);
            stickFigureHostPanel.Invalidate(dirtyRegion);
            stickLastDrawBounds = updatedBounds;
        }

        private void EnsureStickPhysicsRecovery(Rectangle bounds)
        {
            bool invalidPosition = float.IsNaN(stickFigurePosition.X) || float.IsNaN(stickFigurePosition.Y) ||
                                   float.IsInfinity(stickFigurePosition.X) || float.IsInfinity(stickFigurePosition.Y);
            bool tooFarOut = stickFigurePosition.X < bounds.Left - 120f ||
                             stickFigurePosition.X > bounds.Right + 120f ||
                             stickFigurePosition.Y < bounds.Top - 120f ||
                             stickFigurePosition.Y > bounds.Bottom + 120f;

            if (invalidPosition || tooFarOut)
            {
                float safeX = Math.Max(bounds.Left + 24f, Math.Min(bounds.Right - 24f, stickFigurePosition.X));
                if (float.IsNaN(safeX) || float.IsInfinity(safeX))
                {
                    safeX = bounds.Left + (bounds.Width * 0.5f);
                }

                stickFigurePosition = new PointF(safeX, bounds.Bottom);
                stickFigureDestination = stickFigurePosition;
                stickVelocityX = 0f;
                stickVelocityY = 0f;
                stickWantsClimb = false;
                stickClimbTarget = PointF.Empty;
                stickSlipTicks = 0;
                stickInteractionTicks = Math.Min(stickInteractionTicks, 16);
                stickOnGround = true;
                if (stickAction == "Trượt tay" || stickAction == "Nhảy" || stickAction == "Parkour")
                {
                    stickAction = "Bò";
                }
            }

            if (stickFigurePosition.Y >= bounds.Bottom - 0.5f)
            {
                stickFigurePosition = new PointF(stickFigurePosition.X, bounds.Bottom);
                stickOnGround = true;
                if (stickVelocityY > 0f)
                {
                    stickVelocityY = 0f;
                }

                if (stickSlipTicks == 0 && (stickAction == "Trượt tay" || stickAction == "Nhảy"))
                {
                    stickAction = "Bò";
                }
            }
        }

        private Rectangle ComputeStickDrawBounds()
        {
            int x = (int)Math.Round(stickFigurePosition.X) - 46;
            int y = (int)Math.Round(stickFigurePosition.Y) - 82;
            int width = 180;
            int height = 138;

            if (stickInteractionTicks > 0 || stickSpeechTicks > 0)
            {
                width += 80;
                y -= 10;
                height += 18;
            }

            var rect = new Rectangle(x, y, width, height);

            if (stickActiveTarget != null && !stickActiveTarget.IsDisposed && stickActiveTarget.Visible && stickFigureHostPanel != null)
            {
                Rectangle targetRect = GetBoundsRelativeTo(stickActiveTarget, stickFigureHostPanel);
                targetRect.Inflate(14, 14);
                rect = Rectangle.Union(rect, targetRect);
            }

            if (stickFigureHostPanel != null && !stickFigureHostPanel.IsDisposed)
            {
                Rectangle clientRect = new Rectangle(Point.Empty, stickFigureHostPanel.ClientSize);
                rect.Intersect(clientRect);
            }

            return rect;
        }

        private bool NeedNewDestination(Rectangle bounds)
        {
            if (stickFigureDestination == PointF.Empty)
            {
                return true;
            }

            float dx = stickFigureDestination.X - stickFigurePosition.X;
            float dy = stickFigureDestination.Y - stickFigurePosition.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < 3f)
            {
                return true;
            }

            return !bounds.Contains(Point.Round(stickFigureDestination));
        }

        private void PickNextStickDestination(Rectangle bounds)
        {
            var candidates = GetInteractiveControls(stickFigureHostPanel)
                .Where(c => c.Visible && c.Enabled)
                .ToList();

            if (candidates.Count == 0)
            {
                stickActiveTarget = null;
                stickFigureDestination = new PointF(
                    stickRandom.Next(bounds.Left + 18, Math.Max(bounds.Left + 19, bounds.Right - 18)),
                    stickRandom.Next(bounds.Top + 70, Math.Max(bounds.Top + 71, bounds.Bottom - 22)));
                return;
            }

            var scored = candidates
                .Select(c => new { Control = c, Score = EvaluateStickTargetScore(c) })
                .OrderByDescending(x => x.Score)
                .Take(10)
                .ToList();

            if (scored.Count == 0)
            {
                stickActiveTarget = candidates[stickRandom.Next(candidates.Count)];
            }
            else
            {
                int pickIndex = stickRandom.Next(Math.Min(5, scored.Count));
                stickActiveTarget = scored[pickIndex].Control;
            }

            Rectangle targetRect = GetBoundsRelativeTo(stickActiveTarget, stickFigureHostPanel);
            bool climbMode;
            PointF destination = ComputeSmartStickDestination(targetRect, bounds, out climbMode);
            stickFigureDestination = ClampPoint(destination, bounds);
            stickWantsClimb = climbMode;
            if (stickWantsClimb)
            {
                stickClimbTarget = stickFigureDestination;
            }
            else
            {
                stickClimbTarget = PointF.Empty;
            }

            string key = GetStickControlKey(stickActiveTarget);
            if (!string.IsNullOrEmpty(key))
            {
                int visit = 0;
                stickVisitCounter.TryGetValue(key, out visit);
                stickVisitCounter[key] = visit + 1;
            }

            if (stickSpeechCooldown <= 0)
            {
                stickCurrentSpeech = BuildSmartSpeech(stickActiveTarget);
                stickSpeechTicks = 96;
                stickSpeechCooldown = 20;
            }
        }

        private PointF ComputeSmartStickDestination(Rectangle targetRect, Rectangle bounds, out bool climbMode)
        {
            climbMode = true;
            float targetCenterX = targetRect.Left + targetRect.Width / 2f;
            float targetCenterY = targetRect.Top + targetRect.Height / 2f;

            float margin = 14f;
            var anchors = new[]
            {
                new PointF(targetRect.Left - margin, targetCenterY),
                new PointF(targetRect.Right + margin, targetCenterY),
                new PointF(targetCenterX, targetRect.Top - margin),
                new PointF(targetCenterX, targetRect.Bottom + margin)
            };

            PointF best = anchors[0];
            float bestDistance = float.MaxValue;
            foreach (var anchor in anchors)
            {
                PointF clamped = ClampPoint(anchor, bounds);
                float dx = clamped.X - stickFigurePosition.X;
                float dy = clamped.Y - stickFigurePosition.Y;
                float distance = dx * dx + dy * dy;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = clamped;
                }
            }

            return best;
        }

        private bool TryMaintainWallCling(Rectangle bounds)
        {
            if (stickOnGround || stickWantsClimb)
            {
                return false;
            }

            if (stickActiveTarget == null || stickActiveTarget.IsDisposed || !stickActiveTarget.Visible)
            {
                return false;
            }

            float dx = stickFigureDestination.X - stickFigurePosition.X;
            float dy = stickFigureDestination.Y - stickFigurePosition.Y;
            bool nearDestination = Math.Abs(dx) <= 18f && Math.Abs(dy) <= 22f;
            if (!nearDestination)
            {
                return false;
            }

            stickFigurePosition = new PointF(
                stickFigurePosition.X + dx * 0.42f,
                stickFigurePosition.Y + dy * 0.42f);
            stickFigurePosition = ClampPoint(stickFigurePosition, bounds);
            stickVelocityY = 0f;
            stickVelocityX *= 0.6f;
            stickWallClingTicks = Math.Max(stickWallClingTicks, 18);
            stickAction = "Bám ô";
            return true;
        }

        private IEnumerable<Control> GetInteractiveControls(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child == null || child.IsDisposed)
                {
                    continue;
                }

                if (ReferenceEquals(child, stickFigureOverlayPanel))
                {
                    continue;
                }

                if ((child is Label || child is TextBox || child is Button || child is CheckBox || child is Panel) &&
                    !string.Equals(child.Name, "marqueeLabel", StringComparison.OrdinalIgnoreCase))
                {
                    yield return child;
                }

                foreach (var nested in GetInteractiveControls(child))
                {
                    yield return nested;
                }
            }
        }

        private Rectangle GetStickWalkBounds()
        {
            if (stickFigureHostPanel == null)
            {
                return new Rectangle(0, 0, 1, 1);
            }

            int left = 8;
            int top = 8;
            int right = Math.Max(left + 100, stickFigureHostPanel.ClientSize.Width - 8);
            int bottom = Math.Max(top + 100, stickFigureHostPanel.ClientSize.Height - 8);
            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        private static PointF ClampPoint(PointF p, Rectangle bounds)
        {
            float x = Math.Max(bounds.Left, Math.Min(bounds.Right, p.X));
            float y = Math.Max(bounds.Top, Math.Min(bounds.Bottom, p.Y));
            return new PointF(x, y);
        }

        private static Rectangle GetBoundsRelativeTo(Control control, Control ancestor)
        {
            if (control == null || ancestor == null)
            {
                return Rectangle.Empty;
            }

            Point screen = control.PointToScreen(Point.Empty);
            Point local = ancestor.PointToClient(screen);
            return new Rectangle(local, control.Size);
        }

        private int EvaluateStickTargetScore(Control control)
        {
            if (control == null)
            {
                return int.MinValue;
            }

            int score = stickRandom.Next(0, 18);
            string text = (control.Text ?? string.Empty).Trim();
            string name = control.Name ?? string.Empty;
            string key = GetStickControlKey(control);

            if (control is TextBox tb)
            {
                string value = (tb.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value) || value == "0") score += 65;
                else score += 26;
                if (!tb.ReadOnly) score += 10;
                if (!tb.ReadOnly && stickMood == "Tinh nghịch") score += 8;
            }
            else if (control is Label)
            {
                if (!string.IsNullOrWhiteSpace(text)) score += 30;
                if (name.Contains("result", StringComparison.OrdinalIgnoreCase) || name.Contains("net", StringComparison.OrdinalIgnoreCase)) score += 22;
            }
            else if (control is Button)
            {
                score += 16;
            }
            else if (control is CheckBox)
            {
                score += 14;
            }

            if (name.Contains("tax", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("salary", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Lương", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("Net", StringComparison.OrdinalIgnoreCase))
            {
                score += 20;
            }

            if (!string.IsNullOrWhiteSpace(key) && stickVisitCounter.TryGetValue(key, out int visits))
            {
                score -= Math.Min(40, visits * 10);
            }

            if (stickFigureHostPanel != null)
            {
                Rectangle rect = GetBoundsRelativeTo(control, stickFigureHostPanel);
                float dx = rect.Left + rect.Width / 2f - stickFigurePosition.X;
                float dy = rect.Top + rect.Height / 2f - stickFigurePosition.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                score -= (int)(distance * 0.035f);
            }

            return score;
        }

        private void UpdateStickMood()
        {
            decimal net = ReadNumberFromControl("netLabel");
            decimal tax = ReadNumberFromControl("taxTextBox");

            if (net >= 13000000m)
            {
                stickMood = "Phấn khích";
            }
            else if (tax > 0)
            {
                stickMood = "Tập trung";
            }
            else
            {
                stickMood = "Tinh nghịch";
            }
        }

        private decimal ReadNumberFromControl(string controlName)
        {
            try
            {
                var found = this.Controls.Find(controlName, true);
                if (found.Length == 0) return 0m;

                string text = string.Empty;
                if (found[0] is TextBox tb) text = tb.Text;
                else if (found[0] is Label lb) text = lb.Text;

                if (string.IsNullOrWhiteSpace(text)) return 0m;
                var chars = text.Where(c => char.IsDigit(c) || c == ',' || c == '.').ToArray();
                string cleaned = new string(chars).Replace(",", "");
                if (decimal.TryParse(cleaned, out decimal number)) return number;
                return 0m;
            }
            catch
            {
                return 0m;
            }
        }

        private string BuildSmartSpeech(Control target)
        {
            decimal netValue = ReadNumberFromControl("netLabel");
            decimal taxPercent = ReadNumberFromControl("taxTextBox");
            decimal overtime3x = ReadNumberFromControl("overtime3xTextBox");
            decimal overtime2x = ReadNumberFromControl("overtime2xTextBox");
            int emptyKeyFields = CountEmptyKeyInputs();

            if (stickAnnoyedTicks > 0)
            {
                return stickAnnoyedLines[stickRandom.Next(stickAnnoyedLines.Length)];
            }

            if (stickAction == "Parkour" || stickAction == "Bám ô")
            {
                return "Parkour mode: bám ô cho mượt nè!";
            }

            if (target == null)
            {
                if (emptyKeyFields >= 4) return "Mình thấy còn thiếu nhiều ô, điền dần nhé";
                if (netValue >= 10000000m) return GetRandomSalaryComment();
                if (taxPercent >= 20m) return "Thuế đang cao, cân nhắc OT hợp lý nè";
                if (overtime2x + overtime3x >= 30m) return "OT khá dày, nhớ cân bằng sức khỏe nha";
                return stickMood == "Phấn khích" ? "Hôm nay vui quá!" : GetRandomDialogue();
            }

            if (target is TextBox tb)
            {
                string val = (tb.Text ?? string.Empty).Trim();
                string lowerName = (tb.Name ?? string.Empty).ToLowerInvariant();

                if (lowerName.Contains("overtime3x"))
                {
                    return overtime3x > 0 ? stickOtTips[stickRandom.Next(stickOtTips.Length)] : "Có OT x3 thì net sẽ bật mạnh đó";
                }
                if (lowerName.Contains("tax"))
                {
                    if (taxPercent <= 0) return "Thuế đang 0%, khá ổn áp";
                    if (taxPercent >= 30) return "Thuế cao rồi, tối ưu phụ cấp thôi";
                    return stickTaxTips[stickRandom.Next(stickTaxTips.Length)];
                }
                if (lowerName.Contains("salary"))
                {
                    return GetRandomSalaryComment();
                }

                if (string.IsNullOrWhiteSpace(val) || val == "0")
                {
                    return stickMood == "Tập trung" ? "Điền ô này để tính chuẩn hơn" : "Ô này đang trống nè!";
                }
                if (decimal.TryParse(val.Replace(",", ""), out decimal n))
                {
                    if (n > 10000000m) return "Số này to ghê ✨";
                    if (n > 0m) return "Con số ổn áp đó!";
                }
                return "Để mình kiểm tra ô này";
            }

            string t = (target.Text ?? string.Empty).Trim();
            if (t.Contains("Net", StringComparison.OrdinalIgnoreCase) || t.Contains("Thực Nhận", StringComparison.OrdinalIgnoreCase))
            {
                if (netValue >= 10000000m) return GetRandomSalaryComment();
                if (netValue >= 8000000m) return "Net đang trong vùng mục tiêu 8-15 triệu, giữ nhịp này nha";
                return "Cố thêm chút nữa để chạm mốc net 8 triệu nè";
            }
            if (t.Contains("Thuế", StringComparison.OrdinalIgnoreCase))
            {
                return taxPercent >= 20m ? "Thuế này cao, mình nên tối ưu phụ cấp" : "Thuế này vẫn trong tầm kiểm soát";
            }
            if (target is Button)
            {
                return stickMood == "Phấn khích" ? "Bấm phát là mình chạy full combo!" : "Bấm nút này để mình phân tích kết quả nè";
            }

            var netFound = this.Controls.Find("netLabel", true);
            if (netFound.Length > 0 && netFound[0] is Label netLabel && !string.IsNullOrWhiteSpace(netLabel.Text))
            {
                return "Kết quả đang đẹp lắm 💙";
            }

            if (stickMood == "Tập trung") return "Mình đang theo dõi số liệu";
            if (stickMood == "Phấn khích") return "Yayyy, tiếp tục nào!";
            return GetRandomDialogue();
        }

        private string GetRandomDialogue()
        {
            int baseCount = stickMessages.Length;
            int extraCount = stickExtraMessages.Length;
            int pick = stickRandom.Next(baseCount + extraCount);
            return pick < baseCount ? stickMessages[pick] : stickExtraMessages[pick - baseCount];
        }

        private string GetRandomSalaryComment()
        {
            return stickSalaryComments[stickRandom.Next(stickSalaryComments.Length)];
        }

        private int CountEmptyKeyInputs()
        {
            string[] keys = new[]
            {
                "salaryTextBox", "workingDaysTextBox", "overtime2xTextBox", "overtime3xTextBox",
                "overtime15xTextBox", "otDays12TextBox", "otDays8TextBox", "otherBonusTextBox"
            };

            int emptyCount = 0;
            foreach (var key in keys)
            {
                var found = this.Controls.Find(key, true);
                if (found.Length == 0 || !(found[0] is TextBox tb))
                {
                    continue;
                }

                string value = (tb.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value) || value == "0")
                {
                    emptyCount++;
                }
            }

            return emptyCount;
        }

        private void WireStickClickHandlers(Control root)
        {
            if (root == null || root.IsDisposed)
            {
                return;
            }

            root.MouseDown -= StickControl_MouseDown;
            root.MouseDown += StickControl_MouseDown;

            foreach (Control child in root.Controls)
            {
                WireStickClickHandlers(child);
            }
        }

        private void StickControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (stickFigureHostPanel == null || stickFigureHostPanel.IsDisposed)
            {
                return;
            }

            Point clickPoint = stickFigureHostPanel.PointToClient(Control.MousePosition);
            TryDisturbStickFigure(clickPoint);
        }

        private void TryDisturbStickFigure(Point clickPoint)
        {
            if (stickDisturbCooldown > 0 || stickSlipTicks > 0)
            {
                return;
            }

            float dx = clickPoint.X - stickFigurePosition.X;
            float dy = clickPoint.Y - (stickFigurePosition.Y - 10f);
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            bool isMoving = Math.Abs(stickVelocityX) > 0.95f ||
                            Math.Abs(stickFigureDestination.X - stickFigurePosition.X) > 24f ||
                            stickAction == "Chạy" ||
                            stickAction == "Đi bộ" ||
                            stickAction == "Parkour" ||
                            stickAction == "Bò" ||
                            stickAction == "Bám ô";

            if (!isMoving || distance > 70f)
            {
                return;
            }

            stickSlipTicks = 12;
            stickAnnoyedTicks = 88;
            stickDisturbCooldown = 52;
            stickWallClingTicks = 0;
            stickOnGround = false;
            stickVelocityX *= 0.58f;
            stickVelocityY = 0.5f;
            stickAction = "Trượt tay";
            stickCurrentSpeech = stickAnnoyedLines[stickRandom.Next(stickAnnoyedLines.Length)];
            stickSpeechTicks = 68;
            stickSpeechCooldown = 12;
        }

        private static string GetStickControlKey(Control control)
        {
            if (control == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(control.Name))
            {
                return control.Name;
            }

            return control.GetType().Name + ":" + (control.Text ?? string.Empty);
        }

        private void StickFigureHostPanel_Paint(object sender, PaintEventArgs e)
        {
            if (stickFigureHostPanel == null || stickFigureHostPanel.IsDisposed)
            {
                return;
            }

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool facingRight = stickFigureDestination.X >= stickFigurePosition.X;
            float direction = facingRight ? 1f : -1f;
            float stride = (float)Math.Sin(stickWalkPhase);
            float strideCos = (float)Math.Cos(stickWalkPhase);
            float bodyBob = (float)Math.Sin(stickWalkPhase * 0.6f) * 1.1f;
            float lean = 0f;

            float headRadius = 7.2f;
            float bodyTopY = stickFigurePosition.Y - 23f + bodyBob;
            float bodyBottomY = stickFigurePosition.Y - 4f + bodyBob;
            float headCenterX = stickFigurePosition.X;
            float headCenterY = bodyTopY - 9f;

            if (stickAction == "Chạy") lean = 2.2f * direction;
            if (stickAction == "Leo trèo") lean = 1.4f * direction;

            PointF shoulderCenter = new PointF(stickFigurePosition.X + lean, bodyTopY + 6f);
            PointF hipCenter = new PointF(stickFigurePosition.X + lean * 0.45f, bodyBottomY);
            PointF shoulderLeft = new PointF(shoulderCenter.X - 4.8f, shoulderCenter.Y);
            PointF shoulderRight = new PointF(shoulderCenter.X + 4.8f, shoulderCenter.Y);
            PointF hipLeft = new PointF(hipCenter.X - 3.8f, hipCenter.Y);
            PointF hipRight = new PointF(hipCenter.X + 3.8f, hipCenter.Y);

            PointF leftHandTarget = new PointF(shoulderLeft.X - 6.5f, bodyTopY + 8f + stride * 2.5f);
            PointF rightHandTarget = new PointF(shoulderRight.X + 6.5f, bodyTopY + 8f - stride * 2.5f);
            PointF leftFootTarget = new PointF(hipLeft.X - 4.4f + stride * 1.8f, stickFigurePosition.Y + 11f + stride * 1.2f);
            PointF rightFootTarget = new PointF(hipRight.X + 4.4f - stride * 1.8f, stickFigurePosition.Y + 11f - stride * 1.2f);

            if (stickOnGround && (stickAction == "Đi bộ" || stickAction == "Chạy"))
            {
                float speed = Math.Abs(stickVelocityX);
                float stepLength = stickAction == "Chạy" ? 10.5f : 7.5f;
                float lift = stickAction == "Chạy" ? 5.2f : 3.6f;
                float groundY = stickFigurePosition.Y + 11f;

                bool leftSwing = stride > 0f;
                if (leftSwing)
                {
                    leftFootTarget = new PointF(hipLeft.X + stepLength * stride, groundY - Math.Abs(stride) * lift);
                    rightFootTarget = new PointF(
                        stickRightFootPlantX + Math.Sign(stickVelocityX) * Math.Min(speed * 0.22f, 1.1f),
                        groundY + Math.Abs(strideCos) * 0.25f);
                }
                else
                {
                    leftFootTarget = new PointF(
                        stickLeftFootPlantX + Math.Sign(stickVelocityX) * Math.Min(speed * 0.22f, 1.1f),
                        groundY + Math.Abs(strideCos) * 0.25f);
                    rightFootTarget = new PointF(hipRight.X - stepLength * stride, groundY - Math.Abs(stride) * lift);
                }
            }

            Rectangle targetRect = Rectangle.Empty;
            bool hasVisibleTarget = stickActiveTarget != null && !stickActiveTarget.IsDisposed && stickActiveTarget.Visible;
            if (hasVisibleTarget)
            {
                targetRect = GetBoundsRelativeTo(stickActiveTarget, stickFigureHostPanel);
                PointF touchPoint = new PointF(targetRect.Left + targetRect.Width / 2f, targetRect.Top + targetRect.Height / 2f);
                float tx = touchPoint.X - shoulderRight.X;
                float ty = touchPoint.Y - shoulderRight.Y;
                float dist = (float)Math.Sqrt(tx * tx + ty * ty);

                if (dist > 0.001f)
                {
                    float maxReach = 44f;
                    float nx = tx / dist;
                    float ny = ty / dist;
                    PointF stretched = new PointF(shoulderRight.X + nx * maxReach, shoulderRight.Y + ny * maxReach);
                    rightHandTarget = new PointF(
                        rightHandTarget.X + (stretched.X - rightHandTarget.X) * stickHandReach,
                        rightHandTarget.Y + (stretched.Y - rightHandTarget.Y) * stickHandReach);
                }
            }

            if (stickAction == "Chạy")
            {
                float sprint = (float)Math.Sin(stickWalkPhase * 2f) * 6f;
                leftHandTarget = new PointF(shoulderLeft.X - 9.2f, bodyTopY + 7f - sprint * 0.5f);
                rightHandTarget = new PointF(shoulderRight.X + 9.2f, bodyTopY + 7f + sprint * 0.5f);
                leftFootTarget = new PointF(hipLeft.X - 4f + sprint * 0.95f, stickFigurePosition.Y + 11f + sprint);
                rightFootTarget = new PointF(hipRight.X + 4f - sprint * 0.95f, stickFigurePosition.Y + 11f - sprint);
            }
            else if (stickAction == "Nhảy")
            {
                float kick = (float)Math.Sin(stickWalkPhase * 1.2f) * 2f;
                leftHandTarget = new PointF(shoulderLeft.X - 5f, bodyTopY - 12f);
                rightHandTarget = new PointF(shoulderRight.X + 5f, bodyTopY - 12f);
                leftFootTarget = new PointF(hipLeft.X - 1.8f, stickFigurePosition.Y + 7f - kick);
                rightFootTarget = new PointF(hipRight.X + 1.8f, stickFigurePosition.Y + 7f + kick);
            }
            else if (stickAction == "Leo trèo" || stickAction == "Bò")
            {
                float climbWave = (float)Math.Sin(stickWalkPhase * 1.4f) * 5f;
                leftHandTarget = new PointF(shoulderLeft.X - 2f, bodyTopY + 0.5f + climbWave);
                rightHandTarget = new PointF(shoulderRight.X + 2f, bodyTopY + 9f - climbWave);
                leftFootTarget = new PointF(hipLeft.X - 1.2f, stickFigurePosition.Y + 8f - climbWave * 0.55f);
                rightFootTarget = new PointF(hipRight.X + 1.2f, stickFigurePosition.Y + 8f + climbWave * 0.55f);
            }
            else if (stickAction == "Bám ô")
            {
                float cling = (float)Math.Sin(stickGesturePhase * 1.5f) * 1.8f;
                leftHandTarget = new PointF(shoulderLeft.X - 2.4f, bodyTopY - 1.5f + cling);
                rightHandTarget = new PointF(shoulderRight.X + 2.4f, bodyTopY - 4.5f - cling);
                leftFootTarget = new PointF(hipLeft.X - 0.8f, stickFigurePosition.Y + 8.5f - cling * 0.35f);
                rightFootTarget = new PointF(hipRight.X + 0.8f, stickFigurePosition.Y + 7.4f + cling * 0.35f);
            }
            else if (stickAction == "Parkour")
            {
                float pk = (float)Math.Sin(stickGesturePhase * 2.2f);
                leftHandTarget = new PointF(shoulderLeft.X - 4.8f, bodyTopY - 8f - pk * 1.2f);
                rightHandTarget = new PointF(shoulderRight.X + 4.8f, bodyTopY - 5f + pk * 1.2f);
                leftFootTarget = new PointF(hipLeft.X - 2.2f, stickFigurePosition.Y + 7.2f + pk * 0.8f);
                rightFootTarget = new PointF(hipRight.X + 5.2f, stickFigurePosition.Y + 10.6f - pk * 0.8f);
            }
            else if (stickAction == "Tương tác")
            {
                float wave = (float)Math.Sin(stickGesturePhase * 1.8f) * 3f;
                leftHandTarget = new PointF(shoulderLeft.X - 5.2f, bodyTopY + 6.5f + wave);
            }
            else if (stickAction == "Trượt tay")
            {
                float panic = (float)Math.Sin(stickGesturePhase * 2.6f) * 2.4f;
                leftHandTarget = new PointF(shoulderLeft.X - 5.6f, bodyTopY - 7f + panic);
                rightHandTarget = new PointF(shoulderRight.X + 5.6f, bodyTopY - 6f - panic);
                leftFootTarget = new PointF(hipLeft.X - 2.4f, stickFigurePosition.Y + 9.6f + panic * 0.2f);
                rightFootTarget = new PointF(hipRight.X + 2.4f, stickFigurePosition.Y + 9.2f - panic * 0.2f);
            }

            EnforceSideSeparation(ref leftHandTarget, ref rightHandTarget, stickFigurePosition.X, 8.8f);
            EnforceSideSeparation(ref leftFootTarget, ref rightFootTarget, stickFigurePosition.X, 7.6f);

            PointF leftElbowHint = new PointF(shoulderLeft.X - 8f, shoulderLeft.Y + 2.2f);
            PointF rightElbowHint = new PointF(shoulderRight.X + 8f, shoulderRight.Y + 2.2f);
            PointF leftKneeHint = new PointF(hipLeft.X - 6.5f, hipLeft.Y + 8.5f);
            PointF rightKneeHint = new PointF(hipRight.X + 6.5f, hipRight.Y + 8.5f);

            PointF leftElbow = ComputeJointPointWithHint(shoulderLeft, leftHandTarget, 8.8f, 8.3f, leftElbowHint);
            PointF rightElbow = ComputeJointPointWithHint(shoulderRight, rightHandTarget, 8.8f, 8.3f, rightElbowHint);
            PointF leftKnee = ComputeJointPointWithHint(hipLeft, leftFootTarget, 9.2f, 10.2f, leftKneeHint);
            PointF rightKnee = ComputeJointPointWithHint(hipRight, rightFootTarget, 9.2f, 10.2f, rightKneeHint);

            using (var bodyPen = new Pen(Color.FromArgb(22, 22, 22), 3f))
            using (var limbPen = new Pen(Color.FromArgb(18, 18, 18), 3.2f))
            using (var headBrush = new SolidBrush(Color.FromArgb(25, 25, 25)))
            using (var jointBrush = new SolidBrush(Color.FromArgb(18, 18, 18)))
            {
                bodyPen.StartCap = LineCap.Round;
                bodyPen.EndCap = LineCap.Round;
                limbPen.StartCap = LineCap.Round;
                limbPen.EndCap = LineCap.Round;

                g.FillEllipse(headBrush, headCenterX - headRadius, headCenterY - headRadius, headRadius * 2, headRadius * 2);

                g.DrawLine(bodyPen, stickFigurePosition.X, bodyTopY, stickFigurePosition.X, bodyBottomY);
                g.DrawLine(bodyPen, shoulderLeft.X, shoulderLeft.Y - 0.8f, shoulderRight.X, shoulderRight.Y - 0.8f);
                g.DrawLine(bodyPen, hipLeft.X, hipLeft.Y, hipRight.X, hipRight.Y);

                DrawJointedLimb(g, limbPen, jointBrush, shoulderLeft, leftElbow, leftHandTarget, 1.9f);
                DrawJointedLimb(g, limbPen, jointBrush, shoulderRight, rightElbow, rightHandTarget, 1.9f);
                DrawJointedLimb(g, limbPen, jointBrush, hipLeft, leftKnee, leftFootTarget, 2.1f);
                DrawJointedLimb(g, limbPen, jointBrush, hipRight, rightKnee, rightFootTarget, 2.1f);

                g.DrawLine(bodyPen, leftFootTarget.X - 2.6f, leftFootTarget.Y + 0.8f, leftFootTarget.X + 2.6f, leftFootTarget.Y + 0.8f);
                g.DrawLine(bodyPen, rightFootTarget.X - 2.6f, rightFootTarget.Y + 0.8f, rightFootTarget.X + 2.6f, rightFootTarget.Y + 0.8f);

                float frontEyeX = headCenterX + (facingRight ? 2.3f : -2.3f);
                float backEyeX = headCenterX + (facingRight ? -1.1f : 1.1f);
                g.FillEllipse(Brushes.White, frontEyeX - 1.05f, headCenterY - 1.1f, 2.1f, 2.1f);
                g.FillEllipse(Brushes.White, backEyeX - 0.95f, headCenterY - 1.0f, 1.9f, 1.9f);
            }

            if (hasVisibleTarget)
            {
                targetRect.Inflate(3, 3);

                using (var glowPen = new Pen(Color.FromArgb(90, 255, 222, 117), 2f))
                {
                    g.DrawRectangle(glowPen, targetRect);
                }

                if (stickInteractionTicks > 0 && stickInteractionTicks % 12 < 6)
                {
                    float sparkX = targetRect.Left + targetRect.Width * 0.5f + (float)Math.Sin(stickGesturePhase) * 4f;
                    float sparkY = targetRect.Top - 2f;
                    using (var sparkPen = new Pen(Color.FromArgb(180, 255, 245, 142), 1.6f))
                    {
                        g.DrawLine(sparkPen, sparkX - 4, sparkY, sparkX + 4, sparkY);
                        g.DrawLine(sparkPen, sparkX, sparkY - 4, sparkX, sparkY + 4);
                    }
                }

                if (stickInteractionTicks > 0 || stickSpeechTicks > 0)
                {
                    DrawSpeechBubble(g, string.IsNullOrWhiteSpace(stickCurrentSpeech)
                        ? GetRandomDialogue()
                        : stickCurrentSpeech);
                }
            }
        }

        private static PointF ComputeJointPoint(PointF root, PointF end, float upperLen, float lowerLen, bool bendRight)
        {
            float dx = end.X - root.X;
            float dy = end.Y - root.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 0.001f)
            {
                return new PointF(root.X + (bendRight ? upperLen : -upperLen), root.Y);
            }

            float minDist = Math.Abs(upperLen - lowerLen) + 0.001f;
            float maxDist = upperLen + lowerLen - 0.001f;
            float clampedDist = Math.Max(minDist, Math.Min(maxDist, dist));

            float nx = dx / dist;
            float ny = dy / dist;
            PointF mid = new PointF(root.X + nx * (clampedDist * 0.5f), root.Y + ny * (clampedDist * 0.5f));

            float half = clampedDist * 0.5f;
            float hSq = upperLen * upperLen - half * half;
            float h = hSq > 0f ? (float)Math.Sqrt(hSq) : 0f;

            float px = -ny;
            float py = nx;
            float sign = bendRight ? 1f : -1f;
            return new PointF(mid.X + px * h * sign, mid.Y + py * h * sign);
        }

        private static PointF ComputeJointPointWithHint(PointF root, PointF end, float upperLen, float lowerLen, PointF hint)
        {
            float dx = end.X - root.X;
            float dy = end.Y - root.Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 0.001f)
            {
                return hint;
            }

            float minDist = Math.Abs(upperLen - lowerLen) + 0.001f;
            float maxDist = upperLen + lowerLen - 0.001f;
            float d = Math.Max(minDist, Math.Min(maxDist, dist));

            float nx = dx / dist;
            float ny = dy / dist;

            float a = (upperLen * upperLen - lowerLen * lowerLen + d * d) / (2f * d);
            float hSq = upperLen * upperLen - a * a;
            float h = hSq > 0f ? (float)Math.Sqrt(hSq) : 0f;

            PointF basePoint = new PointF(root.X + nx * a, root.Y + ny * a);
            if (h <= 0.0001f)
            {
                return basePoint;
            }

            float px = -ny;
            float py = nx;

            PointF jointA = new PointF(basePoint.X + px * h, basePoint.Y + py * h);
            PointF jointB = new PointF(basePoint.X - px * h, basePoint.Y - py * h);

            float da = (jointA.X - hint.X) * (jointA.X - hint.X) + (jointA.Y - hint.Y) * (jointA.Y - hint.Y);
            float db = (jointB.X - hint.X) * (jointB.X - hint.X) + (jointB.Y - hint.Y) * (jointB.Y - hint.Y);
            return da <= db ? jointA : jointB;
        }

        private static void DrawJointedLimb(Graphics g, Pen limbPen, Brush jointBrush, PointF root, PointF joint, PointF end, float jointRadius)
        {
            g.DrawLine(limbPen, root, joint);
            g.DrawLine(limbPen, joint, end);
            g.FillEllipse(jointBrush, joint.X - jointRadius, joint.Y - jointRadius, jointRadius * 2f, jointRadius * 2f);
        }

        private static void EnforceSideSeparation(ref PointF leftPoint, ref PointF rightPoint, float bodyCenterX, float minGap)
        {
            float leftMaxX = bodyCenterX - minGap * 0.5f;
            float rightMinX = bodyCenterX + minGap * 0.5f;

            if (leftPoint.X > leftMaxX)
            {
                leftPoint = new PointF(leftMaxX, leftPoint.Y);
            }

            if (rightPoint.X < rightMinX)
            {
                rightPoint = new PointF(rightMinX, rightPoint.Y);
            }

            if (rightPoint.X - leftPoint.X < minGap)
            {
                float middle = (leftPoint.X + rightPoint.X) * 0.5f;
                leftPoint = new PointF(middle - minGap * 0.5f, leftPoint.Y);
                rightPoint = new PointF(middle + minGap * 0.5f, rightPoint.Y);
            }
        }

        private void DrawSpeechBubble(Graphics g, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            using (var font = new Font("Arial", 7.5f, FontStyle.Bold))
            {
                Size textSize = TextRenderer.MeasureText(text, font);
                int bubbleWidth = Math.Max(70, textSize.Width + 12);
                int bubbleHeight = Math.Max(24, textSize.Height + 4);
                int x = (int)stickFigurePosition.X + 10;
                int y = (int)stickFigurePosition.Y - 50;

                if (stickFigureHostPanel != null)
                {
                    if (x + bubbleWidth > stickFigureHostPanel.ClientSize.Width - 4)
                    {
                        x = Math.Max(4, (int)stickFigurePosition.X - bubbleWidth - 10);
                    }
                    if (y < 2)
                    {
                        y = (int)stickFigurePosition.Y + 8;
                    }
                }

                Rectangle bubbleRect = new Rectangle(x, y, bubbleWidth, bubbleHeight);
                using (var bubbleBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                using (var bubblePen = new Pen(Color.FromArgb(120, 78, 126), 1.2f))
                {
                    g.FillRectangle(bubbleBrush, bubbleRect);
                    g.DrawRectangle(bubblePen, bubbleRect);
                }

                TextRenderer.DrawText(g, text, font, bubbleRect, Color.FromArgb(37, 60, 102), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private sealed class StickFigureOverlayPanel : Panel
        {
            public StickFigureOverlayPanel()
            {
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw |
                         ControlStyles.SupportsTransparentBackColor, true);
                DoubleBuffered = true;
                BackColor = Color.Transparent;
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    const int WS_EX_TRANSPARENT = 0x20;
                    var cp = base.CreateParams;
                    cp.ExStyle |= WS_EX_TRANSPARENT;
                    return cp;
                }
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // Intentionally skip background paint so this overlay does not cover sibling controls.
            }

            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x84;
                const int HTTRANSPARENT = -1;

                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }

                base.WndProc(ref m);
            }
        }

        private static void EnableDoubleBuffer(Control control)
        {
            if (control == null)
            {
                return;
            }

            try
            {
                var property = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                property?.SetValue(control, true, null);
            }
            catch
            {
            }
        }

        private void ApplyReadonlyTextboxStyles()
        {
            string[] readonlyNames = new[]
            {
                "nameTextBox", "phoneTextBox", "ageTextBox",
                "salaryTextBox", "mealTextBox", "workingDaysTextBox",
                "allowanceTextBox", "taxThresholdTextBox"
            };

            foreach (var name in readonlyNames)
            {
                var found = this.Controls.Find(name, true);
                if (found.Length > 0 && found[0] is TextBox tb)
                {
                    tb.ReadOnly = true;
                    tb.BackColor = Color.FromArgb(227, 236, 250);
                    tb.ForeColor = Color.FromArgb(33, 55, 94);
                }
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
                Control[] found = this.Controls.Find("allowanceTextBox", true);
                if (found.Length > 0 && found[0] is TextBox allowanceTextBox)
                {
                    allowanceTextBox.Text = "Tự động tính và hiển thị";
                }

                var rankingA = this.Controls.Find("rankingACheckBox", true).FirstOrDefault() as CheckBox;
                var rankingB = this.Controls.Find("rankingBCheckBox", true).FirstOrDefault() as CheckBox;
                var rankingC = this.Controls.Find("rankingCCheckBox", true).FirstOrDefault() as CheckBox;
                var editRankingBtn = this.Controls.Find("editRankingBtn", true).FirstOrDefault() as Button;
                if (rankingA != null && rankingB != null && rankingC != null)
                {
                    rankingA.Checked = (user.RatingBonus == "A");
                    rankingB.Checked = (user.RatingBonus == "B");
                    rankingC.Checked = (user.RatingBonus == "C");
                    ApplyRankingCheckboxTexts(rankingA, rankingB, rankingC, user.RankingABonusAmount, user.RankingBBonusAmount, user.RankingCBonusAmount);
                    if (editRankingBtn != null)
                    {
                        editRankingBtn.Tag = $"{user.RankingABonusAmount}|{user.RankingBBonusAmount}|{user.RankingCBonusAmount}";
                    }
                }

                // Insurance percent
                Control[] insuranceFound = this.Controls.Find("insuranceTextBox", true);
                if (insuranceFound.Length > 0 && insuranceFound[0] is TextBox insuranceTextBox)
                {
                    insuranceTextBox.Text = user.InsurancePercent.ToString();
                }
                
                // Don't auto-load recognize count - let user input monthly
                // Load tax threshold
                Control[] taxThresholdFound = this.Controls.Find("taxThresholdTextBox", true);
                if (taxThresholdFound.Length > 0 && taxThresholdFound[0] is TextBox taxThresholdTextBox)
                {
                    decimal thresholdToShow = user.TaxThreshold > 0 ? user.TaxThreshold : BaseTaxThreshold;
                    taxThresholdTextBox.Text = NumberFormatter.FormatNumberDisplay(thresholdToShow);
                    taxThresholdTextBox.ReadOnly = true;
                    taxThresholdTextBox.BackColor = Color.FromArgb(227, 236, 250);
                }

                // Load OT meal editable amounts to buttons and labels
                var editMeal12Btn = this.Controls.Find("editMeal12Btn", true).FirstOrDefault() as Button;
                var meal12DisplayLabelRef = this.Controls.Find("meal12DisplayLabel", true).FirstOrDefault() as Label;
                if (editMeal12Btn != null)
                {
                    editMeal12Btn.Tag = user.OtMeal12Amount;
                    if (meal12DisplayLabelRef != null)
                    {
                        meal12DisplayLabelRef.Text = $"× {(user.OtMeal12Amount/1000m):F1}k";
                    }
                }
                var editMeal8Btn = this.Controls.Find("editMeal8Btn", true).FirstOrDefault() as Button;
                var meal8DisplayLabelRef = this.Controls.Find("meal8DisplayLabel", true).FirstOrDefault() as Label;
                if (editMeal8Btn != null)
                {
                    editMeal8Btn.Tag = user.OtMeal8Amount;
                    if (meal8DisplayLabelRef != null)
                    {
                        meal8DisplayLabelRef.Text = $"× {(user.OtMeal8Amount/1000m):F1}k";
                    }
                }
            }
        }

        // examine user record and return a list of profile fields that are still empty or zero
        // updated to understand the newer breakdown fields (travel/housing/etc.) so that
        // users who only fill those do not get a misleading "missing" warning.
        private List<string> GetMissingUserInfo()
        {
            var missing = new List<string>();
            if (string.IsNullOrEmpty(currentUsername))
                return missing;
            var u = userDataManager.Login(currentUsername);
            if (u == null)
                return missing;
            if (string.IsNullOrWhiteSpace(u.FullName))
                missing.Add("Tên đầy đủ");
            if (string.IsNullOrWhiteSpace(u.Phone))
                missing.Add("SĐT/Zalo");
            if (u.Age <= 0)
                missing.Add("Tuổi");
            if (u.BasicSalary <= 0)
                missing.Add("Lương cơ bản");
            if (u.MealAllowance <= 0)
                missing.Add("Tiền ăn/tháng");

            // allowance used to be a single field, but newer versions split it into
            // travel/housing/cert/rating components.  Treat the profile as complete if
            // any of those sub‑fields are provided (or the old "Allowance" value is
            // still nonzero for backwards compatibility).
            bool hasAllowance = u.Allowance > 0 ||
                                u.TravelAllowance > 0 ||
                                u.HousingAllowance > 0 ||
                                u.CertificateBonus > 0 ||
                                !string.IsNullOrWhiteSpace(u.RatingBonus);
            if (!hasAllowance)
                missing.Add("Tiền phụ cấp (ví dụ: đi lại, nhà ở, chứng chỉ, xếp loại)");

            // similarly attendance incentive may be stored as a flat amount or as a per‑day
            // rate; either one counts.
            bool hasAttendance = u.AttendanceIncentive > 0 || u.AttendancePerDay > 0;
            if (!hasAttendance)
                missing.Add("Tiền thưởng chuyên cần (hoặc chuyên cần/ngày)");

            return missing;
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
            editForm.Height = 570;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;
            try { Theme.ApplyInfinityGlassTheme(editForm); } catch { }

            // Full Name
            int startY = 26, gapY = 44;
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
            mealLabel.Text = "Tiền ăn/tháng:";
            mealLabel.Location = new System.Drawing.Point(30, startY + gapY * 4);
            mealLabel.Width = 120;
            editForm.Controls.Add(mealLabel);

            TextBox mealEditBox = new TextBox();
            mealEditBox.Location = new System.Drawing.Point(160, startY + gapY * 4 - 3);
            mealEditBox.Width = 250;
            mealEditBox.Text = NumberFormatter.FormatNumberDisplay(user.MealAllowance.ToString());
            NumberFormatter.FormatNumberInput(mealEditBox);
            editForm.Controls.Add(mealEditBox);

            // Attendance per day
            Label attendancePerDayLabel = new Label();
            attendancePerDayLabel.Text = "Chuyên cần/ngày:";
            attendancePerDayLabel.Location = new System.Drawing.Point(30, startY + gapY * 5);
            attendancePerDayLabel.Width = 120;
            editForm.Controls.Add(attendancePerDayLabel);

            TextBox attendancePerDayEditBox = new TextBox();
            attendancePerDayEditBox.Location = new System.Drawing.Point(160, startY + gapY * 5 - 3);
            attendancePerDayEditBox.Width = 250;
            attendancePerDayEditBox.Text = NumberFormatter.FormatNumberDisplay(user.AttendancePerDay.ToString());
            NumberFormatter.FormatNumberInput(attendancePerDayEditBox);
            editForm.Controls.Add(attendancePerDayEditBox);

            // Travel per day
            Label travelPerDayLabel = new Label();
            travelPerDayLabel.Text = "Đi lại/ngày:";
            travelPerDayLabel.Location = new System.Drawing.Point(30, startY + gapY * 6);
            travelPerDayLabel.Width = 120;
            editForm.Controls.Add(travelPerDayLabel);

            TextBox travelPerDayEditBox = new TextBox();
            travelPerDayEditBox.Location = new System.Drawing.Point(160, startY + gapY * 6 - 3);
            travelPerDayEditBox.Width = 250;
            travelPerDayEditBox.Text = NumberFormatter.FormatNumberDisplay(user.TravelAllowance.ToString());
            NumberFormatter.FormatNumberInput(travelPerDayEditBox);
            editForm.Controls.Add(travelPerDayEditBox);

            // Housing Allowance
            Label housingLabel = new Label();
            housingLabel.Text = "Tiền nhà ở:";
            housingLabel.Location = new System.Drawing.Point(30, startY + gapY * 7);
            housingLabel.Width = 120;
            editForm.Controls.Add(housingLabel);

            TextBox housingEditBox = new TextBox();
            housingEditBox.Location = new System.Drawing.Point(160, startY + gapY * 7 - 3);
            housingEditBox.Width = 250;
            housingEditBox.Text = NumberFormatter.FormatNumberDisplay(user.HousingAllowance.ToString());
            NumberFormatter.FormatNumberInput(housingEditBox);
            editForm.Controls.Add(housingEditBox);

            // Certificate Bonus
            Label certLabel = new Label();
            certLabel.Text = "Tiền chứng chỉ:";
            certLabel.Location = new System.Drawing.Point(30, startY + gapY * 8);
            certLabel.Width = 120;
            editForm.Controls.Add(certLabel);

            TextBox certEditBox = new TextBox();
            certEditBox.Location = new System.Drawing.Point(160, startY + gapY * 8 - 3);
            certEditBox.Width = 250;
            certEditBox.Text = NumberFormatter.FormatNumberDisplay(user.CertificateBonus.ToString());
            NumberFormatter.FormatNumberInput(certEditBox);
            editForm.Controls.Add(certEditBox);

            // Tax Threshold
            Label taxThresholdLabel = new Label();
            taxThresholdLabel.Text = "Mốc lương tính thuế:";
            taxThresholdLabel.Location = new System.Drawing.Point(30, startY + gapY * 9);
            taxThresholdLabel.Width = 120;
            editForm.Controls.Add(taxThresholdLabel);

            TextBox taxThresholdEditBox = new TextBox();
            taxThresholdEditBox.Location = new System.Drawing.Point(160, startY + gapY * 9 - 3);
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
            int btnY = startY + gapY * 10 - 16;
            int btnGap = 30;
            int formWidth = editForm.ClientSize.Width;
            int totalBtnWidth = saveBtn.Width + btnGap + 120;
            int btnStartX = (formWidth - totalBtnWidth) / 2;
            saveBtn.Location = new System.Drawing.Point(btnStartX, btnY);
            saveBtn.Click += (s, e) =>
            {
                if (UpdateUserData(nameEditBox.Text, phoneEditBox.Text, ageEditBox.Text, salaryEditBox.Text, mealEditBox.Text, certEditBox.Text, taxThresholdEditBox.Text, attendancePerDayEditBox.Text, travelPerDayEditBox.Text, housingEditBox.Text))
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

            try { Theme.ApplyInfinityGlassTheme(editForm); } catch { }
            editForm.ShowDialog();
        }

        private bool UpdateUserData(string fullName, string phone, string age, string salary, string meal, string certificate, string taxThreshold, string attendancePerDay, string travelPerDay, string housingAllowance)
        {
            try
            {
                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(age))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin (Tên, SĐT, Tuổi)!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Default empty fields to "0"
                if (string.IsNullOrWhiteSpace(salary)) salary = "0";
                if (string.IsNullOrWhiteSpace(meal)) meal = "0";
                if (string.IsNullOrWhiteSpace(certificate)) certificate = "0";
                if (string.IsNullOrWhiteSpace(taxThreshold)) taxThreshold = "0";
                if (string.IsNullOrWhiteSpace(attendancePerDay)) attendancePerDay = "8500";
                if (string.IsNullOrWhiteSpace(travelPerDay)) travelPerDay = "8500";
                if (string.IsNullOrWhiteSpace(housingAllowance)) housingAllowance = "100000";

                if (!int.TryParse(age, out int userAge) || !decimal.TryParse(salary, out decimal basicSalary) ||
                    !decimal.TryParse(meal, out decimal mealAllowance) ||
                    !decimal.TryParse(certificate, out decimal certificateBonus) || !decimal.TryParse(taxThreshold, out decimal taxThresholdValue) ||
                    !decimal.TryParse(attendancePerDay, out decimal attendancePerDayValue) || !decimal.TryParse(travelPerDay, out decimal travelPerDayValue) || !decimal.TryParse(housingAllowance, out decimal housingAllowanceValue))
                {
                    MessageBox.Show("Tuổi phải là số, Lương, tiền ăn, tiền chứng chỉ, chuyên cần/ngày, đi lại/ngày, tiền nhà ở và mốc lương tính thuế phải là số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                if (attendancePerDayValue < 0 || travelPerDayValue < 0 || housingAllowanceValue < 0)
                {
                    MessageBox.Show("Tiền chuyên cần/ngày, tiền đi lại/ngày và tiền nhà ở không được âm!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (taxThresholdValue <= 0)
                {
                    MessageBox.Show("Mốc lương tính thuế phải lớn hơn 0!", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                return userDataManager.Register(currentUsername, fullName, phone, userAge, basicSalary, mealAllowance, 0, 0, 0, taxThresholdValue, "", certificateBonus, attendancePerDayValue, travelPerDayValue, housingAllowanceValue);
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

        private void UpdateDailyRate(TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox slDaysOffTextBox, TextBox alDaysOffTextBox)
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

        private void CalculateSalary(TextBox nameTextBox, TextBox monthTextBox, TextBox yearTextBox, TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox slDaysOffTextBox, TextBox alDaysOffTextBox,
                          TextBox overtime2xTextBox, TextBox overtime3xTextBox, TextBox otDays12TextBox, TextBox otDays8TextBox, TextBox overtime15xTextBox, TextBox insuranceTextBox, TextBox taxTextBox,
                          TextBox allowanceTextBox, TextBox recognizeTextBox, TextBox otherBonusTextBox, TextBox taxThresholdTextBox)
        {
            try
            {
                // Clear previous results before calculating new ones
                try
                {
                    var resultTitleFound = this.Controls.Find("resultTitleLabel", true);
                    if (resultTitleFound.Length > 0 && resultTitleFound[0] is Label resultTitle)
                    {
                        resultTitle.Visible = false;
                    }
                    var detailTitleFound = this.Controls.Find("detailTitleLabel", true);
                    if (detailTitleFound.Length > 0 && detailTitleFound[0] is Label detailTitle)
                    {
                        detailTitle.Visible = false;
                    }

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
                EnsureNumericDefaults(salaryTextBox, mealTextBox, workingDaysTextBox, slDaysOffTextBox, alDaysOffTextBox, overtime2xTextBox, overtime3xTextBox, otDays12TextBox, otDays8TextBox, overtime15xTextBox, insuranceTextBox, taxTextBox, allowanceTextBox, recognizeTextBox, otherBonusTextBox, taxThresholdTextBox);
                // Validate required info before calculation
                if (string.IsNullOrEmpty(nameTextBox.Text) || string.IsNullOrEmpty(salaryTextBox.Text) || string.IsNullOrEmpty(mealTextBox.Text) ||
                    string.IsNullOrEmpty(workingDaysTextBox.Text) || string.IsNullOrEmpty(allowanceTextBox.Text))
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
                decimal slDaysOff = decimal.Parse(slDaysOffTextBox.Text);
                decimal alDaysOff = decimal.Parse(alDaysOffTextBox.Text);
                decimal overtime2xHours = decimal.Parse(overtime2xTextBox.Text);  // Làm thêm x2 lương
                decimal overtime3xHours = decimal.Parse(overtime3xTextBox.Text);  // Làm thêm x3 lương
                decimal otDays12 = decimal.Parse(otDays12TextBox.Text);  // Số ngày OT 8/12h
                decimal otDays8 = decimal.Parse(otDays8TextBox.Text);    // Số ngày OT +4h
                decimal overtime15xHours = decimal.Parse(overtime15xTextBox.Text); // Làm thêm x1.5 lương
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

                if (slDaysOff < 0) slDaysOff = 0;
                if (alDaysOff < 0) alDaysOff = 0;
                if (slDaysOff + alDaysOff > workingDays)
                {
                    MessageBox.Show("Tổng ngày nghỉ SL + AL không được vượt quá số ngày công!", "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                decimal payableDays = workingDays - slDaysOff;
                decimal allowanceEligibleDays = workingDays - slDaysOff - alDaysOff;
                if (payableDays < 0) payableDays = 0;
                if (allowanceEligibleDays < 0) allowanceEligibleDays = 0;

                decimal travelPerDay = 8500m;
                decimal attendancePerDay = 8500m;
                decimal rankingAAmount = 300000m;
                decimal rankingBAmount = 275000m;
                decimal rankingCAmount = 250000m;
                // Calculate new incentive components
                decimal travelAllowance = 0m;
                decimal attendanceIncentive = 0m;
                decimal housingAllowance = 100000m;
                decimal ratingBonus = 0m;
                decimal certificateBonus = 0m;

                // Get user data to retrieve rating and certificate
                var user = userDataManager.Login(currentUsername);
                string selectedRating = "";
                var rankingA = this.Controls.Find("rankingACheckBox", true).FirstOrDefault() as CheckBox;
                var rankingB = this.Controls.Find("rankingBCheckBox", true).FirstOrDefault() as CheckBox;
                var rankingC = this.Controls.Find("rankingCCheckBox", true).FirstOrDefault() as CheckBox;
                if (rankingA?.Checked == true) selectedRating = "A";
                else if (rankingB?.Checked == true) selectedRating = "B";
                else if (rankingC?.Checked == true) selectedRating = "C";

                var editRankingBtn = this.Controls.Find("editRankingBtn", true).FirstOrDefault() as Button;
                if (editRankingBtn != null)
                {
                    var parts = (editRankingBtn.Tag?.ToString() ?? "").Split('|');
                    if (parts.Length == 3)
                    {
                        decimal.TryParse(parts[0], out rankingAAmount);
                        decimal.TryParse(parts[1], out rankingBAmount);
                        decimal.TryParse(parts[2], out rankingCAmount);
                    }
                }

                if (user != null)
                {
                    travelPerDay = user.TravelAllowance > 0 ? user.TravelAllowance : 8500m;
                    attendancePerDay = user.AttendancePerDay > 0 ? user.AttendancePerDay : 8500m;
                    housingAllowance = user.HousingAllowance > 0 ? user.HousingAllowance : 100000m;
                    rankingAAmount = user.RankingABonusAmount > 0 ? user.RankingABonusAmount : rankingAAmount;
                    rankingBAmount = user.RankingBBonusAmount > 0 ? user.RankingBBonusAmount : rankingBAmount;
                    rankingCAmount = user.RankingCBonusAmount > 0 ? user.RankingCBonusAmount : rankingCAmount;

                    if (string.IsNullOrWhiteSpace(selectedRating))
                    {
                        selectedRating = user.RatingBonus ?? "";
                    }

                    // Calculate rating bonus
                    if (selectedRating == "A") ratingBonus = rankingAAmount;
                    else if (selectedRating == "B") ratingBonus = rankingBAmount;
                    else if (selectedRating == "C") ratingBonus = rankingCAmount;

                    certificateBonus = user.CertificateBonus;
                }

                travelAllowance = travelPerDay * allowanceEligibleDays;
                attendanceIncentive = attendancePerDay * allowanceEligibleDays;

                // Display incentive components in textboxes
                try
                {
                    var allowanceTB = this.Controls.Find("allowanceTextBox", true).FirstOrDefault() as TextBox;
                    decimal allowanceDisplayTotal = travelAllowance + attendanceIncentive + housingAllowance + ratingBonus + certificateBonus;
                    if (allowanceTB != null) allowanceTB.Text = allowanceDisplayTotal.ToString("N0");
                }
                catch { }


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

                // Calculate deductions - Bảo hiểm dựa trên % nhập, mặc định 10.5%
                decimal insurancePercent = 10.5m;
                if (insuranceTextBox != null)
                {
                    insurancePercent = ParsePercent(insuranceTextBox.Text, 10.5m);
                }
                if (insurancePercent < 0) insurancePercent = 0;
                decimal insuranceDeduction = Math.Round(basicSalary * (insurancePercent / 100m), 0, MidpointRounding.AwayFromZero);

                // Dynamic tax threshold = base (user input) + 730,000 + phần chênh lệch OT x2/x3 + khấu trừ BH
                decimal taxThreshold = ComputeTaxThreshold(baseTaxThresholdInput, hourlyRate, overtime2xHours, overtime3xHours, insuranceDeduction);

                // Calculate gross salary components:
                decimal baseRegularSalary = workingDays * dailySalaryForMeal;
                decimal slSalaryDeduction = slDaysOff * dailySalaryForMeal;
                decimal regularSalary = baseRegularSalary - slSalaryDeduction;
                if (regularSalary < 0) regularSalary = 0;
                decimal overtime2xSalary = Math.Round(overtime2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal overtime3xSalary = Math.Round(overtime3xHours * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                decimal overtime15xSalary = Math.Round(overtime15xHours * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                // Calculate Incentive - New structure with 5 components + recognize + other bonus
                decimal totalIncentive = travelAllowance + attendanceIncentive + housingAllowance + ratingBonus + certificateBonus + (recognizeCount * 50000) + otherBonus;

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
                var resultTitleFoundAfterCalc = this.Controls.Find("resultTitleLabel", true);
                if (resultTitleFoundAfterCalc.Length > 0 && resultTitleFoundAfterCalc[0] is Label resultTitleAfterCalc)
                {
                    resultTitleAfterCalc.Visible = true;
                }
                var detailTitleFoundAfterCalc = this.Controls.Find("detailTitleLabel", true);
                if (detailTitleFoundAfterCalc.Length > 0 && detailTitleFoundAfterCalc[0] is Label detailTitleAfterCalc)
                {
                    detailTitleAfterCalc.Visible = true;
                }

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

                decimal allowanceTotal = travelAllowance + attendanceIncentive + housingAllowance + ratingBonus + certificateBonus;
                string incentiveInfo = $"\n  • Tổng phụ cấp: {allowanceTotal:C0} VND" +
                                        $"\n    - Tiền đi lại ({allowanceEligibleDays:F1} ngày × {travelPerDay:C0}): {travelAllowance:C0} VND" +
                                        $"\n    - Tiền chuyên cần ({allowanceEligibleDays:F1} ngày × {attendancePerDay:C0}): {attendanceIncentive:C0} VND" +
                                        $"\n    - Tiền nhà ở: {housingAllowance:C0} VND";
                if (ratingBonus > 0)
                {
                    incentiveInfo += $"\n    - Tiền xếp loại ({selectedRating}): {ratingBonus:C0} VND";
                }
                if (certificateBonus > 0)
                {
                    incentiveInfo += $"\n    - Tiền chứng chỉ: {certificateBonus:C0} VND";
                }
                if (recognizeCount > 0)
                {
                    incentiveInfo += $"\n  • Recognize ({recognizeCount} × 50k): {recognizeCount * 50000:C0} VND";
                }
                if (otherBonus > 0)
                {
                    incentiveInfo += $"\n  • Tiền bonus khác: {otherBonus:C0} VND";
                }

                string detail = $"Chi Tiết:\n" +
                    $"  • Lương ngày công gốc ({workingDays:F1} ngày × {dailySalaryForMeal:C0}): {baseRegularSalary:C0} VND\n" +
                    $"  • Trừ nghỉ SL ({slDaysOff:F1} ngày × {dailySalaryForMeal:C0}): -{slSalaryDeduction:C0} VND\n" +
                    $"  • Lương ngày công sau trừ SL: {regularSalary:C0} VND\n" +
                    $"  • OT x2 ({overtime2xHours:F1} tiếng × {hourlyRate:C0} × 2): {overtime2xSalary:C0} VND\n" +
                    $"  • OT x3 ({overtime3xHours:F1} tiếng × {hourlyRate:C0} × 3): {overtime3xSalary:C0} VND\n" +
                    $"  • OT x1.5 ({overtime15xHours:F1} tiếng × {hourlyRate:C0} × 1.5): {overtime15xSalary:C0} VND{bonusInfo}{incentiveInfo}";
                
                // Apply typing effect sequentially: line N completes before line N+1 starts
                var labels = new[] { empNameLabel, dayRate8hLabel, mealDayLabel, dayRateLabel, grossLabel, insuranceDeductLabel, taxThresholdResultLabel, taxDeductLabel, netLabel };
                var labelTexts = new[] 
                { 
                    $"Nhân Viên: {employeeName}",
                    $"Lương cơ bản 1 ngày: {basicDailySalary:C0} VND",
                    $"Tiền ăn 1 ngày: {mealDailySalary:C0} VND",
                    $"Tổng lương 1 ngày công: {dailySalaryForMeal:C0} VND",
                    $"Lương Brutto: {grossSalary:C0} VND",
                    $"Khấu Trừ Bảo Hiểm ({insurancePercent}% lương cơ bản): {insuranceDeduction:C0} VND",
                    $"Mốc thuế áp dụng: {taxThreshold:C0} VND",
                    $"Khấu Trừ Thuế: {(taxDeductionDisplay > 0 ? taxDeductionDisplay.ToString("C0") + " VND" : "0 VND")}",
                    $"Lương Net (Thực Nhận): {netSalary:C0} VND"
                };

                void TypeNextLabel(int index)
                {
                    if (index >= labels.Length)
                    {
                        return;
                    }

                    string currentText = labelTexts[index];
                    bool isLastLabel = index == labels.Length - 1;

                    AnimateTypingEffect(labels[index], currentText, 2, 3, () =>
                    {
                        if (isLastLabel)
                        {
                            // Display detail label only after all result lines are fully completed
                            detailLabel.Text = detail;

                            // Update marquee with latest rankings
                            if (marqueeLabel != null)
                            {
                                marqueeText = GetTop5RankingText();
                                marqueeX = marqueeLabel.Width; // Reset position
                            }

                            // Play applause sound when net salary exceeds 15 million
                            if (netSalary > 15000000m)
                            {
                                try
                                {
                                    PlayApplauseEmbedded();
                                }
                                catch
                                {
                                }
                            }
                        }
                        else
                        {
                            TypeNextLabel(index + 1);
                        }
                    });
                }

                TypeNextLabel(0);
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
            saveBtn.Width = 100;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            saveBtn.Location = new System.Drawing.Point((editForm.ClientSize.Width - saveBtn.Width) / 2, 80);
            saveBtn.Click += (s, e) =>
            {
                // Remove thousand separators before parsing
                string cleanText = amountBox.Text.Replace(",", "");
                if (decimal.TryParse(cleanText, out decimal amount))
                {
                    button.Tag = amount;
                    // Update display label with k format
                    if (displayLabel != null)
                    {
                        decimal k = amount / 1000;
                        // Use F1 to show exact value without rounding (e.g., 12.5k instead of 13k)
                        displayLabel.Text = $"× {k:F1}k";
                    }
                    try
                    {
                        if (button.Name == "editMeal12Btn")
                        {
                            userDataManager.UpdateOtMeal12Amount(currentUsername, amount);
                        }
                        else if (button.Name == "editMeal8Btn")
                        {
                            userDataManager.UpdateOtMeal8Amount(currentUsername, amount);
                        }
                    }
                    catch { }
                    editForm.Close();
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số tiền hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            editForm.Controls.Add(saveBtn);

            try { Theme.ApplyInfinityGlassTheme(editForm); } catch { }
            editForm.ShowDialog();
        }

        private void ApplyRankingCheckboxTexts(CheckBox rankingA, CheckBox rankingB, CheckBox rankingC, decimal amountA, decimal amountB, decimal amountC)
        {
            decimal safeA = amountA > 0 ? amountA : 300000m;
            decimal safeB = amountB > 0 ? amountB : 275000m;
            decimal safeC = amountC > 0 ? amountC : 250000m;

            if (rankingA != null) rankingA.Text = $"A ({safeA:C0} VND)";
            if (rankingB != null) rankingB.Text = $"B ({safeB:C0} VND)";
            if (rankingC != null) rankingC.Text = $"C ({safeC:C0} VND)";
        }

        private void OpenRankingEditForm(Button button, CheckBox rankingA, CheckBox rankingB, CheckBox rankingC)
        {
            decimal amountA = 300000m;
            decimal amountB = 275000m;
            decimal amountC = 250000m;

            var parts = (button.Tag?.ToString() ?? "").Split('|');
            if (parts.Length == 3)
            {
                decimal.TryParse(parts[0], out amountA);
                decimal.TryParse(parts[1], out amountB);
                decimal.TryParse(parts[2], out amountC);
            }

            if (amountA <= 0) amountA = 300000m;
            if (amountB <= 0) amountB = 275000m;
            if (amountC <= 0) amountC = 250000m;

            Form editForm = new Form();
            editForm.Text = "Chỉnh Sửa Tiền Ranking";
            editForm.Width = 390;
            editForm.Height = 230;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;
            editForm.BackColor = System.Drawing.Color.White;

            Label labelA = new Label();
            labelA.Text = "Ranking A (VND):";
            labelA.Location = new System.Drawing.Point(30, 25);
            labelA.Width = 130;
            editForm.Controls.Add(labelA);

            TextBox amountABox = new TextBox();
            amountABox.Location = new System.Drawing.Point(170, 22);
            amountABox.Width = 170;
            amountABox.Height = 20;
            amountABox.Font = new System.Drawing.Font("Arial", 9);
            amountABox.BorderStyle = BorderStyle.Fixed3D;
            amountABox.BackColor = System.Drawing.Color.White;
            amountABox.Text = amountA.ToString("0");
            NumberFormatter.FormatNumberInput(amountABox);
            editForm.Controls.Add(amountABox);

            Label labelB = new Label();
            labelB.Text = "Ranking B (VND):";
            labelB.Location = new System.Drawing.Point(30, 65);
            labelB.Width = 130;
            editForm.Controls.Add(labelB);

            TextBox amountBBox = new TextBox();
            amountBBox.Location = new System.Drawing.Point(170, 62);
            amountBBox.Width = 170;
            amountBBox.Height = 20;
            amountBBox.Font = new System.Drawing.Font("Arial", 9);
            amountBBox.BorderStyle = BorderStyle.Fixed3D;
            amountBBox.BackColor = System.Drawing.Color.White;
            amountBBox.Text = amountB.ToString("0");
            NumberFormatter.FormatNumberInput(amountBBox);
            editForm.Controls.Add(amountBBox);

            Label labelC = new Label();
            labelC.Text = "Ranking C (VND):";
            labelC.Location = new System.Drawing.Point(30, 105);
            labelC.Width = 130;
            editForm.Controls.Add(labelC);

            TextBox amountCBox = new TextBox();
            amountCBox.Location = new System.Drawing.Point(170, 102);
            amountCBox.Width = 170;
            amountCBox.Height = 20;
            amountCBox.Font = new System.Drawing.Font("Arial", 9);
            amountCBox.BorderStyle = BorderStyle.Fixed3D;
            amountCBox.BackColor = System.Drawing.Color.White;
            amountCBox.Text = amountC.ToString("0");
            NumberFormatter.FormatNumberInput(amountCBox);
            editForm.Controls.Add(amountCBox);

            Button saveBtn = new Button();
            saveBtn.Text = "💾 Lưu";
            saveBtn.Location = new System.Drawing.Point(130, 145);
            saveBtn.Width = 120;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            saveBtn.Click += (s, e) =>
            {
                string cleanA = amountABox.Text.Replace(",", "");
                string cleanB = amountBBox.Text.Replace(",", "");
                string cleanC = amountCBox.Text.Replace(",", "");

                if (decimal.TryParse(cleanA, out decimal newA) &&
                    decimal.TryParse(cleanB, out decimal newB) &&
                    decimal.TryParse(cleanC, out decimal newC) &&
                    newA >= 0 && newB >= 0 && newC >= 0)
                {
                    button.Tag = $"{newA}|{newB}|{newC}";
                    ApplyRankingCheckboxTexts(rankingA, rankingB, rankingC, newA, newB, newC);

                    try { userDataManager.UpdateRankingBonusAmounts(currentUsername, newA, newB, newC); } catch { }

                    editForm.Close();
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập số tiền hợp lệ (>= 0)!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            editForm.Controls.Add(saveBtn);

            try { Theme.ApplyInfinityGlassTheme(editForm); } catch { }
            editForm.ShowDialog();
        }

        private void OpenInsuranceEditForm(TextBox targetTextBox)
        {
            Form editForm = new Form();
            editForm.Text = "Chỉnh Sửa % Bảo Hiểm";
            editForm.Width = 350;
            editForm.Height = 150;
            editForm.StartPosition = FormStartPosition.CenterParent;
            editForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            editForm.MaximizeBox = false;
            editForm.MinimizeBox = false;
            editForm.BackColor = System.Drawing.Color.White;

            Label label = new Label();
            label.Text = "Phần trăm (%):";
            label.Location = new System.Drawing.Point(30, 30);
            label.Width = 120;
            editForm.Controls.Add(label);

            TextBox percentBox = new TextBox();
            percentBox.Location = new System.Drawing.Point(160, 27);
            percentBox.Width = 150;
            percentBox.Height = 20;
            percentBox.Font = new System.Drawing.Font("Arial", 9);
            percentBox.TextAlign = HorizontalAlignment.Left;
            percentBox.BorderStyle = BorderStyle.Fixed3D;
            percentBox.BackColor = System.Drawing.Color.White;
            percentBox.Text = targetTextBox.Text;
            editForm.Controls.Add(percentBox);

            Button saveBtn = new Button();
            saveBtn.Text = "💾 Lưu";
            saveBtn.Width = 100;
            saveBtn.Height = 35;
            saveBtn.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            saveBtn.BackColor = System.Drawing.Color.Green;
            saveBtn.ForeColor = System.Drawing.Color.White;
            saveBtn.Location = new System.Drawing.Point((editForm.ClientSize.Width - saveBtn.Width) / 2, 80);
            saveBtn.Click += (s, e) =>
            {
                decimal percent = ParsePercent(percentBox.Text, 10.5m);
                if (percent < 0) percent = 0;
                targetTextBox.Text = percent.ToString(CultureInfo.InvariantCulture);
                try { userDataManager.UpdateInsurancePercent(currentUsername, percent); } catch { }
                editForm.Close();
            };
            editForm.Controls.Add(saveBtn);

            try { Theme.ApplyInfinityGlassTheme(editForm); } catch { }
            editForm.ShowDialog();
        }

        private decimal ParsePercent(string text, decimal defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text)) return defaultValue;
            // Try current culture first
            if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out decimal val)) return val;
            // Try invariant with '.' normalized
            string dotNormalized = text.Replace(',', '.');
            if (decimal.TryParse(dotNormalized, NumberStyles.Float, CultureInfo.InvariantCulture, out val)) return val;
            // Try Vietnamese culture with ',' normalized
            string commaNormalized = text.Replace('.', ',');
            var vi = CultureInfo.GetCultureInfo("vi-VN");
            if (decimal.TryParse(commaNormalized, NumberStyles.Float, vi, out val)) return val;
            return defaultValue;
        }

        private Color BlendColor(Color from, Color to, float t)
        {
            if (t < 0f) t = 0f;
            if (t > 1f) t = 1f;

            int a = from.A + (int)((to.A - from.A) * t);
            int r = from.R + (int)((to.R - from.R) * t);
            int g = from.G + (int)((to.G - from.G) * t);
            int b = from.B + (int)((to.B - from.B) * t);

            return Color.FromArgb(a, r, g, b);
        }

        // Typing effect animation for label text
        private void AnimateTypingEffect(Label label, string fullText, int delayPerChar = 15, int charsPerTick = 1, Action onCompleted = null)
        {
            label.Text = "";
            var typingTimer = new System.Windows.Forms.Timer();
            int charIndex = 0;
            int safeCharsPerTick = Math.Max(1, charsPerTick);
            
            typingTimer.Interval = delayPerChar;
            typingTimer.Tick += (s, e) =>
            {
                if (charIndex < fullText.Length)
                {
                    int takeCount = Math.Min(safeCharsPerTick, fullText.Length - charIndex);
                    label.Text += fullText.Substring(charIndex, takeCount);
                    charIndex += takeCount;
                }
                else
                {
                    typingTimer.Stop();
                    try { typingTimer.Dispose(); } catch { }
                    try { onCompleted?.Invoke(); } catch { }
                }
            };
            typingTimer.Start();
        }

        // Compute net salary using the same logic as CalculateSalary but return the net value (no UI changes)
        private decimal ComputeNetSalary(TextBox nameTextBox, TextBox monthTextBox, TextBox yearTextBox, TextBox salaryTextBox, TextBox mealTextBox, TextBox workingDaysTextBox, TextBox slDaysOffTextBox, TextBox alDaysOffTextBox,
                          TextBox overtime2xTextBox, TextBox overtime3xTextBox, TextBox otDays12TextBox, TextBox otDays8TextBox, TextBox overtime15xTextBox, TextBox insuranceTextBox, TextBox taxTextBox,
                          TextBox allowanceTextBox, TextBox recognizeTextBox, TextBox otherBonusTextBox, TextBox taxThresholdTextBox)
        {
            try
            {
                decimal basicSalary = decimal.Parse(salaryTextBox.Text);
                decimal mealAllowancePerDay = decimal.Parse(mealTextBox.Text);
                decimal workingDays = decimal.Parse(workingDaysTextBox.Text);
                decimal slDaysOff = decimal.Parse(slDaysOffTextBox.Text);
                decimal alDaysOff = decimal.Parse(alDaysOffTextBox.Text);
                decimal overtime2xHours = decimal.Parse(overtime2xTextBox.Text);
                decimal overtime3xHours = decimal.Parse(overtime3xTextBox.Text);
                decimal otDays12 = decimal.Parse(otDays12TextBox.Text);
                decimal otDays8 = decimal.Parse(otDays8TextBox.Text);
                decimal overtime15xHours = decimal.Parse(overtime15xTextBox.Text);
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

                decimal payableDays = workingDays - slDaysOff;
                decimal allowanceEligibleDays = workingDays - slDaysOff - alDaysOff;
                if (payableDays < 0) payableDays = 0;
                if (allowanceEligibleDays < 0) allowanceEligibleDays = 0;

                decimal travelPerDay = 8500m;
                decimal attendancePerDay = 8500m;
                decimal rankingAAmount = 300000m;
                decimal rankingBAmount = 275000m;
                decimal rankingCAmount = 250000m;
                // Calculate new incentive components
                decimal travelAllowance = 0m;
                decimal attendanceIncentive = 0m;
                decimal housingAllowance = 100000m;
                decimal ratingBonus = 0m;
                decimal certificateBonus = 0m;

                // Get user data to retrieve rating and certificate
                var user = userDataManager.Login(currentUsername);
                if (user != null)
                {
                    travelPerDay = user.TravelAllowance > 0 ? user.TravelAllowance : 8500m;
                    attendancePerDay = user.AttendancePerDay > 0 ? user.AttendancePerDay : 8500m;
                    housingAllowance = user.HousingAllowance > 0 ? user.HousingAllowance : 100000m;
                    rankingAAmount = user.RankingABonusAmount > 0 ? user.RankingABonusAmount : rankingAAmount;
                    rankingBAmount = user.RankingBBonusAmount > 0 ? user.RankingBBonusAmount : rankingBAmount;
                    rankingCAmount = user.RankingCBonusAmount > 0 ? user.RankingCBonusAmount : rankingCAmount;
                    // Calculate rating bonus
                    if (user.RatingBonus == "A") ratingBonus = rankingAAmount;
                    else if (user.RatingBonus == "B") ratingBonus = rankingBAmount;
                    else if (user.RatingBonus == "C") ratingBonus = rankingCAmount;

                    certificateBonus = user.CertificateBonus;
                }

                travelAllowance = travelPerDay * allowanceEligibleDays;
                attendanceIncentive = attendancePerDay * allowanceEligibleDays;

                decimal bonusMealAllowance = 0;
                if (otDays12 > 0) bonusMealAllowance += otDays12 * meal12Amount;
                if (otDays8 > 0) bonusMealAllowance += otDays8 * meal8Amount;

                decimal basicDailySalary = basicSalary / workingDays;
                decimal mealDailySalary = mealAllowancePerDay / workingDays;
                decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;
                // Round to 3 decimal places for exact calculation
                decimal hourlyRate = Math.Round(basicDailySalary / 8, 3, MidpointRounding.AwayFromZero);

                decimal insurancePercent = 10.5m;
                if (insuranceTextBox != null)
                {
                    insurancePercent = ParsePercent(insuranceTextBox.Text, 10.5m);
                }
                if (insurancePercent < 0) insurancePercent = 0;
                decimal insuranceDeduction = Math.Round(basicSalary * (insurancePercent / 100m), 0, MidpointRounding.AwayFromZero);
                decimal taxThreshold = ComputeTaxThreshold(baseTaxThresholdInput, hourlyRate, overtime2xHours, overtime3xHours, insuranceDeduction);

                decimal regularSalary = payableDays * dailySalaryForMeal;
                decimal overtime2xSalary = Math.Round(overtime2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal overtime3xSalary = Math.Round(overtime3xHours * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                decimal overtime15xSalary = Math.Round(overtime15xHours * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                decimal totalIncentive = travelAllowance + attendanceIncentive + housingAllowance + ratingBonus + certificateBonus + (recognizeCount * 50000) + otherBonus;
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

        private void PlayApplauseEmbedded()
        {
            try
            {
                bool played = false;
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceCandidates = new[]
                {
                    asm.GetName().Name + ".Assets.audio.clap.wav",
                    asm.GetName().Name + ".Assets.audio.applause.wav"
                };

                foreach (var resName in resourceCandidates)
                {
                    using (var rs = asm.GetManifestResourceStream(resName))
                    {
                        if (rs == null) continue;
                        try
                        {
                            string tmp = Path.Combine(Path.GetTempPath(), "applause_" + Guid.NewGuid().ToString("N") + ".wav");
                            using (var fs = File.Create(tmp)) { rs.CopyTo(fs); }
                            var sp = new System.Media.SoundPlayer(tmp);
                            sp.Play();
                            _ = System.Threading.Tasks.Task.Run(async () =>
                            {
                                await System.Threading.Tasks.Task.Delay(12000);
                                try { File.Delete(tmp); } catch { }
                            });
                            played = true;
                            break;
                        }
                        catch
                        {
                        }
                    }
                }

                if (!played)
                {
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;
                    var fileCandidates = new[]
                    {
                        Path.Combine(appDir, "Assets", "audio", "clap.wav"),
                        Path.Combine(appDir, "Assets", "audio", "applause.wav")
                    };

                    foreach (var path in fileCandidates)
                    {
                        try
                        {
                            if (!File.Exists(path)) continue;
                            using (var sp = new System.Media.SoundPlayer(path))
                            {
                                sp.Play();
                            }
                            played = true;
                            break;
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private string GetTop5RankingText()
        {
            try
            {
                int currentMonth = DateTime.Now.Month;
                int currentYear = DateTime.Now.Year;
                
                var users = userDataManager.GetAllUsers();
                var topUsers = users
                    .Where(u => u.LastCalculatedMonth == currentMonth && u.LastCalculatedYear == currentYear && u.LastNetSalary > 0)
                    .OrderByDescending(u => u.LastNetSalary)
                    .Take(5)
                    .ToList();
                
                if (topUsers.Count == 0)
                {
                    return "     BẢNG XẾP HẠNG LƯƠNG THÁNG " + currentMonth.ToString("D2") + "/" + currentYear + " - Chưa có dữ liệu     ";
                }
                
                var rankings = new List<string>();
                for (int i = 0; i < topUsers.Count; i++)
                {
                    string topLabel = "";
                    if (i == 0)
                    {
                        topLabel = "Top1";
                    }
                    else if (i == 1)
                    {
                        topLabel = "Top2";
                    }
                    else if (i == 2)
                    {
                        topLabel = "Top3";
                    }
                    else
                    {
                        topLabel = $"Top{i + 1}";
                    }
                    
                    rankings.Add($"{topLabel}: {topUsers[i].FullName} ({topUsers[i].LastNetSalary:N0} VND)");
                }
                
                return "     " + string.Join("     |     ", rankings) + "     ";
            }
            catch
            {
                return "     BẢNG XẾP HẠNG LƯƠNG     ";
            }
        }

    }
}
