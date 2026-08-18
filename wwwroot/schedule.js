/**
 * LỊCH LÀM VIỆC SỐ 2026 (Digital Shift Schedule)
 * Logic xoay ca: 5 ca Night (n) -> 1 Off -> 5 ca Mid (m) -> 2 Off -> 5 ca Day (d) -> 2 Off (Tổng 20 ngày)
 */

(function () {
    let isScheduleOpen = false;
    let selectedTeam = localStorage.getItem('user_shift_team') || 'A1';
    let selectedMonth = new Date().getMonth() + 1; // Default current month (1-12)
    let selectedYear = 2026;

    // Chu kỳ 20 ngày xoay ca chuẩn:
    // 0..4   : 5 ca Night (n)
    // 5      : FWD (Nghỉ 1/2 chuyển Night -> Mid)
    // 6      : Off (Nghỉ 2/2 chuyển Night -> Mid)
    // 7..11  : 5 ca Mid (m)
    // 12     : Off (Nghỉ 1/1 chuyển Mid -> Day)
    // 13..17 : 5 ca Day (d)
    // 18     : FWD (Nghỉ 1/2 chuyển Day -> Night)
    // 19     : Off (Nghỉ 2/2 chuyển Day -> Night)
    const CYCLE_LENGTH = 20;

    // Anchor Mốc Offset chuẩn xác từ ngày 01/08/2026 cho 8 Kíp
    const TEAM_OFFSETS = {
        'A1': 0,   // n n n n n FWD Off (5 Night -> FWD -> Off)
        'A2': 2,   // n n n FWD Off (3 Night còn lại -> FWD -> Off)
        'B1': 15,  // d d d FWD Off (3 Day còn lại -> FWD -> Off)
        'B2': 17,  // d FWD Off (1 Day còn lại -> FWD -> Off)
        'C1': 10,  // m m Off (2 Mid còn lại -> Off)
        'C2': 12,  // Off d d d d d FWD Off (1 Off -> 5 Day -> FWD -> Off)
        'D1': 5,   // FWD Off m m m m m Off (1 FWD -> 1 Off -> 5 Mid)
        'D2': 7    // m m m m m Off (5 Mid -> Off)
    };

    const ANCHOR_DATE = new Date(2026, 7, 1); // 01/08/2026

    function getShiftForDate(team, targetDate) {
        if (!TEAM_OFFSETS.hasOwnProperty(team)) return 'Off';

        // Tính toán theo mốc chu kỳ 20 ngày
        const diffTime = targetDate.getTime() - ANCHOR_DATE.getTime();
        const diffDays = Math.floor(diffTime / (1000 * 3600 * 24));

        let offset = (TEAM_OFFSETS[team] + diffDays) % CYCLE_LENGTH;
        if (offset < 0) offset += CYCLE_LENGTH;

        if (offset >= 0 && offset <= 4) return 'n';       // 5 Night
        if (offset === 5) return 'FWD';                   // FWD (Nghỉ 1/2 Night -> Mid)
        if (offset === 6) return 'Off';                   // Off (Nghỉ 2/2 Night -> Mid)
        if (offset >= 7 && offset <= 11) return 'm';      // 5 Mid
        if (offset === 12) return 'Off';                  // Off (Nghỉ 1 Mid -> Day)
        if (offset >= 13 && offset <= 17) return 'd';     // 5 Day
        if (offset === 18) return 'FWD';                  // FWD (Nghỉ 1/2 Day -> Night)
        return 'Off';                                     // Off (Nghỉ 2/2 Day -> Night)
    }

    function getShiftLabelAndClass(code) {
        switch (code) {
            case 'n':
                return { label: 'Night (Đêm)', badge: 'n', cssClass: 'night', time: '22:30 - 06:30' };
            case 'm':
                return { label: 'Mid (Chiều)', badge: 'm', cssClass: 'mid', time: '14:30 - 22:30' };
            case 'd':
                return { label: 'Day (Sáng)', badge: 'd', cssClass: 'day', time: '06:30 - 14:30' };
            case 'FWD':
                return { label: 'FWD (Linh hoạt)', badge: 'FWD', cssClass: 'fwd', time: 'Làm việc linh hoạt' };
            default:
                return { label: 'OFF (Nghỉ)', badge: 'Off', cssClass: 'off', time: 'Nghỉ ngơi' };
        }
    }

    // Render bảng lịch dạng Ma Trận Ngang (Landscape Layout y hệt Thẻ in)
    function renderScheduleTable() {
        const thead = document.querySelector('.schedule-table thead');
        const tbody = document.getElementById('scheduleTableBody');
        const summaryText = document.getElementById('shiftSummaryText');
        const selTeam = document.getElementById('selShiftTeam');
        const selMonth = document.getElementById('selShiftMonth');
        const selYear = document.getElementById('selShiftYear');

        if (!tbody) return;

        if (selTeam) selectedTeam = selTeam.value;
        if (selMonth) selectedMonth = parseInt(selMonth.value, 10);
        if (selYear) selectedYear = parseInt(selYear.value, 10);

        // Cập nhật tiêu đề theo Năm được chọn
        const titleEl = document.getElementById('schedulePanelTitle');
        if (titleEl) {
            titleEl.textContent = `📅 LỊCH LÀM VIỆC SỐ ${selectedYear}`;
        }

        localStorage.setItem('user_shift_team', selectedTeam);

        const today = new Date();
        const daysInMonth = new Date(selectedYear, selectedMonth, 0).getDate();
        const daysOfWeek = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

        // 1. Render Header Rows (Hàng Ngày & Hàng Thứ)
        if (thead) {
            let rowDaysHtml = `<tr><th class="sticky-col">Ngày</th>`;
            let rowDaysOfWeekHtml = `<tr><th class="sticky-col">Thứ</th>`;

            for (let d = 1; d <= daysInMonth; d++) {
                const dateObj = new Date(selectedYear, selectedMonth - 1, d);
                const dayOfWeekStr = daysOfWeek[dateObj.getDay()];
                const isToday = (today.getFullYear() === selectedYear && (today.getMonth() + 1) === selectedMonth && today.getDate() === d);
                const todayClass = isToday ? 'today-col' : '';

                rowDaysHtml += `<th class="${todayClass}">${d < 10 ? '0' + d : d}</th>`;
                rowDaysOfWeekHtml += `<th class="${todayClass}" style="color: ${dateObj.getDay() === 0 ? '#f38ba8' : '#a6adc8'};">${dayOfWeekStr}</th>`;
            }

            rowDaysHtml += `</tr>`;
            rowDaysOfWeekHtml += `</tr>`;
            thead.innerHTML = rowDaysHtml + rowDaysOfWeekHtml;
        }

        // 2. Render Rows cho từng Kíp (A1..D2)
        tbody.innerHTML = '';
        const teamsToRender = selectedTeam === 'ALL' 
            ? ['A1', 'A2', 'B1', 'B2', 'C1', 'C2', 'D1', 'D2']
            : [selectedTeam];

        let todayShiftInfo = null;

        teamsToRender.forEach(team => {
            const tr = document.createElement('tr');
            let rowHtml = `<td class="sticky-col" style="color: #a6e3a1;">${team}</td>`;

            for (let d = 1; d <= daysInMonth; d++) {
                const dateObj = new Date(selectedYear, selectedMonth - 1, d);
                const isToday = (today.getFullYear() === selectedYear && (today.getMonth() + 1) === selectedMonth && today.getDate() === d);
                const todayClass = isToday ? 'today-col' : '';

                const shiftCode = getShiftForDate(team, dateObj);
                const info = getShiftLabelAndClass(shiftCode);

                if (isToday && (selectedTeam === 'ALL' || selectedTeam === team)) {
                    todayShiftInfo = info;
                }

                rowHtml += `<td class="${todayClass}"><span class="shift-badge ${info.cssClass}">${info.badge}</span></td>`;
            }

            tr.innerHTML = rowHtml;
            tbody.appendChild(tr);
        });

    // Helper lấy ca làm việc thực tế theo giờ mở app
    function getCurrentTimeShiftInfo() {
        const now = new Date();
        const h = now.getHours();
        const m = now.getMinutes();
        const timeInMins = h * 60 + m;

        // 06:30 (390m) -> 14:30 (870m): Ca Sáng (d)
        // 14:30 (870m) -> 22:30 (1350m): Ca Chiều (m)
        // 22:30 (1350m) -> 06:30 (390m ngày hôm sau): Ca Đêm (n)
        let code = 'n';
        if (timeInMins >= 390 && timeInMins < 870) {
            code = 'd';
        } else if (timeInMins >= 870 && timeInMins < 1350) {
            code = 'm';
        } else {
            code = 'n';
        }

        const timeStr = `${h < 10 ? '0' + h : h}:${m < 10 ? '0' + m : m}`;
        return { code, timeStr };
    }

    // 3. Cập nhật thẻ tóm tắt Ca làm việc hiện tại
    if (summaryText) {
        const currentTodayDate = new Date();
        const timeShift = getCurrentTimeShiftInfo();
        const currentShiftInfo = getShiftLabelAndClass(timeShift.code);

        if (selectedTeam === 'ALL') {
            const allTeams = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2', 'D1', 'D2'];
            const workingTeams = allTeams.filter(t => getShiftForDate(t, currentTodayDate) === timeShift.code);

            summaryText.innerHTML = `${currentShiftInfo.label} (${currentShiftInfo.time}) — <strong>Đang trực:</strong> ${workingTeams.length > 0 ? workingTeams.map(t => 'Shift ' + t).join(', ') : 'Không có'}`;
        } else {
            const shiftCode = getShiftForDate(selectedTeam, currentTodayDate);
            const activeInfo = getShiftLabelAndClass(shiftCode);
            summaryText.innerHTML = `Shift <strong>${selectedTeam}</strong> hôm nay (${currentTodayDate.getDate()}/${currentTodayDate.getMonth() + 1}): ${activeInfo.label} (${activeInfo.time})`;
        }
    }
    }

    function toggleSchedulePanel(show) {
        const panel = document.getElementById('schedulePanelLeft');
        const icon = document.getElementById('scheduleArrowIcon');

        isScheduleOpen = typeof show === 'boolean' ? show : !isScheduleOpen;

        if (isScheduleOpen) {
            if (panel) panel.classList.add('open');
            if (icon) icon.textContent = '◀';
            renderScheduleTable();
        } else {
            if (panel) panel.classList.remove('open');
            if (icon) icon.textContent = '▶';
        }
    }

    // Tự động kiểm tra chỉ hiển thị nút Lịch bên trái khi ở màn hình Đăng Nhập (loginScreen)
    function updateLeftBtnVisibility() {
        const btnToggleLeft = document.getElementById('btnToggleSchedule');
        const loginScreen = document.getElementById('loginScreen');
        const isLoginActive = loginScreen && loginScreen.classList.contains('active');

        if (btnToggleLeft) {
            if (isLoginActive) {
                btnToggleLeft.style.display = 'flex';
            } else {
                btnToggleLeft.style.display = 'none';
                if (isScheduleOpen) toggleSchedulePanel(false);
            }
        }
    }

    // Khởi tạo Event Listeners sau khi DOM load
    document.addEventListener('DOMContentLoaded', () => {
        const btnToggleLeft = document.getElementById('btnToggleSchedule');
        const btnClose = document.getElementById('btnCloseSchedulePanel');
        const selTeam = document.getElementById('selShiftTeam');
        const selMonth = document.getElementById('selShiftMonth');
        const selYear = document.getElementById('selShiftYear');

        if (btnToggleLeft) {
            btnToggleLeft.addEventListener('click', () => toggleSchedulePanel());
        }

        if (btnClose) {
            btnClose.addEventListener('click', () => toggleSchedulePanel(false));
        }

        if (selTeam) {
            selTeam.value = selectedTeam;
            selTeam.addEventListener('change', () => renderScheduleTable());
        }

        if (selMonth) {
            selMonth.value = selectedMonth;
            selMonth.addEventListener('change', () => renderScheduleTable());
        }

        if (selYear) {
            selYear.value = selectedYear;
            selYear.addEventListener('change', () => renderScheduleTable());
        }

        // MutationObserver theo dõi khi chuyển sang màn hình loginScreen
        const loginScreen = document.getElementById('loginScreen');
        if (loginScreen) {
            const observer = new MutationObserver(() => updateLeftBtnVisibility());
            observer.observe(loginScreen, { attributes: true, attributeFilter: ['class'] });
        }
        updateLeftBtnVisibility();
    });

    // Expose global helper
    window.toggleSchedulePanel = toggleSchedulePanel;

})();
