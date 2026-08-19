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

        public static DateTime GetPayDate(int year, int month)
        {
            int lastDay = DateTime.DaysInMonth(year, month);
            DateTime payDate = new DateTime(year, month, lastDay);
            if (payDate.DayOfWeek == DayOfWeek.Saturday)
            {
                payDate = payDate.AddDays(-1);
            }
            else if (payDate.DayOfWeek == DayOfWeek.Sunday)
            {
                payDate = payDate.AddDays(-2);
            }
            return payDate.Date;
        }

        public string GetCurrentPayrollPeriod()
        {
            try
            {
                DateTime now = DateTime.Now;
                int month = now.Month;
                int year = now.Year;

                // Quy tắc chọn tháng tự động:
                // Ngày trả lương là ngày cuối tháng (30, 31). Nếu rơi vào T7, CN thì được trả trước vào T6.
                // Cho tới hết ngày trả lương thì vẫn tính/hiển thị lương của tháng hiện tại.
                // Sau ngày trả lương mới chuyển sang tháng tiếp theo.
                DateTime payDate = GetPayDate(year, month);
                if (now.Date > payDate.Date)
                {
                    month += 1;
                    if (month > 12)
                    {
                        month = 1;
                        year += 1;
                    }
                }

                return JsonSerializer.Serialize(new { success = true, month = month, year = year });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
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

        public string SaveRawUserJson(string username, string userJson)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(userJson))
                    return JsonSerializer.Serialize(new { success = false, message = "Invalid parameters" });

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var user = JsonSerializer.Deserialize<UserInfo>(userJson, options);

                if (user == null)
                {
                    user = new UserInfo { Username = username };
                }
                else if (string.IsNullOrEmpty(user.Username))
                {
                    user.Username = username;
                }

                bool saved = _dataManager.SaveUser(user);
                return JsonSerializer.Serialize(new { success = saved });
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

        private (decimal W1, decimal W2) GetMidMonthWorkingDays(int month, int year)
        {
            if (month <= 0 || year <= 0) return (10m, 12m);
            int m1 = month == 1 ? 12 : month - 1;
            int y1 = month == 1 ? year - 1 : year;
            int daysInPrevMonth = DateTime.DaysInMonth(y1, m1);

            decimal w1 = 0;
            if (daysInPrevMonth >= 21)
            {
                DateTime start1 = new DateTime(y1, m1, 21);
                DateTime end1 = new DateTime(y1, m1, daysInPrevMonth);
                for (DateTime d = start1; d <= end1; d = d.AddDays(1))
                {
                    if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                        w1++;
                }
            }

            decimal w2 = 0;
            DateTime start2 = new DateTime(year, month, 1);
            DateTime end2 = new DateTime(year, month, 20);
            for (DateTime d = start2; d <= end2; d = d.AddDays(1))
            {
                if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday)
                    w2++;
            }

            return (w1, w2);
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
                
                // Variables for mid-month calculation or standard calculation
                decimal insuranceDeduction = 0;
                decimal slSalaryDeduction = 0;
                decimal regularSalary = 0;
                decimal overtime15xSalary = 0;
                decimal overtime2xSalary = 0;
                decimal overtime3xSalary = 0;
                decimal allowanceEligibleDays = 0;

                // Mid-month breakdown variables for detailObject
                decimal w1 = 0, w2 = 0;
                decimal workedDays1 = 0, workedDays2 = 0;
                decimal basicDaily1 = 0, basicDaily2 = 0;
                decimal ot15xSalary1 = 0, ot2xSalary1 = 0, ot3xSalary1 = 0;
                decimal ot15xSalary2 = 0, ot2xSalary2 = 0, ot3xSalary2 = 0;
                decimal slSalaryDeduction1 = 0, slSalaryDeduction2 = 0;

                if (payload.isMidMonthSalaryChange)
                {
                    var (midW1, midW2) = GetMidMonthWorkingDays(payload.month, payload.year);
                    w1 = midW1;
                    w2 = midW2;
                    decimal totalWorkingDaysMid = (w1 + w2) > 0 ? (w1 + w2) : workingDays;

                    decimal oldBasic = payload.oldBasicSalary > 0 ? payload.oldBasicSalary : payload.basicSalary;
                    decimal newBasic = payload.newBasicSalary > 0 ? payload.newBasicSalary : payload.basicSalary;

                    basicDaily1 = oldBasic / totalWorkingDaysMid;
                    basicDaily2 = newBasic / totalWorkingDaysMid;
                    decimal mealDailyMid = payload.mealAllowance / totalWorkingDaysMid;

                    decimal dailyRate1 = basicDaily1 + mealDailyMid;
                    decimal dailyRate2 = basicDaily2 + mealDailyMid;

                    decimal hourlyRate1 = Math.Round(basicDaily1 / 8m, 3, MidpointRounding.AwayFromZero);
                    decimal hourlyRate2 = Math.Round(basicDaily2 / 8m, 3, MidpointRounding.AwayFromZero);

                    decimal regularSalary1 = 0, regularSalary2 = 0;

                    workedDays1 = w1 - payload.slDaysOff1;
                    if (workedDays1 < 0) workedDays1 = 0;

                    workedDays2 = w2 - payload.slDaysOff2;
                    if (workedDays2 < 0) workedDays2 = 0;

                    slSalaryDeduction1 = payload.slDaysOff1 * dailyRate1;
                    slSalaryDeduction2 = payload.slDaysOff2 * dailyRate2;
                    slSalaryDeduction = slSalaryDeduction1 + slSalaryDeduction2;

                    regularSalary1 = workedDays1 * dailyRate1;
                    regularSalary2 = workedDays2 * dailyRate2;
                    regularSalary = regularSalary1 + regularSalary2;

                    ot15xSalary1 = Math.Round(payload.overtime15x1 * hourlyRate1 * 1.5m, 0, MidpointRounding.AwayFromZero);
                    ot2xSalary1 = Math.Round(payload.overtime2x1 * hourlyRate1 * 2.0m, 0, MidpointRounding.AwayFromZero);
                    ot3xSalary1 = Math.Round(payload.overtime3x1 * hourlyRate1 * 3.0m, 0, MidpointRounding.AwayFromZero);

                    ot15xSalary2 = Math.Round(payload.overtime15x2 * hourlyRate2 * 1.5m, 0, MidpointRounding.AwayFromZero);
                    ot2xSalary2 = Math.Round(payload.overtime2x2 * hourlyRate2 * 2.0m, 0, MidpointRounding.AwayFromZero);
                    ot3xSalary2 = Math.Round(payload.overtime3x2 * hourlyRate2 * 3.0m, 0, MidpointRounding.AwayFromZero);

                    overtime15xSalary = ot15xSalary1 + ot15xSalary2;
                    overtime2xSalary = ot2xSalary1 + ot2xSalary2;
                    overtime3xSalary = ot3xSalary1 + ot3xSalary2;

                    // As requested by user: Insurance closed on NEW basic salary
                    insuranceDeduction = Math.Round(newBasic * (insurancePercent / 100m), 0, MidpointRounding.AwayFromZero);

                    allowanceEligibleDays = workedDays1 + workedDays2 - payload.alDaysOff;
                    if (allowanceEligibleDays < 0) allowanceEligibleDays = 0;
                }
                else
                {
                    insuranceDeduction = Math.Round(payload.basicSalary * (insurancePercent / 100m), 0, MidpointRounding.AwayFromZero);

                    decimal otFwdHours = 0;
                    decimal ot2xHours = payload.overtime2x;

                    slSalaryDeduction = payload.slDaysOff * dailySalaryForMeal;
                    decimal baseRegularSalary = workingDays * dailySalaryForMeal;
                    regularSalary = baseRegularSalary - slSalaryDeduction;
                    if (regularSalary < 0) regularSalary = 0;

                    decimal otFwdSalary = Math.Round(otFwdHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                    decimal ot2xSalary = Math.Round(ot2xHours * hourlyRate * 2, 0, MidpointRounding.AwayFromZero);
                    overtime2xSalary = otFwdSalary + ot2xSalary;

                    overtime3xSalary = Math.Round(payload.overtime3x * hourlyRate * 3, 0, MidpointRounding.AwayFromZero);
                    overtime15xSalary = Math.Round(payload.overtime15x * hourlyRate * 1.5m, 0, MidpointRounding.AwayFromZero);

                    allowanceEligibleDays = workingDays - payload.slDaysOff - payload.alDaysOff;
                    if (allowanceEligibleDays < 0) allowanceEligibleDays = 0;
                }

                decimal ot2xHoursTotal = payload.isMidMonthSalaryChange ? (payload.overtime2x1 + payload.overtime2x2) : payload.overtime2x;
                decimal ot3xHoursTotal = payload.isMidMonthSalaryChange ? (payload.overtime3x1 + payload.overtime3x2) : payload.overtime3x;
                decimal ot15xHoursTotal = payload.isMidMonthSalaryChange ? (payload.overtime15x1 + payload.overtime15x2) : payload.overtime15x;

                decimal taxThreshold = ComputeTaxThreshold(payload.taxThreshold, hourlyRate, 0, ot2xHoursTotal, ot3xHoursTotal, ot15xHoursTotal, insuranceDeduction);

                decimal travelAllowance = ComputeProratedAllowanceByWorkedDays(payload.travelAllowance, workingDays, allowanceEligibleDays);
                decimal attendanceIncentive = ComputeProratedAllowanceByWorkedDays(payload.attendanceIncentive, workingDays, allowanceEligibleDays);

                decimal defaultPerfBonus = user != null && user.PerformanceBonus > 0 ? user.PerformanceBonus : 900000m;
                decimal leaveDays = payload.isMidMonthSalaryChange ? (payload.slDaysOff1 + payload.slDaysOff2 + payload.alDaysOff) : (payload.slDaysOff + payload.alDaysOff);
                decimal actualPerformanceBonus = payload.performanceBonus;

                // Điều kiện trừ tiền thưởng hiệu suất chỉ hoạt động khi số tiền nhập vào BẰNG số mặc định HOẶC BẰNG 400.000đ, 850.000đ, 875.000đ.
                // Nếu người dùng nhập bất kỳ số nào khác thì điều kiện trừ tiền sẽ tự động bị vô hiệu hóa.
                bool isDeductionActive = (payload.performanceBonus == defaultPerfBonus) || 
                                         (payload.performanceBonus == 400000m) ||
                                         (payload.performanceBonus == 850000m) ||
                                         (payload.performanceBonus == 875000m);

                if (isDeductionActive)
                {
                    if (leaveDays > 0 && leaveDays <= 1) {
                        actualPerformanceBonus -= payload.perfDeduct1;
                    } else if (leaveDays > 1 && leaveDays <= 2) {
                        actualPerformanceBonus -= payload.perfDeduct2;
                    } else if (leaveDays > 2) {
                        actualPerformanceBonus = 0;
                    }
                    if (actualPerformanceBonus < 0) actualPerformanceBonus = 0;
                }

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

                if (user != null)
                {
                    user.IsMidMonthSalaryChange = payload.isMidMonthSalaryChange;
                    user.OldBasicSalary = payload.oldBasicSalary;
                    user.NewBasicSalary = payload.newBasicSalary;
                    user.SlDaysOff1 = payload.slDaysOff1;
                    user.SlDaysOff2 = payload.slDaysOff2;
                    user.Overtime15x1 = payload.overtime15x1;
                    user.Overtime2x1 = payload.overtime2x1;
                    user.Overtime3x1 = payload.overtime3x1;
                    user.Overtime15x2 = payload.overtime15x2;
                    user.Overtime2x2 = payload.overtime2x2;
                    user.Overtime3x2 = payload.overtime3x2;
                }

                if (payload.month > 0)
                {
                    var detailObject = new {
                        gross = gross,
                        net = net,
                        tax = taxDeduction,
                        insurance = insuranceDeduction,
                        basicSalary = payload.basicSalary,
                        workingDays = payload.workingDays,
                        isMidMonthSalaryChange = payload.isMidMonthSalaryChange,
                        oldBasicSalary = payload.oldBasicSalary,
                        newBasicSalary = payload.newBasicSalary,
                        w1 = w1,
                        w2 = w2,
                        workedDays1 = workedDays1,
                        workedDays2 = workedDays2,
                        regularSalary1 = (workedDays1 * basicDaily1),
                        regularSalary2 = (workedDays2 * basicDaily2),
                        slDaysOff1 = payload.slDaysOff1,
                        slDaysOff2 = payload.slDaysOff2,
                        slDeduction1 = slSalaryDeduction1,
                        slDeduction2 = slSalaryDeduction2,
                        overtime15x1 = payload.overtime15x1,
                        overtime2x1 = payload.overtime2x1,
                        overtime3x1 = payload.overtime3x1,
                        overtime15x2 = payload.overtime15x2,
                        overtime2x2 = payload.overtime2x2,
                        overtime3x2 = payload.overtime3x2,
                        ot15xSalary1 = ot15xSalary1,
                        ot2xSalary1 = ot2xSalary1,
                        ot3xSalary1 = ot3xSalary1,
                        ot15xSalary2 = ot15xSalary2,
                        ot2xSalary2 = ot2xSalary2,
                        ot3xSalary2 = ot3xSalary2,
                        alDaysOff = payload.alDaysOff,
                        slDaysOff = payload.slDaysOff,
                        slDeduction = slSalaryDeduction,
                        overtime15x = ot15xHoursTotal,
                        overtime2x = ot2xHoursTotal,
                        overtime3x = ot3xHoursTotal,
                        overtime15xSalary = overtime15xSalary,
                        overtime2xSalary = overtime2xSalary,
                        overtime3xSalary = overtime3xSalary,
                        otDays8 = payload.otDays8,
                        otDays12 = payload.otDays12,
                        otMeal8Amount = meal8Amount,
                        otMeal12Amount = meal12Amount,
                        bonusMeal = bonusMealAllowance,
                        mealAllowance = payload.mealAllowance,
                        baseTravelAllowance = payload.travelAllowance,
                        travelAllowance = travelAllowance,
                        travelDeduction = payload.travelAllowance - travelAllowance,
                        housingAllowance = payload.housingAllowance,
                        baseAttendanceIncentive = payload.attendanceIncentive,
                        attendanceIncentive = attendanceIncentive,
                        attendanceDeduction = payload.attendanceIncentive - attendanceIncentive,
                        certificateBonus = payload.certificateBonus,
                        basePerformanceBonus = payload.performanceBonus,
                        performanceBonus = actualPerformanceBonus,
                        perfDeduction = payload.performanceBonus - actualPerformanceBonus,
                        otherBonus = payload.otherBonus
                    };
                    string detailJson = JsonSerializer.Serialize(detailObject);
                    
                    if (user != null)
                    {
                        string key = $"{payload.month:D2}-{payload.year}";
                        if (user.SalaryHistory == null) user.SalaryHistory = new System.Collections.Generic.Dictionary<string, decimal>();
                        user.SalaryHistory[key] = net;

                        if (user.SalaryResultHistory == null) user.SalaryResultHistory = new System.Collections.Generic.Dictionary<string, string>();
                        user.SalaryResultHistory[key] = detailJson;

                        if (payload.month == DateTime.Now.Month && payload.year == DateTime.Now.Year)
                        {
                            user.LastCalculatedMonth = payload.month;
                            user.LastCalculatedYear = payload.year;
                            user.LastNetSalary = net;
                        }

                        _dataManager.SaveUser(user);
                    }
                    else
                    {
                        _dataManager.UpdateLastCalculation(payload.username, payload.month, payload.year, net, detailJson);
                    }
                }
                else if (user != null)
                {
                    _dataManager.SaveUser(user);
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
                "Gánh team quá đỉnh, tháng này xứng đáng dẫn đầu!",
                "Phong độ xuất sắc, thu nhập tháng này quá ấm áp.",
                "Cật lực cả tháng qua, nghỉ ngơi xả hơi thôi em!",
                "Dẫn đầu tuyệt đối, không ai đuổi kịp luôn nha.",
                "Xử lý công việc sắc bén, duyệt thưởng nóng tháng này!",
                "Lương thưởng bùng nổ, xứng danh trụ cột công ty!",
                "Hiệu suất kỷ lục, duy trì phong độ đỉnh cao nhé!",
                "Làm việc hết mình, kết quả quá xứng đáng luôn.",
                "Tháng này làm quá tốt, khao anh em chấu bia thôi!",
                "Out trình hoàn toàn, sếp cực kỳ tự hào về em.",
                "Top 1 giữ chắc quá, cả team ai cũng nể phục.",
                "Vượt chỉ tiêu xuất sắc, chuẩn bị nhận thưởng lớn!",
                "Chủ lực của team, tháng nào cũng giữ phong độ vàng.",
                "Tốc độ xử lý công việc đỉnh thật sự, tuyệt vời!",
                "Thành quả xứng đáng cho những ngày cố gắng hết sức.",
                "Giữ vững vị trí Quán quân nhé, xuất sắc lắm em!"
            };

            string[] top2Pool = {
                "Bám đuổi Top 1 suýt soát, tháng sau bứt phá nhé!",
                "Cống hiến rất ấn tượng, suýt nữa là lên ngôi rồi.",
                "Phong độ ổn định lắm, cứ thế này sớm thăng tiến!",
                "Tháng này làm rất tốt, giữ vững đà tiến này nha.",
                "Hiệu suất cực kỳ mượt, tay phải đắc lực của sếp!",
                "Nỗ lực thấy rõ luôn, tháng tới quyết tâm leo Top!",
                "Sát nút vị trí dẫn đầu, cố lên một chút nữa thôi!",
                "Xử lý dự án rất chắc tay, thưởng tháng này rất ấm.",
                "Theo sát Top 1 từng chút, phong thái rất bản lĩnh.",
                "Chỉ thiếu chút may mắn là giật ngôi Quán quân rồi!",
                "Làm việc mượt mà, tháng này xứng đáng nhận thưởng lớn.",
                "Cánh tay đắc lực của phòng, tiếp tục phát huy nhé!",
                "Năng suất ấn tượng lắm, tháng sau quyết tâm lên Top 1!",
                "Tiến bộ rõ rệt qua từng tháng, sếp đánh giá rất cao.",
                "Bứt phá ngoạn mục, vị trí Á quân quá thuyết phục!",
                "Giữ vững tinh thần chiến đấu này cho tháng tới nha."
            };

            string[] top3Pool = {
                "Vào Top 3 VIP là quá đẳng cấp rồi, chúc mừng!",
                "Làm việc rất có gu và hiệu quả, phát huy nhé!",
                "Nỗ lực tuyệt vời, vị trí Top 3 rất xứng đáng.",
                "Giữ phong độ tốt lắm, áp sát Top 1 ngay thôi!",
                "Công việc mượt mà, thưởng quý này cực kỳ tốt.",
                "Duy trì nhịp độ này nhé, cơ hội thăng tiến rộng mở!",
                "Nhân tố chủ lực của team, tiếp tục phát huy nha!",
                "Phong thái làm việc chuyên nghiệp, sếp rất ưng ý.",
                "Tấn công Top 3 cực kỳ thuyết phục, cố lên em!",
                "Nhịp độ làm việc rất chuẩn, giữ đà thăng tiến nhé.",
                "Vững vàng trong nhóm VIP, thu nhập tăng vọt rõ ràng.",
                "Tháng này thi đấu rất cháy, kết quả rất xứng đáng.",
                "Sắp vọt lên Á quân rồi, ráng thêm tí nữa nha!",
                "Ý thức trách nhiệm tuyệt vời, luôn hoàn thành xuất sắc.",
                "Mọi cố gắng đều được đền đáp, chúc mừng Top 3!",
                "Phong độ ổn định qua từng kỳ, tiếp tục bùng nổ nhé."
            };

            string[] top5Pool = {
                "Nhích thêm chút nữa là lọt Top 3 rồi, cố lên!",
                "Âm thầm tích lũy, tháng sau bùng nổ chắc luôn.",
                "Phong độ đang lên rất đều, bám sát Top trên nha.",
                "Làm việc chắc chắn lắm, cơ hội thăng tiến gần rồi.",
                "Đang tích lũy công lực, tháng tới bùng nổ nhé!",
                "Tiềm năng còn rất lớn, tháng sau quyết tâm vào Top 3!",
                "Nhịp làm việc rất chuẩn chỉ, thu nhập đang tăng tiến.",
                "Cơ hội leo Top đang rộng mở, cố gắng ở chặng tiếp theo!",
                "Chỉ còn cách Top 3 một bước ngắn, tự tin lên em!",
                "Tháng này có sự tiến bộ rõ rệt, sếp ghi nhận nhé.",
                "Duy trì đà tăng trưởng này, tháng sau chắc chắn bùng nổ.",
                "Làm việc có kế hoạch rất tốt, phát huy mạnh mẽ nha.",
                "Sắp sửa bứt phá rồi, giữ vững tinh thần này nhé!",
                "Tích cực và chủ động, kết quả đang tốt lên từng ngày.",
                "Nhân tố tiềm năng lớn, tháng tới hứa hẹn bùng nổ!"
            };

            string[] generalPool = {
                "Đang giấu nghề đúng không, tháng sau bung sức nhé!",
                "Nhịp làm việc rất đều đặn, thêm chút bứt phá nữa thôi.",
                "Cố gắng giữ lửa nha, tiềm năng còn rất nhiều.",
                "Cứ kiên trì là thu nhập tăng vọt ngay, tiến lên!",
                "Tháng tới quyết tâm bùng nổ để leo Top nhé!",
                "Phong độ đang nhích dần, kiên trì là thành công!",
                "Mỗi ngày tiến một chút, thu nhập sẽ tự động tăng!",
                "Cần thêm một chút đột phá, sếp luôn tin tưởng em.",
                "Cố gắng phân bổ thời gian tốt hơn để tăng hiệu suất nha.",
                "Chăm chỉ là điểm mạnh, tập trung tăng tốc tháng tới!",
                "Tích lũy kinh nghiệm đều đặn, cơ hội đang chờ phía trước.",
                "Giữ vững năng lượng tích cực, tháng sau bứt phá nhé!",
                "Không ngừng học hỏi, nhất định thu nhập sẽ bùng nổ.",
                "Hãy tự tin thử sức ở những nhiệm vụ mới tháng tới nha.",
                "Chặng đường dài cần sự bền bỉ, cố gắng hết mình nhé!",
                "Tháng mới mục tiêu mới, quyết tâm vào nhóm dẫn đầu!"
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
                1 => $"Gánh team xuất sắc, dẫn đầu tuyệt đối!",
                2 => $"Phong độ ấn tượng, suýt chút là vọt Top 1!",
                3 => $"Giữ vững đà tiến, vững vàng trong Top 3 VIP!",
                _ => $"Nỗ lực tuyệt vời, quyết tâm bứt phá tháng tới!"
            };

            if (!usedComments.Contains(fallback)) return fallback;
            return $"{fallback} ✨";
        }

        public string FirebaseLogin(string projectId, string userUid, string credentialsPath = "")
        {
            return JsonSerializer.Serialize(new { success = false, message = "Cloud sync has been disabled." });
        }

        public string FirebaseSyncAll()
        {
            return JsonSerializer.Serialize(new { success = false, count = 0, message = "Cloud sync has been disabled." });
        }

        public string GetAllUsersJson()
        {
            try
            {
                var users = _dataManager.GetAllUsers();
                return JsonSerializer.Serialize(new { success = true, users = users });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new { success = false, message = ex.Message });
            }
        }

        public string FirebaseSaveSalaryTransaction(string username, string periodKey, decimal netSalary, string detailJson)
        {
            return JsonSerializer.Serialize(new { success = false });
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
            public bool isMidMonthSalaryChange { get; set; }
            public decimal oldBasicSalary { get; set; }
            public decimal newBasicSalary { get; set; }
            public decimal alDaysOff1 { get; set; }
            public decimal slDaysOff1 { get; set; }
            public decimal alDaysOff2 { get; set; }
            public decimal slDaysOff2 { get; set; }
            public decimal overtime15x1 { get; set; }
            public decimal overtime2x1 { get; set; }
            public decimal overtime3x1 { get; set; }
            public decimal overtime15x2 { get; set; }
            public decimal overtime2x2 { get; set; }
            public decimal overtime3x2 { get; set; }
        }
    }
}
