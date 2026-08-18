// Firebase Web Auth & Real-time Cloud Sync Module

// =========================================================================
// ⚙️ CẤU HÌNH FIREBASE (Dán mã firebaseConfig của bạn vào biến dưới đây)
// =========================================================================
window.firebaseConfig = window.firebaseConfig || {
  apiKey: "AIzaSyAeA6arqYldNyini3JkZLBBD7zgd5Eb-rM",
  authDomain: "salary-8c0ca.firebaseapp.com",
  projectId: "salary-8c0ca",
  storageBucket: "salary-8c0ca.firebasestorage.app",
  messagingSenderId: "277337938330",
  appId: "1:277337938330:web:b93c16a9801be9832c8223",
  measurementId: "G-6QS56CQ5KK"
};

(function () {
    const cloudPanel = document.getElementById('cloudSyncPanel');
    const btnToggle = document.getElementById('btnToggleCloudPanel');
    const btnClose = document.getElementById('btnCloseCloudPanel');
    const arrowIcon = btnToggle ? btnToggle.querySelector('.arrow-icon') : null;

    // Elements
    const tabLoginBtn = document.getElementById('tabLoginBtn');
    const tabRegisterBtn = document.getElementById('tabRegisterBtn');
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');

    const txtLoginEmail = document.getElementById('txtLoginEmail');
    const txtLoginPassword = document.getElementById('txtLoginPassword');
    const btnLoginSubmit = document.getElementById('btnLoginSubmit');
    const btnForgotPassword = document.getElementById('btnForgotPassword');

    const txtRegEmail = document.getElementById('txtRegEmail');
    const txtRegPassword = document.getElementById('txtRegPassword');
    const txtRegPasswordConfirm = document.getElementById('txtRegPasswordConfirm');
    const btnRegisterSubmit = document.getElementById('btnRegisterSubmit');

    const firebaseAuthSection = document.getElementById('firebaseAuthSection');
    const firebaseVerificationSection = document.getElementById('firebaseVerificationSection');
    const firebaseStatusSection = document.getElementById('firebaseStatusSection');

    const lblUnverifiedEmail = document.getElementById('lblUnverifiedEmail');
    const btnResendVerification = document.getElementById('btnResendVerification');
    const btnCheckVerification = document.getElementById('btnCheckVerification');

    const lblFirebaseUserEmail = document.getElementById('lblFirebaseUserEmail');
    const btnManualSync = document.getElementById('btnManualSync');
    const btnFirebaseLogout = document.getElementById('btnFirebaseLogout');
    const cloudSyncLog = document.getElementById('cloudSyncLog');

    let isPanelOpen = false;
    let firebaseApp = null;
    let firebaseAuth = null;
    let firestoreDb = null;
    let firestoreUnsubscribe = null;

    // Khởi tạo Firebase SDK
    function initFirebaseSDK() {
        const config = window.firebaseConfig;
        if (!config || !config.apiKey || config.apiKey === "YOUR_API_KEY") {
            addLog("Vui lòng nhập mã firebaseConfig vào file cloud-sync.js!", false);
            return false;
        }

        try {
            if (!firebase.apps.length) {
                firebaseApp = firebase.initializeApp(config);
            } else {
                firebaseApp = firebase.app();
            }
            firebaseAuth = firebase.auth();
            firestoreDb = firebase.firestore();

            // Đăng ký lắng nghe trạng thái đăng nhập Firebase Auth
            firebaseAuth.onAuthStateChanged(user => handleAuthStateChanged(user));
            return true;
        } catch (ex) {
            addLog("Lỗi khởi tạo Firebase SDK: " + ex.message, false);
            return false;
        }
    }

    // Gửi thông điệp postMessage sang C# WinForms để thay đổi chiều rộng (Width)
    function notifyCSharpResize(open) {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.postMessage({
                action: 'resize_form',
                isOpen: open,
                targetWidth: 1180
            });
        }
    }

    function togglePanel(show) {
        isPanelOpen = typeof show === 'boolean' ? show : !isPanelOpen;
        if (isPanelOpen) {
            if (cloudPanel) cloudPanel.classList.add('open');
            if (arrowIcon) arrowIcon.textContent = '▶';
        } else {
            if (cloudPanel) cloudPanel.classList.remove('open');
            if (arrowIcon) arrowIcon.textContent = '◀';
        }
        notifyCSharpResize(isPanelOpen);
    }

    if (btnToggle) btnToggle.addEventListener('click', () => togglePanel());
    if (btnClose) btnClose.addEventListener('click', () => togglePanel(false));

    function addLog(message, isSuccess = true) {
        if (!cloudSyncLog) return;
        const p = document.createElement('p');
        p.className = `log-item ${isSuccess ? 'success' : 'error'}`;
        p.innerHTML = `[${new Date().toLocaleTimeString()}] ${message}`;
        cloudSyncLog.prepend(p);
    }

    // Switch Tabs Đăng nhập / Đăng ký
    if (tabLoginBtn && tabRegisterBtn) {
        tabLoginBtn.addEventListener('click', () => {
            tabLoginBtn.classList.add('active');
            tabRegisterBtn.classList.remove('active');
            loginForm.classList.remove('hidden');
            registerForm.classList.add('hidden');
        });
        tabRegisterBtn.addEventListener('click', () => {
            tabRegisterBtn.classList.add('active');
            tabLoginBtn.classList.remove('active');
            registerForm.classList.remove('hidden');
            loginForm.classList.add('hidden');
        });
    }

    // Xử lý Thay đổi Trạng thái Đăng Nhập Firebase Auth
    function handleAuthStateChanged(user) {
        if (user) {
            if (user.emailVerified) {
                if (firebaseAuthSection) firebaseAuthSection.classList.add('hidden');
                if (firebaseVerificationSection) firebaseVerificationSection.classList.add('hidden');
                if (firebaseStatusSection) firebaseStatusSection.classList.remove('hidden');

                if (lblFirebaseUserEmail) lblFirebaseUserEmail.textContent = user.email;
                addLog(`Đã xác thực tài khoản: ${user.email}`, true);

                startFirestoreListener(user.uid);
            } else {
                if (firebaseAuthSection) firebaseAuthSection.classList.add('hidden');
                if (firebaseVerificationSection) firebaseVerificationSection.classList.remove('hidden');
                if (firebaseStatusSection) firebaseStatusSection.classList.add('hidden');

                if (lblUnverifiedEmail) lblUnverifiedEmail.textContent = user.email;
                addLog(`CẢNH BÁO: Email ${user.email} chưa được xác thực!`, false);
            }
        } else {
            if (firebaseAuthSection) firebaseAuthSection.classList.remove('hidden');
            if (firebaseVerificationSection) firebaseVerificationSection.classList.add('hidden');
            if (firebaseStatusSection) firebaseStatusSection.classList.add('hidden');

            if (firestoreUnsubscribe) {
                firestoreUnsubscribe();
                firestoreUnsubscribe = null;
            }
        }
    }

    // Đăng ký Tài khoản mới bằng Email + Password
    if (btnRegisterSubmit) {
        btnRegisterSubmit.addEventListener('click', async () => {
            if (!initFirebaseSDK()) return;

            const email = txtRegEmail.value.trim();
            const password = txtRegPassword.value;
            const passwordConfirm = txtRegPasswordConfirm.value;

            if (!email || !password) {
                addLog("Lỗi: Vui lòng nhập đầy đủ Email và Mật khẩu!", false);
                return;
            }
            if (password !== passwordConfirm) {
                addLog("Lỗi: Mật khẩu xác nhận không khớp!", false);
                return;
            }

            try {
                addLog("Đang tạo tài khoản mới...", true);
                const userCredential = await firebaseAuth.createUserWithEmailAndPassword(email, password);
                const user = userCredential.user;

                await user.sendEmailVerification();
                addLog(`Đăng ký thành công! Đã gửi link xác thực tới ${email}. Vui lòng kiểm tra hộp thư!`, true);

            } catch (ex) {
                addLog("Lỗi đăng ký: " + ex.message, false);
            }
        });
    }

    // Đăng nhập bằng Email + Password
    if (btnLoginSubmit) {
        btnLoginSubmit.addEventListener('click', async () => {
            if (!initFirebaseSDK()) return;

            const email = txtLoginEmail.value.trim();
            const password = txtLoginPassword.value;

            if (!email || !password) {
                addLog("Lỗi: Vui lòng nhập Email và Mật khẩu!", false);
                return;
            }

            try {
                addLog("Đang đăng nhập...", true);
                await firebaseAuth.signInWithEmailAndPassword(email, password);
            } catch (ex) {
                addLog("Lỗi đăng nhập: " + ex.message, false);
            }
        });
    }

    // Quên Mật Khẩu
    if (btnForgotPassword) {
        btnForgotPassword.addEventListener('click', async () => {
            if (!initFirebaseSDK()) return;

            const email = txtLoginEmail.value.trim();
            if (!email) {
                addLog("Vui lòng nhập Email vào ô Đăng nhập rồi bấm Quên Mật Khẩu.", false);
                return;
            }

            try {
                await firebaseAuth.sendPasswordResetEmail(email);
                addLog(`Đã gửi link đặt lại mật khẩu đến Email: ${email}`, true);
            } catch (ex) {
                addLog("Lỗi khôi phục MK: " + ex.message, false);
            }
        });
    }

    // Gửi lại Email xác minh
    if (btnResendVerification) {
        btnResendVerification.addEventListener('click', async () => {
            const user = firebaseAuth ? firebaseAuth.currentUser : null;
            if (user) {
                try {
                    await user.sendEmailVerification();
                    addLog(`Đã gửi lại email xác thực tới: ${user.email}`, true);
                } catch (ex) {
                    addLog("Lỗi gửi email: " + ex.message, false);
                }
            }
        });
    }

    // Kiểm tra lại trạng thái Xác minh Email
    if (btnCheckVerification) {
        btnCheckVerification.addEventListener('click', async () => {
            const user = firebaseAuth ? firebaseAuth.currentUser : null;
            if (user) {
                await user.reload();
                if (user.emailVerified) {
                    addLog("Email đã được xác minh thành công!", true);
                    handleAuthStateChanged(user);
                } else {
                    addLog("Email vẫn chưa được xác minh. Vui lòng nhấp link trong email!", false);
                }
            }
        });
    }

    // Đăng xuất
    if (btnFirebaseLogout) {
        btnFirebaseLogout.addEventListener('click', async () => {
            if (firebaseAuth) {
                await firebaseAuth.signOut();
                addLog("Đã đăng xuất Cloud.", true);
            }
        });
    }

    // Real-time Firestore Listen
    function startFirestoreListener(uid) {
        if (!firestoreDb) return;

        if (firestoreUnsubscribe) firestoreUnsubscribe();

        const employeesRef = firestoreDb.collection("backups").doc(uid).collection("employees");

        firestoreUnsubscribe = employeesRef.onSnapshot(snapshot => {
            snapshot.docChanges().forEach(change => {
                if (change.type === "added" || change.type === "modified") {
                    const data = change.doc.data();
                    const username = change.doc.id || data.Username || data.username;

                    addLog(`Nhận đồng bộ Firestore: ${username}`, true);

                    if (username && window.chrome && window.chrome.webview && window.chrome.webview.hostObjects.backend) {
                        try {
                            window.chrome.webview.hostObjects.backend.SaveRawUserJson(username, JSON.stringify(data));
                        } catch (e) {}
                    }

                    if (typeof window.onCloudDataSync === 'function') {
                        window.onCloudDataSync({ username: username, type: change.type });
                    }
                }
            });
        }, error => {
            addLog("Lỗi Firestore Listen: " + error.message, false);
        });
    }

    // Sao lưu toàn bộ dữ liệu local lên Cloud Firestore (Sync Now)
    if (btnManualSync) {
        btnManualSync.addEventListener('click', async () => {
            const user = firebaseAuth ? firebaseAuth.currentUser : null;
            if (!user || !user.emailVerified) {
                addLog("Vui lòng đăng nhập và xác minh email trước!", false);
                return;
            }

            try {
                if (window.chrome && window.chrome.webview && window.chrome.webview.hostObjects.backend) {
                    addLog("Đang đọc dữ liệu local và đẩy lên Cloud Firestore...", true);
                    const usersResJson = await window.chrome.webview.hostObjects.backend.GetAllUsersJson();
                    const usersRes = JSON.parse(usersResJson);

                    if (usersRes.success && Array.isArray(usersRes.users)) {
                        let count = 0;
                        const batch = firestoreDb.batch();
                        usersRes.users.forEach(u => {
                            if (u.Username) {
                                u._lastSyncedAt = new Date().toISOString();
                                const docRef = firestoreDb.collection("backups").doc(user.uid).collection("employees").doc(u.Username);
                                batch.set(docRef, u, { merge: true });
                                count++;
                            }
                        });
                        await batch.commit();
                        addLog(`Đã sao lưu thành công ${count} hồ sơ nhân viên lên Firestore!`, true);
                    } else {
                        addLog("Lỗi đọc dữ liệu local: " + (usersRes.message || "Không có dữ liệu"), false);
                    }
                }
            } catch (ex) {
                addLog("Lỗi sao lưu Cloud: " + ex.message, false);
            }
        });
    }

    // Tải toàn bộ dữ liệu từ Cloud Firestore về máy local (Restore All)
    const btnRestoreFromCloud = document.getElementById('btnRestoreFromCloud');
    if (btnRestoreFromCloud) {
        btnRestoreFromCloud.addEventListener('click', async () => {
            const user = firebaseAuth ? firebaseAuth.currentUser : null;
            if (!user || !user.emailVerified) {
                addLog("Vui lòng đăng nhập và xác minh email trước!", false);
                return;
            }

            try {
                addLog("Đang kết nối Firestore và tải toàn bộ hồ sơ về local...", true);
                const snapshot = await firestoreDb.collection("backups").doc(user.uid).collection("employees").get();

                if (snapshot.empty) {
                    addLog("Chưa có bản sao lưu nào trên Cloud Firestore.", false);
                    return;
                }

                let count = 0;
                snapshot.forEach(doc => {
                    const data = doc.data();
                    const username = doc.id || data.Username || data.username;
                    if (username && data && window.chrome && window.chrome.webview && window.chrome.webview.hostObjects.backend) {
                        try {
                            window.chrome.webview.hostObjects.backend.SaveRawUserJson(username, JSON.stringify(data));
                            count++;
                        } catch (e) {}
                    }
                });

                addLog(`Khôi phục thành công ${count} hồ sơ nhân viên từ Cloud về máy!`, true);

                if (typeof window.loadRanking === 'function') window.loadRanking();
                if (typeof window.loadUserData === 'function' && window.currentUsername) window.loadUserData(window.currentUsername);

            } catch (ex) {
                addLog("Lỗi tải từ Cloud: " + ex.message, false);
            }
        });
    }

    // Hàm tự động đồng bộ tài khoản hiện tại lên Cloud Firestore khi tính lương xong
    window.autoSyncCurrentUserToCloud = async function() {
        const user = firebaseAuth ? firebaseAuth.currentUser : null;
        if (!user || !user.emailVerified) return;

        try {
            const username = window.currentUsername || (window.currentUser ? window.currentUser.Username : null);
            if (username && window.chrome && window.chrome.webview && window.chrome.webview.hostObjects.backend) {
                const userJson = await window.chrome.webview.hostObjects.backend.Login(username);
                const res = JSON.parse(userJson);
                if (res.success && res.user) {
                    res.user._lastSyncedAt = new Date().toISOString();
                    await firestoreDb.collection("backups").doc(user.uid).collection("employees").doc(username).set(res.user, { merge: true });
                    addLog(`[Tự động] Đã đồng bộ kết quả lương (${username}) lên Firestore!`, true);
                }
            }
        } catch (e) {
            console.error("Auto sync error:", e);
        }
    };

    // Tự động kiểm tra chỉ hiển thị nút mũi tên khi ở màn hình Đăng Nhập (loginScreen)
    function updateCloudBtnVisibility() {
        const loginScreen = document.getElementById('loginScreen');
        const isLoginActive = loginScreen && loginScreen.classList.contains('active');

        if (btnToggle) {
            if (isLoginActive) {
                btnToggle.style.display = 'flex';
            } else {
                btnToggle.style.display = 'none';
                if (isPanelOpen) togglePanel(false);
            }
        }
    }

    // Tự động khởi tạo SDK và theo dõi chuyển màn hình
    setTimeout(() => {
        initFirebaseSDK();

        const loginScreen = document.getElementById('loginScreen');
        if (loginScreen) {
            const observer = new MutationObserver(() => updateCloudBtnVisibility());
            observer.observe(loginScreen, { attributes: true, attributeFilter: ['class'] });
        }
        updateCloudBtnVisibility();
    }, 300);

})();
