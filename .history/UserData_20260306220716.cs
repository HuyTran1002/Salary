using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SalaryCalculator
{
    public class SalaryCalculationDetail
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal? BasicSalary { get; set; }
        public decimal? WorkingDays { get; set; }
        public decimal? Overtime2xHours { get; set; }
        public decimal? Overtime3xHours { get; set; }
        public decimal? Overtime15xHours { get; set; }
        public decimal? OtDays12 { get; set; }
        public decimal? OtDays8 { get; set; }
        public string? SelectedRating { get; set; }
        public int? RecognizeCount { get; set; }
        public decimal? OtherBonus { get; set; }
        public decimal? Meal12Amount { get; set; }
        public decimal? Meal8Amount { get; set; }
        public decimal? TravelPerDay { get; set; }
        public decimal? AttendancePerDay { get; set; }
        public decimal? CertificateBonus { get; set; }
        public decimal? RankingABonusAmount { get; set; }
        public decimal? RankingBBonusAmount { get; set; }
        public decimal? RankingCBonusAmount { get; set; }
    }

    public class UserInfo
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public int Age { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal MealAllowance { get; set; }
        public decimal Allowance { get; set; }
        public decimal AttendanceIncentive { get; set; }
        public int RecognizeCount { get; set; }
        public decimal TaxThreshold { get; set; }
        // New incentive components
        public decimal TravelAllowance { get; set; } = 8500m;
        public decimal AttendancePerDay { get; set; } = 8500m;
        public decimal HousingAllowance { get; set; } = 100000m;
        public string RatingBonus { get; set; } = "";
        public decimal CertificateBonus { get; set; } = 0;
        public decimal RankingABonusAmount { get; set; } = 300000m;
        public decimal RankingBBonusAmount { get; set; } = 275000m;
        public decimal RankingCBonusAmount { get; set; } = 250000m;
        // Persisted settings
        public decimal InsurancePercent { get; set; } = 10.5m;
        public decimal OtMeal12Amount { get; set; } = 30000m;
        public decimal OtMeal8Amount { get; set; } = 20000m;
        // Lưu lịch sử lương theo tháng/năm
        public Dictionary<string, decimal> SalaryHistory { get; set; } = new Dictionary<string, decimal>();
        // Lưu chi tiết toàn bộ input theo tháng/năm
        public Dictionary<string, SalaryCalculationDetail> SalaryInputHistory { get; set; } = new Dictionary<string, SalaryCalculationDetail>();
        // Dùng cho bảng xếp hạng tháng hiện tại
        public int LastCalculatedMonth { get; set; } = 0;
        public int LastCalculatedYear { get; set; } = 0;
        public decimal LastNetSalary { get; set; } = 0;
    }

    public class UserDataManager
    {
        private static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SalaryCalculator");
        private static readonly string UsersFile = Path.Combine(DataFolder, "users.json");
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        public UserDataManager()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
        }

        public bool Register(string username, string fullName, string phone, int age, decimal basicSalary, decimal mealAllowance, decimal allowance, decimal attendanceIncentive = 710000, int recognizeCount = 0, decimal taxThreshold = 0, string ratingBonus = "", decimal certificateBonus = 0, decimal attendancePerDay = 8500m, decimal travelAllowancePerDay = 8500m, decimal housingAllowance = 100000m, decimal? rankingABonusAmount = null, decimal? rankingBBonusAmount = null, decimal? rankingCBonusAmount = null)
        {
            try
            {
                // Upsert: preserve existing persisted settings (insurance/OT meal) and history if user exists
                var existing = Login(username);
                UserInfo user = existing ?? new UserInfo();
                user.Username = username;
                user.FullName = fullName;
                user.Phone = phone;
                user.Age = age;
                user.BasicSalary = basicSalary;
                user.MealAllowance = mealAllowance;
                user.Allowance = allowance;
                user.AttendanceIncentive = attendanceIncentive;
                user.RecognizeCount = recognizeCount;
                user.TaxThreshold = taxThreshold;
                user.RatingBonus = ratingBonus;
                user.CertificateBonus = certificateBonus;
                user.AttendancePerDay = attendancePerDay < 0 ? 0 : attendancePerDay;
                user.TravelAllowance = travelAllowancePerDay < 0 ? 0 : travelAllowancePerDay;
                user.HousingAllowance = housingAllowance < 0 ? 0 : housingAllowance;
                if (rankingABonusAmount.HasValue) user.RankingABonusAmount = rankingABonusAmount.Value < 0 ? 0 : rankingABonusAmount.Value;
                if (rankingBBonusAmount.HasValue) user.RankingBBonusAmount = rankingBBonusAmount.Value < 0 ? 0 : rankingBBonusAmount.Value;
                if (rankingCBonusAmount.HasValue) user.RankingCBonusAmount = rankingCBonusAmount.Value < 0 ? 0 : rankingCBonusAmount.Value;

                string json = JsonSerializer.Serialize(user);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateInsurancePercent(string username, decimal percent)
        {
            try
            {
                var user = Login(username);
                if (user == null) return false;
                if (percent < 0) percent = 0;
                user.InsurancePercent = percent;
                string json = JsonSerializer.Serialize(user);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                return true;
            }
            catch { return false; }
        }

        public bool UpdateOtMeal12Amount(string username, decimal amount)
        {
            try
            {
                var user = Login(username);
                if (user == null) return false;
                if (amount < 0) amount = 0;
                user.OtMeal12Amount = amount;
                string json = JsonSerializer.Serialize(user, JsonOptions);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                return true;
            }
            catch { return false; }
        }

        public bool UpdateOtMeal8Amount(string username, decimal amount)
        {
            try
            {
                var user = Login(username);
                if (user == null) return false;
                if (amount < 0) amount = 0;
                user.OtMeal8Amount = amount;
                string json = JsonSerializer.Serialize(user, JsonOptions);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                return true;
            }
            catch { return false; }
        }

        public bool UpdateRankingBonusAmounts(string username, decimal amountA, decimal amountB, decimal amountC)
        {
            try
            {
                var user = Login(username);
                if (user == null) return false;
                if (amountA < 0) amountA = 0;
                if (amountB < 0) amountB = 0;
                if (amountC < 0) amountC = 0;
                user.RankingABonusAmount = amountA;
                user.RankingBBonusAmount = amountB;
                user.RankingCBonusAmount = amountC;
                string json = JsonSerializer.Serialize(user, JsonOptions);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                return true;
            }
            catch { return false; }
        }

        public bool IsNewUser(string username)
        {
            return !UserExists(username);
        }

        public UserInfo Login(string username)
        {
            try
            {
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                if (!File.Exists(userFile))
                    return null;

                string json = File.ReadAllText(userFile);
                return JsonSerializer.Deserialize<UserInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        public bool UserExists(string username)
        {
            string userFile = Path.Combine(DataFolder, $"{username}.json");
            return File.Exists(userFile);
        }

        public List<UserInfo> GetAllUsers()
        {
            try
            {
                var users = new List<UserInfo>();
                if (!Directory.Exists(DataFolder))
                    return users;

                var files = Directory.GetFiles(DataFolder, "*.json");
                foreach (var file in files)
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var user = JsonSerializer.Deserialize<UserInfo>(json);
                        if (user != null)
                            users.Add(user);
                    }
                    catch { }
                }

                // Lấy tháng/năm hiện tại
                int nowMonth = DateTime.Now.Month;
                int nowYear = DateTime.Now.Year;
                // Lấy lương tháng hiện tại nếu có
                foreach (var u in users)
                {
                    string key = $"{nowMonth:D2}-{nowYear}";
                    if (u.SalaryHistory != null && u.SalaryHistory.ContainsKey(key))
                    {
                        u.LastNetSalary = u.SalaryHistory[key];
                        u.LastCalculatedMonth = nowMonth;
                        u.LastCalculatedYear = nowYear;
                    }
                    else
                    {
                        // Nếu chưa tính lương tháng hiện tại thì reset về 0
                        u.LastNetSalary = 0;
                        u.LastCalculatedMonth = nowMonth;
                        u.LastCalculatedYear = nowYear;
                    }
                }
                // Xếp hạng theo lương tháng hiện tại
                return users.OrderByDescending(u => u.LastNetSalary).ToList();
            }
            catch
            {
                return new List<UserInfo>();
            }
        }

        public bool UpdateLastCalculation(string username, int month, int year, decimal netSalary, SalaryCalculationDetail detail = null)
        {
            try
            {
                var user = Login(username);
                if (user == null)
                    return false;

                // Luôn lưu lịch sử lương cho bất kỳ tháng nào
                string key = $"{month:D2}-{year}";
                if (user.SalaryHistory == null)
                    user.SalaryHistory = new Dictionary<string, decimal>();
                user.SalaryHistory[key] = netSalary;

                if (detail != null)
                {
                    if (user.SalaryInputHistory == null)
                        user.SalaryInputHistory = new Dictionary<string, SalaryCalculationDetail>();
                    user.SalaryInputHistory[key] = detail;
                }

                // Chỉ cập nhật LastCalculation nếu là tháng/năm hiện tại (cho bảng xếp hạng)
                if (month == DateTime.Now.Month && year == DateTime.Now.Year)
                {
                    user.LastCalculatedMonth = month;
                    user.LastCalculatedYear = year;
                    user.LastNetSalary = netSalary;
                }

                string json = JsonSerializer.Serialize(user, JsonOptions);
                string userFile = Path.Combine(DataFolder, $"{username}.json");
                File.WriteAllText(userFile, json);
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
