using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace SalaryCalculator
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class WebBackendBridge
    {
        private UserDataManager _dataManager;
        
        public WebBackendBridge()
        {
            _dataManager = new UserDataManager();
        }

        public string Login(string username)
        {
            try
            {
                var user = _dataManager.Login(username);
                if (user != null)
                {
                    return JsonSerializer.Serialize(new { success = true, user = user });
                }
                
                return JsonSerializer.Serialize(new { success = false, message = "Tài khoản chưa được đăng ký", needsRegistration = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        public string RegisterUser(string payloadJson)
        {
            try
            {
                using (var doc = JsonDocument.Parse(payloadJson))
                {
                    var root = doc.RootElement;
                    string username = root.GetProperty("username").GetString();
                    
                    var user = new UserInfo
                    {
                        Username = username,
                        FullName = root.GetProperty("fullName").GetString(),
                        BasicSalary = root.GetProperty("basicSalary").GetDecimal(),
                        MealAllowance = root.GetProperty("mealAllowance").GetDecimal(),
                        TravelAllowance = root.GetProperty("travelAllowance").GetDecimal(),
                        HousingAllowance = root.GetProperty("housingAllowance").GetDecimal(),
                        AttendanceIncentive = root.GetProperty("attendanceIncentive").GetDecimal(),
                        CertificateBonus = root.GetProperty("certificateBonus").GetDecimal(),
                        Allowance = root.GetProperty("otherBonus").GetDecimal(),
                        InsurancePercent = root.GetProperty("insurancePercent").GetDecimal(),
                        TaxThreshold = root.GetProperty("taxThreshold").GetDecimal()
                    };
                    
                    _dataManager.SaveUser(user);
                    return JsonSerializer.Serialize(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        public string UpdateProfile(string payloadJson)
        {
            try
            {
                using (var doc = JsonDocument.Parse(payloadJson))
                {
                    var root = doc.RootElement;
                    string username = root.GetProperty("username").GetString();
                    
                    var user = _dataManager.Login(username);
                    if (user == null) return JsonSerializer.Serialize(new { success = false, message = "User not found" });
                    
                    user.FullName = root.GetProperty("fullName").GetString();
                    user.BasicSalary = root.GetProperty("basicSalary").GetDecimal();
                    user.MealAllowance = root.GetProperty("mealAllowance").GetDecimal();
                    user.TravelAllowance = root.GetProperty("travelAllowance").GetDecimal();
                    user.HousingAllowance = root.GetProperty("housingAllowance").GetDecimal();
                    user.AttendanceIncentive = root.GetProperty("attendanceIncentive").GetDecimal();
                    user.CertificateBonus = root.GetProperty("certificateBonus").GetDecimal();
                    user.Allowance = root.GetProperty("otherBonus").GetDecimal();
                    user.InsurancePercent = root.GetProperty("insurancePercent").GetDecimal();
                    user.TaxThreshold = root.GetProperty("taxThreshold").GetDecimal();
                    
                    if (root.TryGetProperty("performanceBonus", out var pb)) user.PerformanceBonus = pb.GetDecimal();
                    if (root.TryGetProperty("perfDeduct1", out var pd1)) user.PerfDeduct1 = pd1.GetDecimal();
                    if (root.TryGetProperty("perfDeduct2", out var pd2)) user.PerfDeduct2 = pd2.GetDecimal();
                    if (root.TryGetProperty("otMeal12Amount", out var ot12)) user.OtMeal12Amount = ot12.GetDecimal();
                    if (root.TryGetProperty("otMeal8Amount", out var ot8)) user.OtMeal8Amount = ot8.GetDecimal();
                    
                    _dataManager.SaveUser(user);
                    return JsonSerializer.Serialize(new { success = true, user = user });
                }
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        private decimal ComputeEffectiveMonthlyAllowance(decimal monthlyBase, decimal workingDays)
        {
            monthlyBase = Math.Max(0m, monthlyBase);

            if (workingDays > 23m)
            {
                monthlyBase += 8500m * (workingDays - 23m);
            }

            return monthlyBase;
        }

        private decimal ComputeProratedAllowanceByWorkedDays(decimal monthlyBase, decimal workingDays, decimal actualWorkedDays)
        {
            if (workingDays <= 0 || actualWorkedDays <= 0) return 0m;
            decimal effectiveMonthly = ComputeEffectiveMonthlyAllowance(monthlyBase, workingDays);
            decimal prorated = (effectiveMonthly / workingDays) * actualWorkedDays;
            return Math.Round(prorated, 0, MidpointRounding.AwayFromZero);
        }

        private decimal ComputeTaxThreshold(decimal baseThreshold, decimal hourlyRate, decimal otFwdHours, decimal ot2xHours, decimal overtime3xHours, decimal overtime15xHours, decimal insuranceDeduction)
        {
            decimal ot2xSalary = Math.Round(ot2xHours * hourlyRate * 2m, 0, MidpointRounding.AwayFromZero);
            decimal ot3xSalary = Math.Round(overtime3xHours * hourlyRate * 3m, 0, MidpointRounding.AwayFromZero);
            decimal ot15xSalary = Math.Round(overtime15xHours * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

            // Phần tiền OT được MIỄN THUẾ theo Luật Thuế TNCN (Thông tư 111/2013/TT-BTC):
            // OT 1.5x ➔ 0.5x miễn thuế (1/3 tổng tiền OT 1.5x)
            // OT 2.0x ➔ 1.0x miễn thuế (1/2 tổng tiền OT 2.0x)
            // OT 3.0x ➔ 2.0x miễn thuế (2/3 tổng tiền OT 3.0x)
            decimal additionalOTx2 = ot2xSalary / 2m; 
            decimal additionalOTx3 = ot3xSalary * 2m / 3m; 
            decimal additionalOTx15 = ot15xSalary / 3m;
            decimal FixedThresholdAddon = 730000m; // Miễn thuế phụ cấp ăn trưa

            return baseThreshold + insuranceDeduction + FixedThresholdAddon + additionalOTx2 + additionalOTx3 + additionalOTx15;
        }

        public string CalculateSalary(string payloadJson)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<CalcPayload>(payloadJson);
                
                decimal workingDays = payload.workingDays > 0 ? payload.workingDays : 1;
                
                var user = !string.IsNullOrEmpty(payload.username) ? _dataManager.Login(payload.username) : null;

                decimal meal12Amount = payload.otMeal12Amount > 0 ? payload.otMeal12Amount : (user != null && user.OtMeal12Amount > 0 ? user.OtMeal12Amount : 30000m);
                decimal meal8Amount = payload.otMeal8Amount > 0 ? payload.otMeal8Amount : (user != null && user.OtMeal8Amount > 0 ? user.OtMeal8Amount : 20000m);
                decimal bonusMealAllowance = 0;
                if (payload.otDays12 > 0) bonusMealAllowance += payload.otDays12 * meal12Amount;
                if (payload.otDays8 > 0) bonusMealAllowance += payload.otDays8 * meal8Amount;

                decimal basicDailySalary = payload.basicSalary / workingDays;
                decimal mealDailySalary = payload.mealAllowance / workingDays;
                decimal dailySalaryForMeal = basicDailySalary + mealDailySalary;

                decimal hourlyRate = Math.Round(basicDailySalary / 8, 3, MidpointRounding.AwayFromZero);

                decimal insurancePercent = payload.insurancePercent < 0 ? 0 : payload.insurancePercent;
                decimal insuranceDeduction = Math.Round(payload.basicSalary * (insurancePercent / 100m), 0, MidpointRounding.AwayFromZero);

                decimal otFwdHours = 0;
                decimal ot2xHours = payload.overtime2x;

                decimal taxThreshold = ComputeTaxThreshold(payload.taxThreshold, hourlyRate, otFwdHours, ot2xHours, payload.overtime3x, payload.overtime15x, insuranceDeduction);

                decimal slSalaryDeduction = payload.slDaysOff * dailySalaryForMeal;
                decimal baseRegularSalary = workingDays * dailySalaryForMeal;
                decimal regularSalary = baseRegularSalary - slSalaryDeduction;
                if (regularSalary < 0) regularSalary = 0;

                decimal otFwdSalary = Math.Round(otFwdHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal ot2xSalary = Math.Round(ot2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                decimal overtime2xSalary = otFwdSalary + ot2xSalary;

                decimal overtime3xSalary = Math.Round(payload.overtime3x * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                decimal overtime15xSalary = Math.Round(payload.overtime15x * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                decimal allowanceEligibleDays = workingDays - payload.slDaysOff - payload.alDaysOff;
                if (allowanceEligibleDays < 0) allowanceEligibleDays = 0;

                decimal travelAllowance = ComputeProratedAllowanceByWorkedDays(payload.travelAllowance, workingDays, allowanceEligibleDays);
                decimal attendanceIncentive = ComputeProratedAllowanceByWorkedDays(payload.attendanceIncentive, workingDays, allowanceEligibleDays);

                decimal leaveDays = payload.slDaysOff + payload.alDaysOff;
                decimal actualPerformanceBonus = payload.performanceBonus;
                if (leaveDays > 0 && leaveDays <= 1) {
                    actualPerformanceBonus -= payload.perfDeduct1;
                } else if (leaveDays > 1 && leaveDays <= 2) {
                    actualPerformanceBonus -= payload.perfDeduct2;
                } else if (leaveDays > 2) {
                    actualPerformanceBonus = 0;
                }
                if (actualPerformanceBonus < 0) actualPerformanceBonus = 0;

                decimal totalIncentive = travelAllowance + attendanceIncentive + payload.housingAllowance + payload.certificateBonus + payload.otherBonus + actualPerformanceBonus;

                decimal grossSalary = regularSalary + overtime2xSalary + overtime3xSalary + overtime15xSalary + bonusMealAllowance + totalIncentive;
                
                decimal taxBase = grossSalary - taxThreshold;
                decimal taxRate = 0;
                if (taxBase <= 0) taxRate = 0;
                else if (taxBase > 0 && taxBase <= 10000000) taxRate = 0.05m;
                else if (taxBase > 10000000 && taxBase <= 30000000) taxRate = 0.10m;
                else if (taxBase > 30000000 && taxBase <= 60000000) taxRate = 0.20m;
                else if (taxBase > 60000000 && taxBase <= 100000000) taxRate = 0.30m;
                else if (taxBase > 100000000) taxRate = 0.35m;

                decimal taxDeduction = taxBase > 0 ? Math.Floor(taxBase * taxRate) : 0;
                decimal netSalary = grossSalary - insuranceDeduction - taxDeduction;

                decimal net = netSalary;
                decimal gross = grossSalary;

                if (payload.month > 0)
                {
                    var detailObject = new {
                        gross = gross,
                        net = net,
                        tax = taxDeduction,
                        insurance = insuranceDeduction,
                        basicSalary = payload.basicSalary,
                        workingDays = payload.workingDays,
                        alDaysOff = payload.alDaysOff,
                        slDaysOff = payload.slDaysOff,
                        slDeduction = slSalaryDeduction,
                        overtime15x = payload.overtime15x,
                        overtime2x = payload.overtime2x,
                        overtime3x = payload.overtime3x,
                        overtime15xSalary = overtime15xSalary,
                        overtime2xSalary = overtime2xSalary,
                        overtime3xSalary = overtime3xSalary,
                        otDays8 = payload.otDays8,
                        otDays12 = payload.otDays12,
                        otMeal8Amount = meal8Amount,
                        otMeal12Amount = meal12Amount,
                        bonusMeal = bonusMealAllowance,
                        mealAllowance = payload.mealAllowance,
                        travelAllowance = travelAllowance,
                        housingAllowance = payload.housingAllowance,
                        attendanceIncentive = attendanceIncentive,
                        certificateBonus = payload.certificateBonus,
                        performanceBonus = actualPerformanceBonus,
                        otherBonus = payload.otherBonus
                    };
                    string detailJson = JsonSerializer.Serialize(detailObject);
                    _dataManager.UpdateLastCalculation(payload.username, payload.month, payload.year, net, detailJson);
                }

                return JsonSerializer.Serialize(new { success = true, gross = gross, tax = taxDeduction, insurance = insuranceDeduction, net = net });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }
        public string GetSalaryHistory(string username)
        {
            try
            {
                var user = _dataManager.Login(username);
                if (user == null || user.SalaryHistory == null)
                    return JsonSerializer.Serialize(new { success = true, history = new object[0] });

                var historyList = new System.Collections.Generic.List<object>();
                bool needsSave = false;
                var keysToRemove = new System.Collections.Generic.List<string>();
                
                foreach (var entry in user.SalaryHistory)
                {
                    if (entry.Key.StartsWith("00/") || entry.Key.StartsWith("0/") || 
                        entry.Key.StartsWith("00-") || entry.Key.StartsWith("0-"))
                    {
                        keysToRemove.Add(entry.Key);
                        needsSave = true;
                    }
                    else
                    {
                        string detail = null;
                        if (user.SalaryResultHistory != null && user.SalaryResultHistory.ContainsKey(entry.Key))
                        {
                            detail = user.SalaryResultHistory[entry.Key];
                        }
                        historyList.Add(new { period = entry.Key, netSalary = entry.Value, detail = detail });
                    }
                }
                
                if (needsSave)
                {
                    foreach(var k in keysToRemove) 
                    {
                        user.SalaryHistory.Remove(k);
                        if (user.SalaryResultHistory != null)
                            user.SalaryResultHistory.Remove(k);
                    }
                    _dataManager.SaveUser(user);
                }
                
                return JsonSerializer.Serialize(new { success = true, history = historyList });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        public string DeleteSalaryHistoryEntry(string username, string period)
        {
            try
            {
                var user = _dataManager.Login(username);
                if (user == null)
                    return JsonSerializer.Serialize(new { success = false, message = "Không tìm thấy người dùng" });

                bool removed = false;
                if (user.SalaryHistory != null && user.SalaryHistory.ContainsKey(period))
                {
                    user.SalaryHistory.Remove(period);
                    removed = true;
                }
                if (user.SalaryResultHistory != null && user.SalaryResultHistory.ContainsKey(period))
                {
                    user.SalaryResultHistory.Remove(period);
                    removed = true;
                }

                if (removed)
                {
                    _dataManager.SaveUser(user);
                }

                return JsonSerializer.Serialize(new { success = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        public string GetRanking(int month, int year)
        {
            try
            {
                var users = _dataManager.GetAllUsers();
                string key = $"{month:D2}-{year}";
                
                var sorted = users
                    .Where(u => u.SalaryHistory != null && u.SalaryHistory.ContainsKey(key) && u.SalaryHistory[key] > 0)
                    .OrderByDescending(u => u.SalaryHistory[key])
                    .ToList();
                
                var rankingList = new System.Collections.Generic.List<object>();
                int rank = 1;
                foreach (var u in sorted)
                {
                    decimal netSalary = u.SalaryHistory[key];
                    string displayName = string.IsNullOrEmpty(u.FullName) ? u.Username : u.FullName;
                    string comment = GetCeoReviewComment(rank, displayName, netSalary);
                    rankingList.Add(new { rank = rank, name = displayName, netSalary = netSalary, comment = comment });
                    rank++;
                }
                return JsonSerializer.Serialize(new { success = true, ranking = rankingList });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        private static string GetCeoReviewComment(int rank, string name, decimal netSalary)
        {
            int seed = Math.Abs((name ?? "").GetHashCode() ^ DateTime.Now.DayOfYear ^ (rank * 101) ^ ((int)(netSalary % 997)));
            Random rnd = new Random(seed);

            if (rank == 1)
            {
                string[] openers = { "👑 Thư ký đâu,", "💼 Đòn bẩy tài chính,", "🚀 Out trình tuyệt đối,", "💎 Phong độ Tổng tài,", "🔥 Gánh cả tập đoàn," };
                string[] verbs = { "bứt phá doanh thu,", "cống hiến vượt ngưỡng,", "tạo kỷ lục thu nhập,", "bùng nổ chỉ số," };
                string[] closers = { "duyệt thưởng nóng ngay!", "ghế Phó TGĐ là của em!", "book vé resort đặc quyền!", "xứng danh Quán quân!" };

                return $"{openers[rnd.Next(openers.Length)]} {verbs[rnd.Next(verbs.Length)]} {closers[rnd.Next(closers.Length)]}";
            }
            if (rank == 2)
            {
                string[] openers = { "⚡ Tay phải đắc lực,", "🚀 Năng suất thần tốc,", "💎 Trụ cột chiến lược,", "🎯 Chiến thần KPI,", "🌟 Á Quân đỉnh cao," };
                string[] verbs = { "chạy đua cực bản lĩnh,", "áp sát vị trí Top 1,", "cống hiến ấn tượng,", "tăng tốc vượt đối thủ," };
                string[] closers = { "tháng sau cướp ngôi vương nhé!", "1 bước nữa tới đỉnh cao!", "duyệt thưởng lớn tháng này!", "Tổng tài rất tự hào!" };

                return $"{openers[rnd.Next(openers.Length)]} {verbs[rnd.Next(verbs.Length)]} {closers[rnd.Next(closers.Length)]}";
            }
            if (rank == 3)
            {
                string[] openers = { "✨ Vào Top 3 VIP,", "🌟 Làm việc rất có gu,", "🎯 Quyết đoán bản lĩnh,", "🔥 Quý Quân kiệt xuất,", "💼 Phong thái tinh anh," };
                string[] verbs = { "khẳng định vị thế,", "bứt phá vào hàng ngũ sao,", "xử lý dự án cực mượt,", "tạo ấn tượng mạnh mẽ," };
                string[] closers = { "chuẩn bị nhận dự án lớn!", "thưởng quý này cực ấm!", "tương lai cực rộng mở!", "giữ vững đà thăng tiến!" };

                return $"{openers[rnd.Next(openers.Length)]} {verbs[rnd.Next(verbs.Length)]} {closers[rnd.Next(closers.Length)]}";
            }
            if (rank <= 5)
            {
                string[] openers = { "📈 Tiềm năng cực lớn,", "☕ Thể hiện rất chuẩn chỉ,", "⚡ Nhịp độ rất ổn định,", "💼 Ứng viên tiềm năng," };
                string[] verbs = { "đang giấu 50% công lực,", "bám đuổi Top 3 sát nút,", "tích lũy phong độ bùng nổ,", "duy trì hiệu suất tốt," };
                string[] closers = { "đừng để Top 3 ngủ quên!", "tháng sau vượt Top 3 nhé!", "bứt phá để tăng thu nhập!", "tăng tốc lên Top 3 ngay!" };

                return $"{openers[rnd.Next(openers.Length)]} {verbs[rnd.Next(verbs.Length)]} {closers[rnd.Next(closers.Length)]}";
            }

            string[] restOpeners = { "⏳ Hơi trầm đấy,", "💡 Tài năng có thừa,", "🔥 Năng lượng lên nào,", "⚡ Cần tăng tốc ngay," };
            string[] restVerbs = { "chưa bung hết công lực,", "đang đợi ngày bùng nổ,", "cần chủ động đột phá,", "vẫn giấu nghề đúng không," };
            string[] restClosers = { "đừng để Tổng tài thất vọng!", "bứt phá ở tháng tới nhé!", "muốn tăng lương phải cháy!", "cơ hội luôn rộng mở!" };

            return $"{restOpeners[rnd.Next(restOpeners.Length)]} {restVerbs[rnd.Next(restVerbs.Length)]} {restClosers[rnd.Next(restClosers.Length)]}";
        }
        // DTO for payload
        private class CalcPayload
        {
            public string username { get; set; }
            public int month { get; set; }
            public int year { get; set; }
            public decimal workingDays { get; set; }
            public decimal basicSalary { get; set; }
            public decimal mealAllowance { get; set; }
            public decimal travelAllowance { get; set; }
            public decimal housingAllowance { get; set; }
            public decimal allowance { get; set; }
            public decimal otherBonus { get; set; }
            public decimal alDaysOff { get; set; }
            public decimal slDaysOff { get; set; }
            public decimal leaveDays { get; set; }
            public decimal overtime15x { get; set; }
            public decimal overtime2x { get; set; }
            public decimal overtime3x { get; set; }
            public decimal otDays8 { get; set; }
            public decimal otDays12 { get; set; }
            public decimal attendanceIncentive { get; set; }
            public decimal certificateBonus { get; set; }
            public int recognizeCount { get; set; }
            public string ratingBonus { get; set; }
            public decimal insurancePercent { get; set; }
            public decimal taxThreshold { get; set; }
            public decimal performanceBonus { get; set; }
            public decimal perfDeduct1 { get; set; }
            public decimal perfDeduct2 { get; set; }
            public decimal otMeal8Amount { get; set; }
            public decimal otMeal12Amount { get; set; }
        }
    }
}
