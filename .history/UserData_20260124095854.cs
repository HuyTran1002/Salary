using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SalaryCalculator
{
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
        // Persisted settings
        public decimal InsurancePercent { get; set; } = 10.5m;
        public decimal OtMeal12Amount { get; set; } = 30000m;
        public decimal OtMeal8Amount { get; set; } = 20000m;
        // Lưu lịch sử lương theo tháng/năm
        public Dictionary<string, decimal> SalaryHistory { get; set; } = new Dictionary<string, decimal>();
        // Dùng cho bảng xếp hạng tháng hiện tại
        public int LastCalculatedMonth { get; set; } = 0;
        public int LastCalculatedYear { get; set; } = 0;
        public decimal LastNetSalary { get; set; } = 0;
    }

    public class UserDataManager
    {
        private static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SalaryCalculator");
        private static readonly string UsersFile = Path.Combine(DataFolder, "users.json");

        public UserDataManager()
        {
            if (!Directory.Exists(DataFolder))
                Directory.CreateDirectory(DataFolder);
        }

        public bool Register(string username, string fullName, string phone, int age, decimal basicSalary, decimal mealAllowance, decimal allowance, decimal attendanceIncentive = 710000, int recognizeCount = 0, decimal taxThreshold = 0)
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
                string json = JsonSerializer.Serialize(user);
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
                string json = JsonSerializer.Serialize(user);
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

        public bool UpdateLastCalculation(string username, int month, int year, decimal netSalary)
        {
            try
            {
                var user = Login(username);
                if (user == null)
                    return false;

                // Chỉ lưu lịch sử và ghi file nếu là tháng/năm hiện tại
                if (month == DateTime.Now.Month && year == DateTime.Now.Year)
                {
                    string key = $"{month:D2}-{year}";
                    if (user.SalaryHistory == null)
                        user.SalaryHistory = new Dictionary<string, decimal>();
                    user.SalaryHistory[key] = netSalary;
                    user.LastCalculatedMonth = month;
                    user.LastCalculatedYear = year;
                    user.LastNetSalary = netSalary;

                    string json = JsonSerializer.Serialize(user);
                    string userFile = Path.Combine(DataFolder, $"{username}.json");
                    File.WriteAllText(userFile, json);
                }
                // Nếu là tháng khác thì không lưu
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
