using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Cloud.Firestore;
using Microsoft.Web.WebView2.WinForms;

namespace SalaryCalculator
{
    public class FirebaseSyncService
    {
        private static readonly Lazy<FirebaseSyncService> _instance = 
            new Lazy<FirebaseSyncService>(() => new FirebaseSyncService());

        public static FirebaseSyncService Instance => _instance.Value;

        private FirestoreDb _firestoreDb;
        private FirestoreChangeListener _firestoreListener;
        private WebView2 _webView;
        private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);

        public string CurrentUserId { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserId) && _firestoreDb != null;

        private FirebaseSyncService() { }

        public void InitializeWebView(WebView2 webView)
        {
            _webView = webView;
        }

        /// <summary>
        /// 1. Đăng nhập / Khởi tạo Firestore Service
        /// </summary>
        public async Task<(bool success, string message)> LoginAsync(string projectId, string credentialsJsonPath, string userUid)
        {
            try
            {
                if (!File.Exists(credentialsJsonPath))
                {
                    return (false, "File Service Account JSON không tồn tại.");
                }

                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsJsonPath);
                _firestoreDb = await FirestoreDb.CreateAsync(projectId);
                CurrentUserId = userUid;

                // Tự động kích hoạt Listen real-time sau khi đăng nhập
                ListenForUpdates();

                return (true, "Đăng nhập Firebase thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi đăng nhập Firebase: {ex.Message}");
            }
        }

        /// <summary>
        /// Khởi tạo Firestore trực tiếp bằng Project ID nếu đã cấu hình GOOGLE_APPLICATION_CREDENTIALS
        /// </summary>
        public async Task<(bool success, string message)> LoginWithProjectIdAsync(string projectId, string userUid)
        {
            try
            {
                _firestoreDb = await FirestoreDb.CreateAsync(projectId);
                CurrentUserId = userUid;
                ListenForUpdates();
                return (true, "Đăng nhập Firebase thành công.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi đăng nhập Firebase: {ex.Message}");
            }
        }

        /// <summary>
        /// 2. SyncAllLocalToJsonToCloud: Quét toàn bộ file JSON local và upload lên Cloud Firestore
        /// Cấu trúc Firestore: backups/{UID}/employees/{username}
        /// </summary>
        public async Task<(bool success, int count, string message)> SyncAllLocalToJsonToCloudAsync()
        {
            if (!IsLoggedIn) return (false, 0, "Chưa đăng nhập Firebase.");

            try
            {
                string dataFolder = UserDataManager.DataFolder;
                if (!Directory.Exists(dataFolder)) return (false, 0, "Thư mục dữ liệu local không tồn tại.");

                string[] files = Directory.GetFiles(dataFolder, "*.json");
                int successCount = 0;

                CollectionReference employeeRef = _firestoreDb
                    .Collection("backups")
                    .Document(CurrentUserId)
                    .Collection("employees");

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.StartsWith("_")) continue; // Bỏ qua file cài đặt chung

                    string username = Path.GetFileNameWithoutExtension(file);
                    string jsonContent = await SafeReadAllTextAsync(file);

                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        var dictData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                        if (dictData != null)
                        {
                            dictData["_lastSyncedAt"] = DateTime.UtcNow.ToString("o");
                            DocumentReference docRef = employeeRef.Document(username);
                            await docRef.SetAsync(dictData, SetOptions.MergeAll);
                            successCount++;
                        }
                    }
                }

                return (true, successCount, $"Đã đồng bộ {successCount} hồ sơ nhân viên lên Firestore.");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Lỗi sao lưu Cloud: {ex.Message}");
            }
        }

        /// <summary>
        /// 3. ListenForUpdates: Lắng nghe thay đổi real-time từ Firestore
        /// Tự động cập nhật file JSON local và gọi C# Bridge thông báo JS render UI.
        /// </summary>
        public void ListenForUpdates()
        {
            if (!IsLoggedIn) return;

            try
            {
                CollectionReference employeeRef = _firestoreDb
                    .Collection("backups")
                    .Document(CurrentUserId)
                    .Collection("employees");

                _firestoreListener?.StopAsync();

                _firestoreListener = employeeRef.Listen(snapshot =>
                {
                    Task.Run(async () =>
                    {
                        foreach (DocumentChange change in snapshot.Changes)
                        {
                            if (change.ChangeType == DocumentChange.Type.Added || change.ChangeType == DocumentChange.Type.Modified)
                            {
                                DocumentSnapshot doc = change.Document;
                                string username = doc.Id;
                                Dictionary<string, object> data = doc.ToDictionary();

                                string jsonStr = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                                string userFilePath = Path.Combine(UserDataManager.DataFolder, $"{username}.json");

                                bool written = await SafeWriteAllTextAsync(userFilePath, jsonStr);

                                if (written)
                                {
                                    NotifyFrontendUI(username, "updated");
                                }
                            }
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Firestore Listen Error]: {ex.Message}");
            }
        }

        /// <summary>
        /// 4. SaveSalaryWithTransaction: Lưu/Tính toán lương bằng Transaction chống xung đột dữ liệu
        /// </summary>
        public async Task<bool> SaveSalaryWithTransactionAsync(string username, string periodKey, decimal netSalary, string detailJson)
        {
            if (!IsLoggedIn) return false;

            try
            {
                DocumentReference docRef = _firestoreDb
                    .Collection("backups")
                    .Document(CurrentUserId)
                    .Collection("employees")
                    .Document(username);

                await _firestoreDb.RunTransactionAsync(async transaction =>
                {
                    DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(docRef);
                    Dictionary<string, object> userDict;

                    if (snapshot.Exists)
                    {
                        userDict = snapshot.ToDictionary();
                    }
                    else
                    {
                        userDict = new Dictionary<string, object> { { "Username", username } };
                    }

                    Dictionary<string, object> history;
                    if (userDict.ContainsKey("SalaryHistory") && userDict["SalaryHistory"] is Dictionary<string, object> existingHistory)
                    {
                        history = existingHistory;
                    }
                    else
                    {
                        history = new Dictionary<string, object>();
                    }
                    history[periodKey] = netSalary;
                    userDict["SalaryHistory"] = history;

                    Dictionary<string, object> resultHistory;
                    if (userDict.ContainsKey("SalaryResultHistory") && userDict["SalaryResultHistory"] is Dictionary<string, object> existingResultHistory)
                    {
                        resultHistory = existingResultHistory;
                    }
                    else
                    {
                        resultHistory = new Dictionary<string, object>();
                    }
                    resultHistory[periodKey] = detailJson;
                    userDict["SalaryResultHistory"] = resultHistory;

                    userDict["_lastUpdatedTransaction"] = DateTime.UtcNow.ToString("o");

                    transaction.Set(docRef, userDict, SetOptions.MergeAll);
                });

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Transaction Error]: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Đọc file an toàn tránh lỗi File Lock khi ứng dụng khác hoặc thread khác đang mở
        /// </summary>
        private async Task<string> SafeReadAllTextAsync(string filePath, int maxRetries = 3, int delayMs = 200)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _fileLock.WaitAsync();
                    try
                    {
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (var reader = new StreamReader(fs, Encoding.UTF8))
                        {
                            return await reader.ReadToEndAsync();
                        }
                    }
                    finally
                    {
                        _fileLock.Release();
                    }
                }
                catch (IOException)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(delayMs);
                }
            }
            return null;
        }

        /// <summary>
        /// Ghi file an toàn sử dụng FileShare.ReadWrite và Temp File Swapping
        /// </summary>
        private async Task<bool> SafeWriteAllTextAsync(string filePath, string content, int maxRetries = 3, int delayMs = 200)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _fileLock.WaitAsync();
                    try
                    {
                        string tempPath = filePath + ".tmp";
                        using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            await writer.WriteAsync(content);
                        }

                        if (File.Exists(filePath))
                        {
                            File.Replace(tempPath, filePath, null);
                        }
                        else
                        {
                            File.Move(tempPath, filePath);
                        }
                        return true;
                    }
                    finally
                    {
                        _fileLock.Release();
                    }
                }
                catch (IOException)
                {
                    if (i == maxRetries - 1) return false;
                    await Task.Delay(delayMs);
                }
            }
            return false;
        }

        /// <summary>
        /// Phát tín hiệu cho Javascript UI thông qua WebView2 ExecuteScriptAsync
        /// </summary>
        private void NotifyFrontendUI(string username, string changeType)
        {
            if (_webView == null || _webView.IsDisposed || !_webView.IsHandleCreated) return;

            try
            {
                var payload = new { username = username, type = changeType, timestamp = DateTime.Now.ToString("HH:mm:ss") };
                string jsonStr = JsonSerializer.Serialize(payload);
                string script = $"if (typeof window.onCloudDataSync === 'function') {{ window.onCloudDataSync({jsonStr}); }}";

                if (_webView.InvokeRequired)
                {
                    _webView.BeginInvoke(new Action(async () =>
                    {
                        await _webView.CoreWebView2.ExecuteScriptAsync(script);
                    }));
                }
                else
                {
                    _webView.CoreWebView2.ExecuteScriptAsync(script);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Notify UI Error]: {ex.Message}");
            }
        }
    }
}
