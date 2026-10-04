/**
 * 3D Glassmorphism Alert & Toast Notification Engine
 * Seamlessly replaces native window.alert() with a premium, custom UI modal
 * and provides non-blocking toast notifications.
 */
(function () {
    // -------------------------------------------------------------
    // 1. Sleek Floating Toast Notifications
    // -------------------------------------------------------------
    function getToastContainer() {
        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toastContainer';
            if (document.body) {
                document.body.appendChild(container);
            } else {
                document.addEventListener('DOMContentLoaded', () => {
                    document.body.appendChild(container);
                });
            }
        }
        return container;
    }

    function showToast(message, type = 'info', duration = 3500) {
        if (!message) return;
        const container = getToastContainer();

        const toast = document.createElement('div');
        toast.className = `custom-toast custom-toast-${type}`;

        let iconChar = 'ℹ️';
        if (type === 'success') iconChar = '✓';
        else if (type === 'error') iconChar = '✕';

        toast.innerHTML = `
            <div class="toast-icon">${iconChar}</div>
            <div class="toast-msg">${message}</div>
            <button class="toast-close" title="Đóng">&times;</button>
        `;

        function removeToast() {
            if (toast.classList.contains('hide')) return;
            toast.classList.add('hide');
            setTimeout(() => {
                if (toast.parentElement) toast.parentElement.removeChild(toast);
            }, 300);
        }

        const closeBtn = toast.querySelector('.toast-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                removeToast();
            });
        }

        toast.addEventListener('click', removeToast);
        container.appendChild(toast);

        if (duration > 0) {
            setTimeout(removeToast, duration);
        }
    }

    // -------------------------------------------------------------
    // 2. Custom 3D Glassmorphic Modal Alert Popup
    // -------------------------------------------------------------
    let alertOverlay = null;
    let alertCard = null;
    let alertIconWrap = null;
    let alertIcon = null;
    let alertTitle = null;
    let alertMessage = null;
    let alertOkBtn = null;
    let alertCloseBtn = null;
    let alertResolve = null;

    function buildAlertDOM() {
        if (alertOverlay) return;

        alertOverlay = document.createElement('div');
        alertOverlay.id = 'customAlertModal';
        alertOverlay.className = 'custom-alert-overlay';

        alertOverlay.innerHTML = `
            <div class="custom-alert-card glass-card">
                <button class="custom-alert-close" id="customAlertCloseBtn" title="Đóng">&times;</button>
                <div class="custom-alert-icon-wrap" id="customAlertIconWrap">
                    <span id="customAlertIcon" class="custom-alert-icon">✓</span>
                </div>
                <h3 id="customAlertTitle" class="custom-alert-title">Thông Báo</h3>
                <div id="customAlertMessage" class="custom-alert-message"></div>
                <div class="custom-alert-actions">
                    <button id="customAlertOkBtn" class="btn-3d btn-primary custom-alert-btn">OK</button>
                </div>
            </div>
        `;

        const target = document.body || document.documentElement;
        target.appendChild(alertOverlay);

        alertCard = alertOverlay.querySelector('.custom-alert-card');
        alertIconWrap = alertOverlay.querySelector('#customAlertIconWrap');
        alertIcon = alertOverlay.querySelector('#customAlertIcon');
        alertTitle = alertOverlay.querySelector('#customAlertTitle');
        alertMessage = alertOverlay.querySelector('#customAlertMessage');
        alertOkBtn = alertOverlay.querySelector('#customAlertOkBtn');
        alertCloseBtn = alertOverlay.querySelector('#customAlertCloseBtn');

        function closeAlert() {
            if (!alertOverlay.classList.contains('show')) return;
            alertOverlay.classList.remove('show');
            if (alertResolve) {
                const res = alertResolve;
                alertResolve = null;
                res(true);
            }
        }

        alertOkBtn.addEventListener('click', closeAlert);
        alertCloseBtn.addEventListener('click', closeAlert);
        alertOverlay.addEventListener('click', (e) => {
            if (e.target === alertOverlay) closeAlert();
        });

        window.addEventListener('keydown', (e) => {
            if (alertOverlay.classList.contains('show')) {
                if (e.key === 'Escape' || e.key === 'Enter') {
                    e.preventDefault();
                    closeAlert();
                }
            }
        });
    }

    function customAlert(message, type = null, title = null) {
        return new Promise((resolve) => {
            buildAlertDOM();
            if (!alertOverlay) {
                if (resolve) resolve(true);
                return;
            }

            const str = String(message || '');
            let inferredType = type;
            let inferredTitle = title;
            let iconChar = 'ℹ️';

            const lower = str.toLowerCase();
            if (!inferredType) {
                if (lower.includes('thành công') || lower.includes('hoàn tất') || lower.includes('success')) {
                    inferredType = 'success';
                } else if (lower.includes('lỗi') || lower.includes('thất bại') || lower.includes('error') || lower.includes('vui lòng') || lower.includes('không thể')) {
                    inferredType = 'error';
                } else {
                    inferredType = 'info';
                }
            }

            if (!inferredTitle) {
                if (inferredType === 'success') inferredTitle = 'Thành Công';
                else if (inferredType === 'error') inferredTitle = lower.includes('vui lòng') ? 'Lưu Ý' : 'Thông Báo Lỗi';
                else inferredTitle = 'Thông Báo';
            }

            if (inferredType === 'success') {
                iconChar = '✓';
            } else if (inferredType === 'error') {
                iconChar = lower.includes('vui lòng') ? '⚠️' : '✕';
            } else {
                iconChar = 'ℹ️';
            }

            // Clean previous type classes
            alertCard.classList.remove('type-success', 'type-error', 'type-info');
            alertCard.classList.add(`type-${inferredType}`);

            alertIcon.textContent = iconChar;
            alertTitle.textContent = inferredTitle;
            alertMessage.textContent = str;

            alertResolve = resolve;
            alertOverlay.classList.add('show');

            setTimeout(() => {
                if (alertOkBtn) alertOkBtn.focus();
            }, 100);
        });
    }

    // Expose globals
    window.showToast = showToast;
    window.customAlert = customAlert;

    // Gracefully override window.alert to render our custom 3D glassmorphic popup modal
    window.alert = function (msg) {
        return customAlert(msg);
    };

    // Auto-init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', buildAlertDOM);
    } else {
        buildAlertDOM();
    }
})();
