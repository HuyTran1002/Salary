/**
 * LỊCH LÀM VIỆC SỐ 2026 (Digital Shift Schedule)
 * Logic xoay ca: 5 ca Night (n) -> 1 Off -> 5 ca Mid (m) -> 2 Off -> 5 ca Day (d) -> 2 Off (Tổng 20 ngày)
 */

(function () {
    let isScheduleOpen = false;
    let selectedTeam = localStorage.getItem('user_shift_team') || 'A1';
    let selectedMonth = new Date().getMonth() + 1; // Default current month (1-12)
    let selectedYear = 2026;
    let lastRenderedDateString = '';

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
        if (team === 'N') {
            const day = targetDate.getDay();
            if (day >= 1 && day <= 5) return 'd'; // Mon-Fri
            if (day === 6) return 'FWD'; // Sat
            return 'Off'; // Sun
        }

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
        lastRenderedDateString = today.toDateString();
        const daysInMonth = new Date(selectedYear, selectedMonth, 0).getDate();
        const daysOfWeek = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];

        // 1. Render Header Rows (Hàng Ngày & Hàng Thứ)
        if (thead) {
            let rowDaysHtml = `<tr><th class="sticky-col">Ngày</th>`;
            let rowLunarDaysHtml = `<tr><th class="sticky-col" style="color: #f38ba8; font-size: 0.85em;">Âm Lịch</th>`;
            let rowDaysOfWeekHtml = `<tr><th class="sticky-col">Thứ</th>`;

            for (let d = 1; d <= daysInMonth; d++) {
                const dateObj = new Date(selectedYear, selectedMonth - 1, d);
                const dayOfWeekStr = daysOfWeek[dateObj.getDay()];
                const isToday = (today.getFullYear() === selectedYear && (today.getMonth() + 1) === selectedMonth && today.getDate() === d);
                const todayClass = isToday ? 'today-col' : '';

                rowDaysOfWeekHtml += `<th class="${todayClass}" style="color: ${dateObj.getDay() === 0 ? '#f38ba8' : '#a6adc8'};">${dayOfWeekStr}</th>`;

                // Calculate Lunar Date & Holidays
                let lunarDayStr = "";
                let solarHolidayTitle = "";
                let lunarHolidayTitle = "";
                try {
                    if (typeof window.Lunar !== 'undefined') {
                        const lunar = window.Lunar.fromDate(dateObj);
                        const lD = lunar.getDay();
                        const lM = lunar.getMonth();
                        lunarDayStr = lD === 1 ? `${lD}/${lM}` : `${lD}`;
                        
                        const sD = dateObj.getDate();
                        const sM = dateObj.getMonth() + 1;
                        let solarHols = [];
                        let lunarHols = [];
                        
                        // Solar Holidays
                        const solarKey = `${sD}/${sM}`;
                        const SOLAR_HOLIDAYS = {
                            "1/1": "Tết DL", "9/1": "Ngày HSSV VN", "14/2": "Valentine", "27/2": "Ngày Thầy thuốc VN",
                            "8/3": "Quốc tế Phụ nữ", "20/3": "Quốc tế Hạnh phúc", "26/3": "Thành lập Đoàn", "1/4": "Cá tháng Tư",
                            "30/4": "Giải Phóng MN", "1/5": "Quốc Tế LĐ", "7/5": "Chiến thắng ĐBP", "15/5": "Thành lập Đội",
                            "19/5": "Sinh nhật Bác", "1/6": "Quốc tế Thiếu nhi", "21/6": "Ngày Báo chí VN", "28/6": "Ngày Gia đình VN",
                            "27/7": "Thương binh Liệt sĩ", "19/8": "Cách mạng T8", "2/9": "Quốc Khánh", "1/10": "Người cao tuổi",
                            "10/10": "Giải phóng Thủ đô", "20/10": "Phụ nữ VN", "31/10": "Halloween", "9/11": "Pháp luật VN",
                            "20/11": "Nhà giáo VN", "23/11": "Khởi nghĩa Nam Kỳ", "24/11": "Ngày VH VN", "22/12": "Thành lập QĐND",
                            "24/12": "Giáng sinh (Eve)", "25/12": "Giáng sinh"
                        };
                        
                        if (SOLAR_HOLIDAYS[solarKey]) solarHols.push(SOLAR_HOLIDAYS[solarKey]);
                        if (sD === 1 && sM === 9) solarHols.push("Quốc Khánh"); // Additional day for National Day
                        
                        // Lunar Holidays
                        const lunarKey = `${lD}/${lM}`;
                        const LUNAR_HOLIDAYS = {
                            "15/1": "Tết Nguyên Tiêu", "3/3": "Tết Hàn Thực", "10/3": "Giỗ Tổ HV", "15/4": "Lễ Phật Đản",
                            "5/5": "Tết Đoan Ngọ", "15/7": "Lễ Vu Lan", "15/8": "Tết Trung Thu", "9/9": "Tết Trùng Cửu",
                            "10/10": "Tết Trùng Thập", "15/10": "Tết Hạ Nguyên", "23/12": "Ông Công Ông Táo"
                        };
                        
                        if (LUNAR_HOLIDAYS[lunarKey]) lunarHols.push(LUNAR_HOLIDAYS[lunarKey]);
                        if ((lM === 12 && lD >= 29) || (lM === 1 && lD >= 1 && lD <= 5)) {
                            // Deduplicate if we already added Tet ND
                            if (!lunarHols.includes("Tết NĐ")) lunarHols.push("Tết NĐ");
                        }
                        
                        const HOLIDAY_HISTORY = {
                            "Tết DL": "🎉 Tết Dương lịch (01/01)\nNgày đầu tiên của năm mới theo lịch Gregorius. Đánh dấu sự chuyển giao năm cũ và năm mới trên toàn thế giới.",
                            "Ngày HSSV VN": "🎓 Ngày Học sinh - Sinh viên Việt Nam (09/01)\nKỷ niệm ngày truyền thống của phong trào học sinh, sinh viên và Hội Sinh viên Việt Nam.",
                            "Valentine": "❤️ Lễ Tình nhân (14/02)\nNgày tôn vinh tình yêu đôi lứa trên toàn thế giới, gắn liền với truyền thuyết về vị Thánh tình yêu Valentine.",
                            "Ngày Thầy thuốc VN": "⚕️ Ngày Thầy thuốc Việt Nam (27/02)\nNgày tôn vinh y bác sĩ, gắn liền với bức thư Bác Hồ gửi cán bộ y tế năm 1955: 'Lương y phải như từ mẫu'.",
                            "Quốc tế Phụ nữ": "🌹 Quốc tế Phụ nữ (08/03)\nNgày tôn vinh vẻ đẹp, sự hy sinh của phụ nữ toàn cầu. Ở VN còn là ngày kỷ niệm Khởi nghĩa Hai Bà Trưng hào hùng.",
                            "Quốc tế Hạnh phúc": "😊 Quốc tế Hạnh phúc (20/03)\nNgày lễ của Liên Hợp Quốc truyền tải thông điệp về sự cân bằng, hài hòa và lan tỏa niềm vui trong cuộc sống.",
                            "Thành lập Đoàn": "🚩 Thành lập Đoàn TNCS Hồ Chí Minh (26/03)\nKỷ niệm ngày thành lập lực lượng nòng cốt, tiên phong của thanh niên Việt Nam (1931).",
                            "Cá tháng Tư": "🤡 Cá tháng Tư (01/04)\nNgày hội vui vẻ của những lời nói dối vô hại, mang lại tiếng cười và sự bất ngờ thú vị.",
                            "Giải Phóng MN": "🇻🇳 Giải phóng miền Nam (30/04)\nNgày 30/04/1975, cờ Mặt trận tung bay trên Dinh Độc Lập, đánh dấu sự kiện thống nhất đất nước trọn vẹn.",
                            "Quốc Tế LĐ": "👷 Quốc tế Lao động (01/05)\nBắt nguồn từ cuộc bãi công tại Chicago (Mỹ) năm 1886 đòi quyền làm việc 8 giờ/ngày. Tôn vinh người lao động.",
                            "Chiến thắng ĐBP": "🎖️ Chiến thắng Điện Biên Phủ (07/05)\nKỷ niệm chiến thắng lịch sử năm 1954 'lừng lẫy năm châu, chấn động địa cầu', đập tan ách thực dân Pháp.",
                            "Thành lập Đội": "🧣 Thành lập Đội TNTP Hồ Chí Minh (15/05)\nNgày truyền thống của Đội Thiếu niên Tiền phong, ươm mầm thế hệ tương lai.",
                            "Sinh nhật Bác": "🌻 Sinh nhật Bác (19/05)\nKỷ niệm ngày sinh vị cha già dân tộc, danh nhân văn hóa thế giới Hồ Chí Minh (1890 - 1969).",
                            "Quốc tế Thiếu nhi": "🧸 Quốc tế Thiếu nhi (01/06)\nNgày tết dành riêng cho trẻ em, nhắc nhở toàn nhân loại về quyền và sự bảo vệ trẻ em.",
                            "Ngày Báo chí VN": "📰 Ngày Báo chí Cách mạng VN (21/06)\nKỷ niệm ngày Bác Hồ sáng lập ra tờ báo Thanh Niên (1925), tiếng nói của cách mạng.",
                            "Ngày Gia đình VN": "👨‍👩‍👧‍👦 Ngày Gia đình Việt Nam (28/06)\nNgày tôn vinh, gìn giữ và phát huy những giá trị văn hóa truyền thống tốt đẹp của gia đình Việt.",
                            "Thương binh Liệt sĩ": "🕯️ Thương binh Liệt sĩ (27/07)\nNgày đền ơn đáp nghĩa, tri ân sâu sắc những anh hùng, thương bệnh binh đã đổ máu vì độc lập tự do.",
                            "Cách mạng T8": "⭐ Cách mạng tháng Tám (19/08)\nKỷ niệm thành công của Cách mạng tháng Tám (1945) và Ngày truyền thống lực lượng Công an Nhân dân.",
                            "Quốc Khánh": "🇻🇳 Quốc khánh (02/09)\nNgày 02/09/1945 tại Quảng trường Ba Đình, Bác Hồ đọc Tuyên ngôn Độc lập khai sinh nước Việt Nam Dân chủ Cộng hòa.",
                            "Người cao tuổi": "👵 Quốc tế Người cao tuổi (01/10)\nNgày tôn vinh những đóng góp và nâng cao nhận thức bảo vệ, chăm sóc người cao tuổi.",
                            "Giải phóng Thủ đô": "🕊️ Giải phóng Thủ đô (10/10)\nKỷ niệm ngày đoàn quân chiến thắng tiến về tiếp quản Thủ đô Hà Nội rợp bóng cờ hoa (1954).",
                            "Phụ nữ VN": "🌸 Ngày Phụ nữ Việt Nam (20/10)\nKỷ niệm thành lập Hội Liên hiệp Phụ nữ Việt Nam, tôn vinh người phụ nữ đảm đang, bất khuất.",
                            "Halloween": "🎃 Lễ hội Halloween (31/10)\nLễ hội hóa trang truyền thống mang ý nghĩa xua đuổi tà ma và vui chơi vào đêm trước Lễ Các Thánh.",
                            "Pháp luật VN": "⚖️ Ngày Pháp luật Việt Nam (09/11)\nNgày tôn vinh Hiến pháp, pháp luật và giáo dục ý thức thượng tôn pháp luật cho toàn dân.",
                            "Nhà giáo VN": "📚 Ngày Nhà giáo Việt Nam (20/11)\nNgày truyền thống 'tôn sư trọng đạo', tri ân sâu sắc các thầy cô giáo trong sự nghiệp trồng người cao cả.",
                            "Khởi nghĩa Nam Kỳ": "🔥 Khởi nghĩa Nam Kỳ (23/11)\nKỷ niệm cuộc khởi nghĩa oanh liệt năm 1940, rung chuyển chính quyền thực dân tại Nam Kỳ.",
                            "Ngày VH VN": "🏛️ Ngày Di sản Văn hóa VN (24/11)\nKỷ niệm Hội nghị Văn hóa toàn quốc 1946: 'Văn hóa soi đường cho quốc dân đi'. Tôn vinh di sản dân tộc.",
                            "Thành lập QĐND": "⚔️ Ngày thành lập Quân đội Nhân dân (22/12)\nKỷ niệm thành lập lực lượng vũ trang nhân dân (1944), đội quân 'từ nhân dân mà ra, vì nhân dân mà chiến đấu'.",
                            "Giáng sinh (Eve)": "🎄 Đêm Giáng Sinh (24/12)\nĐêm Thánh vô cùng, thời khắc thiêng liêng chuẩn bị đón mừng Chúa Giê-su giáng sinh.",
                            "Giáng sinh": "⛪ Lễ Giáng Sinh (25/12)\nLễ kỷ niệm Chúa Giê-su ra đời của tín đồ Công giáo, ngày nay đã trở thành một lễ hội văn hóa toàn cầu.",
                            "Tết Nguyên Tiêu": "🏮 Tết Nguyên Tiêu (Rằm tháng Giêng)\nĐêm trăng tròn đầu tiên của năm, người dân thường lên chùa cầu an, thả hoa đăng và ngắm trăng sáng.",
                            "Tết Hàn Thực": "🍡 Tết Hàn Thực (03/03 ÂL)\nTết ăn đồ nguội lạnh. Ở VN, mọi người làm bánh trôi, bánh chay dâng lên cúng tổ tiên với lòng thành kính.",
                            "Giỗ Tổ HV": "🐉 Giỗ Tổ Hùng Vương (10/03 ÂL)\n'Dù ai đi ngược về xuôi/ Nhớ ngày giỗ Tổ mùng Mười tháng Ba'. Lễ hội tín ngưỡng thờ cúng quốc tổ thiêng liêng.",
                            "Lễ Phật Đản": "🪷 Lễ Phật Đản (Vesak - 15/04 ÂL)\nKỷ niệm ngày Đức Phật Thích Ca Mâu Ni đản sinh. Một trong những ngày lễ thiêng liêng, lớn nhất của Phật giáo.",
                            "Tết Đoan Ngọ": "🌿 Tết Đoan Ngọ (05/05 ÂL)\n'Tết diệt sâu bọ', vào giữa trưa (giờ Ngọ), người dân ăn trái cây, cơm rượu nếp để tiêu trừ bệnh tật trong năm.",
                            "Lễ Vu Lan": "🙏🏻 Lễ Vu Lan báo hiếu (15/07 ÂL)\nNgày lễ lớn của Đạo Phật để con cái báo hiếu công ơn sinh thành của cha mẹ. Đồng thời là ngày Xá tội vong nhân.",
                            "Tết Trung Thu": "🥮 Tết Trung Thu (15/08 ÂL)\nTết của thiếu nhi với đèn ông sao, múa lân. Đồng thời là Tết Đoàn viên để gia đình quây quần thưởng trăng, ăn bánh.",
                            "Tết Trùng Cửu": "🌼 Tết Trùng Cửu (09/09 ÂL)\nNgày Tết cổ truyền mang ý nghĩa trường thọ. Thời xưa thường có tục leo núi cao, ngắm hoa cúc nở rộ.",
                            "Tết Trùng Thập": "🌾 Tết Thường Tân (10/10 ÂL)\nTết Cơm mới, lễ tạ ơn thần linh, đất trời đã ban cho một vụ mùa màng bội thu, no ấm.",
                            "Tết Hạ Nguyên": "🌕 Tết Hạ Nguyên (15/10 ÂL)\nRằm tháng Mười, lễ tạ ân thần linh, tổ tiên vào kỳ rằm cuối cùng của năm trước khi đón năm mới.",
                            "Ông Công Ông Táo": "🐟 Tiễn Ông Công Ông Táo (23/12 ÂL)\nNgày Táo Quân cưỡi cá chép bay về trời báo cáo Ngọc Hoàng về những việc làm, sinh hoạt của gia đình trong năm.",
                            "Tết NĐ": "🧨 Tết Nguyên Đán\nTết cổ truyền thiêng liêng và lớn nhất của Việt Nam. Là dịp gia đình sum vầy, tưởng nhớ tổ tiên, hy vọng năm mới an khang."
                        };
                        
                        if (solarHols.length > 0) {
                            solarHolidayTitle = solarHols.map(h => HOLIDAY_HISTORY[h]).join('&#10;&#10;-------------------------&#10;&#10;');
                        }
                        if (lunarHols.length > 0) {
                            lunarHolidayTitle = lunarHols.map(h => HOLIDAY_HISTORY[h]).join('&#10;&#10;-------------------------&#10;&#10;');
                        }
                    }
                } catch(e) {}
                
                let solarTitleAttr = solarHolidayTitle ? `title="${solarHolidayTitle}"` : "";
                let solarStyle = solarHolidayTitle ? `color: #f9e2af; font-weight: bold; cursor: help;` : ``;
                rowDaysHtml += `<th class="${todayClass}" style="${solarStyle}" ${solarTitleAttr}>${d < 10 ? '0' + d : d}${solarHolidayTitle ? '★' : ''}</th>`;
                
                let lunarTitleAttr = lunarHolidayTitle ? `title="${lunarHolidayTitle}"` : "";
                let lunarStyle = lunarHolidayTitle ? `color: #f9e2af; font-weight: bold; cursor: help;` : `color: #f38ba8; font-weight: normal;`;
                rowLunarDaysHtml += `<th class="${todayClass}" style="font-size: 0.85em; ${lunarStyle}" ${lunarTitleAttr}>${lunarDayStr}${lunarHolidayTitle ? '★' : ''}</th>`;
            }

            rowDaysHtml += `</tr>`;
            rowLunarDaysHtml += `</tr>`;
            rowDaysOfWeekHtml += `</tr>`;
            thead.innerHTML = rowDaysHtml + rowLunarDaysHtml + rowDaysOfWeekHtml;
        }

        // 2. Render Rows cho từng Kíp (N, A1..D2)
        tbody.innerHTML = '';
        const teamsToRender = selectedTeam === 'ALL' 
            ? ['N', 'A1', 'A2', 'B1', 'B2', 'C1', 'C2', 'D1', 'D2']
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



    // 3. Cập nhật thẻ tóm tắt Ca làm việc hiện tại
    updateCurrentShiftSummary();
}

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

function updateCurrentShiftSummary() {
    const summaryText = document.getElementById('shiftSummaryText');
    const teamSelect = document.getElementById('selShiftTeam');
    const selectedTeam = teamSelect ? teamSelect.value : 'ALL';
    
    if (summaryText) {
        const currentTodayDate = new Date();
        const timeShift = getCurrentTimeShiftInfo();
        const currentShiftInfo = getShiftLabelAndClass(timeShift.code);

        if (selectedTeam === 'ALL') {
            const allTeams = ['N', 'A1', 'A2', 'B1', 'B2', 'C1', 'C2', 'D1', 'D2'];
            const workingTeams = allTeams.filter(t => getShiftForDate(t, currentTodayDate) === timeShift.code);

            summaryText.innerHTML = `${currentShiftInfo.label} (${currentShiftInfo.time}) — <strong>Đang trực:</strong> ${workingTeams.length > 0 ? workingTeams.map(t => 'Shift ' + t).join(', ') : 'Không có'}`;
        } else {
            const shiftCode = getShiftForDate(selectedTeam, currentTodayDate);
            const activeInfo = getShiftLabelAndClass(shiftCode);
            summaryText.innerHTML = `Shift <strong>${selectedTeam}</strong> hôm nay (${currentTodayDate.getDate()}/${currentTodayDate.getMonth() + 1}): ${activeInfo.label} (${activeInfo.time})`;
        }
    }

    // Cập nhật nhãn Tuần (WW) theo chuẩn Intel (Chủ nhật là .0, tuần bắt đầu từ Chủ nhật)
    const wwBadge = document.getElementById('currentWwBadge');
    if (wwBadge) {
        const dObj = new Date();
        const day = dObj.getDay(); // 0 (CN) -> 6 (T7)
        
        const sunday = new Date(dObj.getFullYear(), dObj.getMonth(), dObj.getDate());
        sunday.setDate(sunday.getDate() - day);
        
        const year = sunday.getFullYear();
        const jan1 = new Date(year, 0, 1);
        const jan1Day = jan1.getDay();
        
        const firstSunday = new Date(year, 0, 1);
        firstSunday.setDate(firstSunday.getDate() - jan1Day);
        
        const ww = Math.floor((sunday - firstSunday) / (7 * 24 * 60 * 60 * 1000)) + 1;
        
        wwBadge.innerText = `WW${ww < 10 ? '0' + ww : ww}.${day}`;
    }
}

    let scheduleAutoRefreshTimer = null;

    function startAutoRefreshTimer() {
        stopAutoRefreshTimer();
        // Cập nhật real-time trạng thái ca hiện tại mỗi 1 giây (rất nhẹ vì chỉ update text)
        scheduleAutoRefreshTimer = setInterval(() => {
            if (isScheduleOpen) {
                if (new Date().toDateString() !== lastRenderedDateString) {
                    // Nếu đã qua ngày mới, render lại toàn bộ để cập nhật cột highlight
                    renderScheduleTable();
                } else {
                    // Nếu vẫn trong ngày cũ, chỉ update text cực nhẹ
                    updateCurrentShiftSummary();
                }
            }
        }, 1000);
    }

    function stopAutoRefreshTimer() {
        if (scheduleAutoRefreshTimer) {
            clearInterval(scheduleAutoRefreshTimer);
            scheduleAutoRefreshTimer = null;
        }
    }

    function toggleSchedulePanel(show) {
        const panel = document.getElementById('schedulePanelLeft');
        const icon = document.getElementById('scheduleArrowIcon');

        isScheduleOpen = typeof show === 'boolean' ? show : !isScheduleOpen;

        if (isScheduleOpen) {
            if (panel) panel.classList.add('open');
            if (icon) icon.textContent = '◀';

            // Tự động đồng bộ Tháng / Năm hiện tại từ màn hình chính (nếu có)
            const mainMonthInput = document.getElementById('month');
            const mainYearInput = document.getElementById('year');
            const selMonth = document.getElementById('selShiftMonth');
            const selYear = document.getElementById('selShiftYear');

            if (mainMonthInput && mainMonthInput.value && selMonth) {
                selMonth.value = mainMonthInput.value;
            }
            if (mainYearInput && mainYearInput.value && selYear) {
                selYear.value = mainYearInput.value;
            }

            renderScheduleTable();
            startAutoRefreshTimer();
        } else {
            if (panel) panel.classList.remove('open');
            if (icon) icon.textContent = '▶';
            stopAutoRefreshTimer();
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
        const mainMonth = document.getElementById('month');
        const mainYear = document.getElementById('year');

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

        // Lắng nghe sự kiện đổi Tháng/Năm ở màn hình chính để tự động đồng bộ lịch
        if (mainMonth) {
            mainMonth.addEventListener('change', () => {
                if (selMonth && mainMonth.value) {
                    selMonth.value = mainMonth.value;
                    if (isScheduleOpen) renderScheduleTable();
                }
            });
        }

        if (mainYear) {
            mainYear.addEventListener('change', () => {
                if (selYear && mainYear.value) {
                    selYear.value = mainYear.value;
                    if (isScheduleOpen) renderScheduleTable();
                }
            });
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
