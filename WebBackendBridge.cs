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
                var usedComments = new System.Collections.Generic.HashSet<string>();
                int rank = 1;
                foreach (var u in sorted)
                {
                    decimal netSalary = u.SalaryHistory[key];
                    string displayName = string.IsNullOrEmpty(u.FullName) ? u.Username : u.FullName;
                    string comment = GetCeoReviewComment(rank, displayName, netSalary, usedComments, month, year);
                    usedComments.Add(comment);
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

        private static string GetCeoReviewComment(int rank, string name, decimal netSalary, System.Collections.Generic.HashSet<string> usedComments, int month, int year)
        {
            string[] top1Pool = {
                "👑 Quán Quân Thu Nhập: Out trình hoàn toàn, xứng danh trụ cột tài chính của công ty!",
                "🏆 Ngôi Sao Bùng Nổ: Doanh số & thu nhập tạo kỷ lục mới, Tổng tài duyệt thưởng nóng!",
                "💎 Đỉnh Cao Phong Độ: Gánh team cực đỉnh, ghế Phó Tổng Giám Đốc đang chờ sẵn!",
                "🔥 Chiến Thần Bứt Phá: Thu nhập bùng nổ vượt ngưỡng, phong thái Tổng tài chính hiệu!",
                "🚀 Kỷ Lục Gia Thu Nhập: Bứt phá tuyệt đối, Thư ký đâu book ngay resort 5 sao chúc mừng!"
            };

            string[] top2Pool = {
                "🥈 Á Quân Xuất Sắc: Năng suất thần tốc, bám đuổi Top 1 cực kỳ bản lĩnh!",
                "⚡ Tay Phải Đắc Lực: Cống hiến ấn tượng, chỉ thiếu 1 bước nữa là cướp ngôi vương!",
                "🎯 Chiến Thần KPI: Phong độ tăng trưởng ổn định, tháng sau quyết tâm lên Top 1!",
                "🌟 Trụ Cột Chiến Lược: Xử lý khối lượng công việc cực mượt, Tổng tài rất tự hào!",
                "💼 Tinh Anh Công Ty: Hiệu suất bứt phá mạnh mẽ, duyệt thưởng lớn tháng này!"
            };

            string[] top3Pool = {
                "🥉 Quý Quân Bản Lĩnh: Khẳng định vị thế trong Top 3 VIP, phong thái rất có gu!",
                "✨ Ngôi Sao Bứt Phá: Tấn công Top 3 cực kỳ ngoạn mục, tương lai vô cùng rộng mở!",
                "🔥 Phong Độ Thăng Hoa: Xử lý dự án sắc bén, chuẩn bị nhận dự án lớn tháng tới!",
                "🌟 Nhân Tố Chủ Lực: Duy trì nhịp độ làm việc tuyệt vời, thưởng quý này cực ấm!",
                "💪 Chiến Binh Kiệt Xuất: Bứt phá ấn tượng vào hàng ngũ VIP, giữ vững đà tiến nhé!"
            };

            string[] top5Pool = {
                "📈 Tiềm Năng Bùng Nổ: Đang giấu 50% công lực, bám đuổi Top 3 rất sát nút!",
                "☕ Tinh Anh Tiềm Năng: Nhịp độ làm việc rất chuẩn chỉ, tháng sau vượt Top 3 ngay!",
                "⚡ Nhân Tố Đột Phá: Tích lũy phong độ bùng nổ, cơ hội thăng tiến đang rất gần!",
                "🔥 Ứng Viên Sáng Giá: Đừng để Top 3 ngủ quên, bứt phá mạnh mẽ ở tháng tới nhé!"
            };

            string[] generalPool = {
                "⏳ Đang Tích Lũy Năng Lượng: Tài năng có thừa, chờ ngày bung hết 100% công lực!",
                "💡 Nhân Tố Ẩn Số: Cần chủ động đột phá hơn nữa, muốn tăng thu nhập phải cháy!",
                "⚡ Nhịp Độ Ổn Định: Phong độ đang đi lên, kiên trì bứt phá ở chặng đua tiếp theo!",
                "🎯 Mục Tiêu Phía Trước: Đang giấu nghề đúng không, cơ hội bùng nổ luôn rộng mở!",
                "🚀 Năng Lượng Đang Tăng: Cố gắng duy trì phong độ, tháng sau chắc chắn bùng nổ!"
            };

            string[] candidatePool = top1Pool;
            if (rank == 1) candidatePool = top1Pool;
            else if (rank == 2) candidatePool = top2Pool;
            else if (rank == 3) candidatePool = top3Pool;
            else if (rank <= 5) candidatePool = top5Pool;
            else candidatePool = generalPool;

            // Pick a dynamic non-duplicate comment seeded by month, year, rank, name, and salary
            int seed = Math.Abs((month * 10007 + year * 31) ^ (rank * 101) ^ (name ?? "").GetHashCode() ^ ((int)(netSalary % 997)));
            int startIndex = seed % candidatePool.Length;

            for (int i = 0; i < candidatePool.Length; i++)
            {
                int idx = (startIndex + i) % candidatePool.Length;
                string option = candidatePool[idx];
                if (!usedComments.Contains(option))
                {
                    return option;
                }
            }

            // Fallback: Generate custom personalized non-duplicate comment
            string fallback = rank switch {
                1 => $"👑 Quán Quân {name}: Thu nhập ấn tượng {netSalary:N0} VNĐ, dẫn đầu tuyệt đối!",
                2 => $"🥈 Á Quân {name}: Thu nhập {netSalary:N0} VNĐ, tay phải đắc lực của công ty!",
                3 => $"🥉 Quý Quân {name}: Thu nhập {netSalary:N0} VNĐ, vững vàng trong Top 3 VIP!",
                _ => $"🌟 {name}: Đạt vị trí #{rank} với thu nhập {netSalary:N0} VNĐ, cố gắng ở tháng tới!"
            };

            if (!usedComments.Contains(fallback)) return fallback;
            return $"{fallback} ✨";
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
