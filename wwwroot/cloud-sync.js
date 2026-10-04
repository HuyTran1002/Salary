/**
 * Real-Time Cloud Synchronization Module (Firebase Firestore)
 * Works synchronously on Desktop (WebView2) and Mobile (Capacitor/Android)
 */
(function () {
    const CONFIG_KEY = 'salary_firebase_config';
    let db = null;
    let auth = null;
    let isInitialized = false;
    let unsubscribeListener = null;

    // Load saved Firebase config
    function getStoredConfig() {
        try {
            if (window.FIREBASE_CONFIG) return window.FIREBASE_CONFIG;
            const saved = localStorage.getItem(CONFIG_KEY);
            return saved ? JSON.parse(saved) : null;
        } catch (e) {
            return null;
        }
    }

    function saveStoredConfig(cfg) {
        try {
            localStorage.setItem(CONFIG_KEY, JSON.stringify(cfg));
        } catch (e) {}
    }

    // Initialize Firebase
    function initFirebase() {
        const config = getStoredConfig();
        if (!config || !config.apiKey || !config.projectId) {
            updateStatusUI(false, "Chưa cấu hình Cloud (Đang chạy Offline)");
            return false;
        }

        if (typeof firebase === 'undefined') {
            updateStatusUI(false, "Không thể tải thư viện Firebase (Đang Offline)");
            return false;
        }

        try {
            if (!firebase.apps.length) {
                firebase.initializeApp(config);
            }
            db = firebase.firestore();
            auth = firebase.auth ? firebase.auth() : null;
            if (auth) {
                auth.onAuthStateChanged(u => {
                    if (!u) {
                        auth.signInAnonymously().catch(() => {});
                    }
                });
            }
            isInitialized = true;

            updateStatusUI(true, `Đã kết nối Cloud (${config.projectId})`);
            startRealtimeListener();
            return true;
        } catch (e) {
            console.error("Firebase init error:", e);
            updateStatusUI(false, "Lỗi kết nối Cloud: " + e.message);
            return false;
        }
    }

    // Real-time listener for incoming changes from other devices
    function startRealtimeListener() {
        if (!db) return;
        if (unsubscribeListener) unsubscribeListener();

        try {
            unsubscribeListener = db.collection("salary_users").onSnapshot(snapshot => {
                const backend = typeof window.getBackend === 'function' ? window.getBackend() : (window.localBackend || null);
                if (!backend) return;

                snapshot.docChanges().forEach(change => {
                    if (change.type === "added" || change.type === "modified") {
                        const data = change.doc.data();
                        const username = change.doc.id || data.Username;
                        if (username && backend.SaveRawUserJson) {
                            try {
                                backend.SaveRawUserJson(username, JSON.stringify(data));
                                // If currently logged in user was modified, trigger UI reload
                                if (window.currentUsername && window.currentUsername.toLowerCase() === username.toLowerCase()) {
                                    if (typeof window.loadUserData === 'function') {
                                        window.loadUserData(username);
                                    }
                                }
                                if (typeof window.loadRanking === 'function') {
                                    window.loadRanking();
                                }
                            } catch (err) {
                                console.warn("Failed to save synced user locally:", err);
                            }
                        }
                    }
                });
            }, error => {
                console.warn("Firestore listener error:", error);
            });
        } catch (e) {
            console.error("Failed to start realtime listener:", e);
        }
    }

    // Push single user to Firestore
    async function pushUserToCloud(user) {
        if (!db || !user || !user.Username) return;
        try {
            const copy = JSON.parse(JSON.stringify(user));
            copy._lastSynced = new Date().toISOString();
            await db.collection("salary_users").doc(user.Username).set(copy, { merge: true });
            console.log(`[CloudSync] Đã đẩy dữ liệu của ${user.Username} lên Cloud.`);
        } catch (e) {
            console.error("[CloudSync] Lỗi đẩy lên Cloud:", e);
        }
    }

    // Push all local users to Firestore
    async function pushAllToCloud() {
        if (!db) {
            throw new Error("Chưa kết nối Firebase! Vui lòng cấu hình trước.");
        }
        const backend = typeof window.getBackend === 'function' ? window.getBackend() : (window.localBackend || null);
        if (!backend) throw new Error("Không tìm thấy backend lưu trữ");

        let users = [];
        if (backend.GetAllUsersJson) {
            const res = await backend.GetAllUsersJson();
            const parsed = JSON.parse(res);
            if (parsed.success && Array.isArray(parsed.users)) {
                users = parsed.users;
            }
        }

        if (users.length === 0) {
            throw new Error("Không có dữ liệu nào trên máy để đồng bộ.");
        }

        const batch = db.batch();
        let count = 0;
        users.forEach(u => {
            if (u.Username) {
                const docRef = db.collection("salary_users").doc(u.Username);
                const copy = JSON.parse(JSON.stringify(u));
                copy._lastSynced = new Date().toISOString();
                batch.set(docRef, copy, { merge: true });
                count++;
            }
        });

        await batch.commit();
        return count;
    }

    // Pull all users from Firestore to local
    async function pullAllFromCloud() {
        if (!db) {
            throw new Error("Chưa kết nối Firebase! Vui lòng cấu hình trước.");
        }
        const backend = typeof window.getBackend === 'function' ? window.getBackend() : (window.localBackend || null);
        if (!backend || !backend.SaveRawUserJson) throw new Error("Backend không hỗ trợ lưu dữ liệu");

        const snapshot = await db.collection("salary_users").get();
        if (snapshot.empty) {
            return 0;
        }

        let count = 0;
        snapshot.forEach(doc => {
            const data = doc.data();
            const username = doc.id || data.Username;
            if (username && data) {
                backend.SaveRawUserJson(username, JSON.stringify(data));
                count++;
            }
        });

        if (typeof window.loadRanking === 'function') window.loadRanking();
        if (window.currentUsername && typeof window.loadUserData === 'function') {
            window.loadUserData(window.currentUsername);
        }

        return count;
    }

    function updateStatusUI(online, message) {
        const badge = document.getElementById('cloudSyncStatusBadge');
        const desc = document.getElementById('cloudSyncStatusDesc');
        if (badge) {
            badge.className = online ? 'cloud-badge online' : 'cloud-badge offline';
            badge.textContent = online ? '🟢 Online' : '⚪ Offline';
        }
        if (desc) {
            desc.textContent = message || '';
        }
        const btnHeader = document.getElementById('btnCloudSyncHeader');
        if (btnHeader) {
            btnHeader.title = message || 'Cloud Sync';
            btnHeader.style.borderColor = online ? '#10b981' : 'rgba(255,255,255,0.2)';
        }
    }

    // Fetch single user directly from Cloud by username (ideal for freshly installed devices)
    async function fetchUserDirectly(username) {
        if (!db || !username) return null;
        try {
            const trimmed = username.trim();
            // 1. Direct get by Document ID
            const docSnap = await db.collection("salary_users").doc(trimmed).get();
            if (docSnap.exists) {
                return docSnap.data();
            }
            // 2. Case-insensitive search fallback
            const snapshot = await db.collection("salary_users").get();
            let matched = null;
            snapshot.forEach(doc => {
                const data = doc.data();
                const docUser = (doc.id || data.Username || '').trim();
                if (docUser.toLowerCase() === trimmed.toLowerCase()) {
                    matched = data;
                }
            });
            return matched;
        } catch (e) {
            console.warn("[CloudSync] fetchUserDirectly warning:", e);
            return null;
        }
    }

    // Expose API
    window.CloudSync = {
        init: initFirebase,
        pushUserToCloud,
        pushAllToCloud,
        pullAllFromCloud,
        fetchUserDirectly,
        saveConfig(cfg) {
            saveStoredConfig(cfg);
            return initFirebase();
        },
        getConfig: getStoredConfig,
        isConnected: () => isInitialized && db !== null
    };

    // Auto sync current logged in user when profile or salary calculation completes
    window.autoSyncCurrentUserToCloud = async function() {
        if (!isInitialized || !db) return;
        const backend = typeof window.getBackend === 'function' ? window.getBackend() : (window.localBackend || null);
        if (!backend) return;
        const username = window.currentUsername || (window.currentUser ? window.currentUser.Username : null);
        if (!username) return;

        try {
            const userRes = await backend.Login(username);
            const parsed = JSON.parse(userRes);
            if (parsed.success && parsed.user) {
                await pushUserToCloud(parsed.user);
                console.log(`[CloudSync] Auto-synced user: ${username}`);
            }
        } catch (e) {
            console.warn("Auto sync failed:", e);
        }
    };

    // Auto-init on load and bind UI elements
    function setupCloudUI() {
        const modal = document.getElementById('cloudSyncModal');
        const btnOpen1 = document.getElementById('btnOpenCloudSync');
        const btnOpen2 = document.getElementById('btnOpenCloudSyncLogin');
        const btnClose = document.getElementById('btnCloseCloudSync');
        const btnPush = document.getElementById('btnCloudSyncPush');
        const btnPull = document.getElementById('btnCloudSyncPull');
        const btnSave = document.getElementById('btnSaveFirebaseConfig');
        const inputProj = document.getElementById('cfgFirebaseProjectId');
        const inputKey = document.getElementById('cfgFirebaseApiKey');

        function openModal() {
            if (!modal) return;
            const cfg = getStoredConfig() || {};
            if (inputProj) inputProj.value = cfg.projectId || '';
            if (inputKey) inputKey.value = cfg.apiKey || '';
            modal.classList.remove('hidden');
            modal.style.display = 'flex';
        }

        function closeModal() {
            if (modal) {
                modal.classList.add('hidden');
                modal.style.display = 'none';
            }
        }

        window.openCloudSyncModal = openModal;
        window.closeCloudSyncModal = closeModal;

        if (btnOpen1 && !btnOpen1.hasAttribute('onclick')) btnOpen1.addEventListener('click', openModal);
        if (btnOpen2 && !btnOpen2.hasAttribute('onclick')) btnOpen2.addEventListener('click', openModal);
        if (btnClose && !btnClose.hasAttribute('onclick')) btnClose.addEventListener('click', closeModal);
        if (modal) {
            modal.addEventListener('click', (e) => {
                if (e.target === modal) closeModal();
            });
        }

        if (btnSave) {
            btnSave.addEventListener('click', () => {
                const projectId = inputProj ? inputProj.value.trim() : '';
                const apiKey = inputKey ? inputKey.value.trim() : '';
                if (!projectId || !apiKey) {
                    alert("Vui lòng nhập đầy đủ Firebase Project ID và API Key!");
                    return;
                }
                const cfg = {
                    projectId,
                    apiKey,
                    authDomain: `${projectId}.firebaseapp.com`
                };
                saveStoredConfig(cfg);
                const success = initFirebase();
                if (success) {
                    alert("Đã kết nối thành công với Firebase Cloud!");
                } else {
                    alert("Đã lưu cấu hình! Đang thử kết nối lại...");
                }
            });
        }

        if (btnPush) {
            btnPush.addEventListener('click', async () => {
                btnPush.disabled = true;
                btnPush.textContent = "Đang đẩy...";
                try {
                    const count = await pushAllToCloud();
                    alert(`Đã sao lưu thành công ${count} hồ sơ lên Firebase Cloud!`);
                } catch (err) {
                    alert("Lỗi sao lưu: " + err.message);
                } finally {
                    btnPush.disabled = false;
                    btnPush.textContent = "⬆️ Sao lưu lên Cloud";
                }
            });
        }

        if (btnPull) {
            btnPull.addEventListener('click', async () => {
                btnPull.disabled = true;
                btnPull.textContent = "Đang tải...";
                try {
                    const count = await pullAllFromCloud();
                    alert(`Đã tải về thành công ${count} hồ sơ từ Cloud!`);
                } catch (err) {
                    alert("Lỗi tải về: " + err.message);
                } finally {
                    btnPull.disabled = false;
                    btnPull.textContent = "⬇️ Tải về máy này";
                }
            });
        }
    }

    if (document.readyState === 'loading') {
        window.addEventListener('DOMContentLoaded', () => {
            setTimeout(initFirebase, 200);
            setupCloudUI();
        });
    } else {
        setTimeout(initFirebase, 200);
        setupCloudUI();
    }
})();
