/**
 * Bridge Adapter for Mobile & Browser
 * Provides a 100% drop-in offline & online data engine when running outside Windows WebView2
 */
(function () {
    const STORAGE_KEY_USERS = 'salary_app_users';
    const STORAGE_KEY_SETTINGS = 'salary_app_settings';

    // Helper to get all users from LocalStorage
    function getStoredUsers() {
        try {
            const raw = localStorage.getItem(STORAGE_KEY_USERS);
            if (!raw) return {};
            return JSON.parse(raw);
        } catch (e) {
            console.error("Error reading stored users:", e);
            return {};
        }
    }

    // Helper to save all users to LocalStorage
    function saveStoredUsers(users) {
        try {
            localStorage.setItem(STORAGE_KEY_USERS, JSON.stringify(users));
        } catch (e) {
            console.error("Error saving users to localStorage:", e);
        }
    }

    function getPayDate(year, month) {
        const lastDay = new Date(year, month, 0).getDate();
        let payDate = new Date(year, month - 1, lastDay);
        const dayOfWeek = payDate.getDay();
        if (dayOfWeek === 6) { // Saturday
            payDate.setDate(payDate.getDate() - 1);
        } else if (dayOfWeek === 0) { // Sunday
            payDate.setDate(payDate.getDate() - 2);
        }
        return payDate;
    }

    function computeEffectiveMonthlyAllowance(monthlyBase, workingDays) {
        monthlyBase = Math.max(0, Number(monthlyBase) || 0);
        if (workingDays > 23) {
            monthlyBase += 8500 * (workingDays - 23);
        }
        return monthlyBase;
    }

    function computeProratedAllowance(monthlyBase, workingDays, actualWorkedDays) {
        if (workingDays <= 0 || actualWorkedDays <= 0) return 0;
        const effectiveMonthly = computeEffectiveMonthlyAllowance(monthlyBase, workingDays);
        const prorated = (effectiveMonthly / workingDays) * actualWorkedDays;
        return Math.round(prorated);
    }

    function computeTaxThreshold(baseThreshold, hourlyRate, otFwdHours, ot2xHours, ot3xHours, ot15xHours, insuranceDeduction) {
        const ot2xSalary = Math.round(ot2xHours * hourlyRate * 2.0);
        const ot3xSalary = Math.round(ot3xHours * hourlyRate * 3.0);
        const ot15xSalary = Math.round(ot15xHours * hourlyRate * 1.5);

        const additionalOTx2 = ot2xSalary / 2.0;
        const additionalOTx3 = (ot3xSalary * 2.0) / 3.0;
        const additionalOTx15 = ot15xSalary / 3.0;
        const FixedThresholdAddon = 730000;

        return baseThreshold + insuranceDeduction + FixedThresholdAddon + additionalOTx2 + additionalOTx3 + additionalOTx15;
    }

    function getMidMonthWorkingDays(month, year) {
        if (month <= 0 || year <= 0) return { w1: 10, w2: 12 };
        const m1 = month === 1 ? 12 : month - 1;
        const y1 = month === 1 ? year - 1 : year;
        const daysInPrevMonth = new Date(y1, m1, 0).getDate();

        let w1 = 0;
        if (daysInPrevMonth >= 21) {
            for (let d = 21; d <= daysInPrevMonth; d++) {
                const date = new Date(y1, m1 - 1, d);
                const day = date.getDay();
                if (day !== 0 && day !== 6) w1++;
            }
        }

        let w2 = 0;
        for (let d = 1; d <= 20; d++) {
            const date = new Date(year, month - 1, d);
            const day = date.getDay();
            if (day !== 0 && day !== 6) w2++;
        }

        return { w1, w2 };
    }

    const reviewQuotesTop1 = [
        "Gánh team quá đỉnh, tháng này xứng đáng dẫn đầu!",
        "Phong độ xuất sắc, thu nhập tháng này quá ấm áp.",
        "Cật lực cả tháng qua, nghỉ ngơi xả hơi thôi em!",
        "Dẫn đầu tuyệt đối, không ai đuổi kịp luôn nha.",
        "Xử lý công việc sắc bén, duyệt thưởng nóng tháng này!",
        "Lương thưởng bùng nổ, xứng danh trụ cột công ty!",
        "Hiệu suất kỷ lục, duy trì phong độ đỉnh cao nhé!",
        "Làm việc hết mình, kết quả quá xứng đáng luôn.",
        "Tháng này làm quá tốt, khao anh em chấu bia thôi!",
        "Out trình hoàn toàn, sếp cực kỳ tự hào về em."
    ];

    const reviewQuotesTop2 = [
        "Bám đuổi Top 1 suýt soát, tháng sau bứt phá nhé!",
        "Cống hiến rất ấn tượng, suýt nữa là lên ngôi rồi.",
        "Phong độ ổn định lắm, cứ thế này sớm thăng tiến!",
        "Tháng này làm rất tốt, giữ vững đà tiến này nha.",
        "Hiệu suất cực kỳ mượt, tay phải đắc lực của sếp!",
        "Nỗ lực thấy rõ luôn, tháng tới quyết tâm leo Top!",
        "Sát nút vị trí dẫn đầu, cố lên một chút nữa thôi!"
    ];

    const reviewQuotesTop3 = [
        "Vào Top 3 VIP là quá đẳng cấp rồi, chúc mừng!",
        "Làm việc rất có gu và hiệu quả, phát huy nhé!",
        "Nỗ lực tuyệt vời, vị trí Top 3 rất xứng đáng.",
        "Giữ phong độ tốt lắm, áp sát Top 1 ngay thôi!",
        "Công việc mượt mà, thưởng quý này cực kỳ tốt.",
        "Duy trì nhịp độ này nhé, cơ hội thăng tiến rộng mở!"
    ];

    const reviewQuotesGeneral = [
        "Nhích thêm chút nữa là lọt Top 3 rồi, cố lên!",
        "Âm thầm tích lũy, tháng sau bùng nổ chắc luôn.",
        "Phong độ đang lên rất đều, bám sát Top trên nha.",
        "Làm việc chắc chắn lắm, cơ hội thăng tiến gần rồi.",
        "Đang tích lũy công lực, tháng tới bùng nổ nhé!",
        "Tháng này làm việc rất năng nổ, tiếp tục duy trì nhé!"
    ];

    function getCeoReviewComment(rank, name, netSalary, usedComments) {
        let pool = reviewQuotesGeneral;
        if (rank === 1) pool = reviewQuotesTop1;
        else if (rank === 2) pool = reviewQuotesTop2;
        else if (rank === 3) pool = reviewQuotesTop3;

        for (const comment of pool) {
            if (!usedComments.has(comment)) return comment;
        }
        return pool[Math.floor(Math.random() * pool.length)] + " ✨";
    }

    // Drop-in LocalBackend implementation
    window.localBackend = {
        async GetCurrentPayrollPeriod() {
            try {
                const now = new Date();
                let month = now.getMonth() + 1;
                let year = now.getFullYear();

                const payDate = getPayDate(year, month);
                const todayMidnight = new Date(now.getFullYear(), now.getMonth(), now.getDate());

                if (todayMidnight.getTime() > payDate.getTime()) {
                    month += 1;
                    if (month > 12) {
                        month = 1;
                        year += 1;
                    }
                }
                return JSON.stringify({ success: true, month, year });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async Login(username) {
            try {
                if (!username) return JSON.stringify({ success: false, message: "Username không được để trống" });
                const users = getStoredUsers();
                const key = username.trim().toLowerCase();
                let user = users[key];

                // Nếu thiết bị mới cài chưa có dữ liệu tài khoản này, kiểm tra trực tiếp trên Cloud
                if (!user && window.CloudSync && typeof window.CloudSync.fetchUserDirectly === 'function') {
                    try {
                        const cloudUser = await window.CloudSync.fetchUserDirectly(username);
                        if (cloudUser && cloudUser.Username) {
                            user = cloudUser;
                            users[key] = user;
                            saveStoredUsers(users);
                            console.log(`[BridgeAdapter] Đã tự động đồng bộ tài khoản ${username} từ Cloud về máy.`);
                        }
                    } catch (errCloud) {
                        console.warn("[BridgeAdapter] Lỗi kiểm tra Cloud:", errCloud);
                    }
                }

                if (user) {
                    return JSON.stringify({ success: true, user });
                }
                return JSON.stringify({ success: false, message: "Tài khoản chưa được đăng ký", needsRegistration: true });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async RegisterUser(payloadJson) {
            try {
                const payload = JSON.parse(payloadJson);
                const users = getStoredUsers();
                const key = payload.username.trim().toLowerCase();

                const newUser = {
                    Username: payload.username,
                    FullName: payload.fullName || payload.username,
                    BasicSalary: Number(payload.basicSalary) || 0,
                    MealAllowance: Number(payload.mealAllowance) || 0,
                    TravelAllowance: Number(payload.travelAllowance) || 195500,
                    HousingAllowance: Number(payload.housingAllowance) || 100000,
                    AttendanceIncentive: Number(payload.attendanceIncentive) || 195500,
                    CertificateBonus: Number(payload.certificateBonus) || 0,
                    Allowance: Number(payload.otherBonus) || 0,
                    InsurancePercent: Number(payload.insurancePercent) || 10.5,
                    TaxThreshold: Number(payload.taxThreshold) || 11000000,
                    PerformanceBonus: Number(payload.performanceBonus) || 900000,
                    PerfDeduct1: 500000,
                    PerfDeduct2: 700000,
                    OtMeal12Amount: 30000,
                    OtMeal8Amount: 20000,
                    SalaryHistory: {},
                    SalaryResultHistory: {}
                };

                users[key] = newUser;
                saveStoredUsers(users);

                if (window.CloudSync && typeof window.CloudSync.pushUserToCloud === 'function') {
                    window.CloudSync.pushUserToCloud(newUser);
                }

                return JSON.stringify({ success: true, user: newUser });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async UpdateProfile(payloadJson) {
            try {
                const payload = JSON.parse(payloadJson);
                const users = getStoredUsers();
                const key = payload.username.trim().toLowerCase();
                let user = users[key] || { Username: payload.username, SalaryHistory: {}, SalaryResultHistory: {} };

                user.FullName = payload.fullName || user.FullName || user.Username;
                user.BasicSalary = Number(payload.basicSalary) || 0;
                user.MealAllowance = Number(payload.mealAllowance) || 0;
                user.TravelAllowance = Number(payload.travelAllowance) || 0;
                user.HousingAllowance = Number(payload.housingAllowance) || 0;
                user.AttendanceIncentive = Number(payload.attendanceIncentive) || 0;
                user.CertificateBonus = Number(payload.certificateBonus) || 0;
                user.Allowance = Number(payload.otherBonus) || 0;
                user.InsurancePercent = Number(payload.insurancePercent) || 0;
                user.TaxThreshold = Number(payload.taxThreshold) || 0;

                if (payload.performanceBonus !== undefined) user.PerformanceBonus = Number(payload.performanceBonus);
                if (payload.perfDeduct1 !== undefined) user.PerfDeduct1 = Number(payload.perfDeduct1);
                if (payload.perfDeduct2 !== undefined) user.PerfDeduct2 = Number(payload.perfDeduct2);
                if (payload.otMeal12Amount !== undefined) user.OtMeal12Amount = Number(payload.otMeal12Amount);
                if (payload.otMeal8Amount !== undefined) user.OtMeal8Amount = Number(payload.otMeal8Amount);

                users[key] = user;
                saveStoredUsers(users);

                if (window.CloudSync && typeof window.CloudSync.pushUserToCloud === 'function') {
                    window.CloudSync.pushUserToCloud(user);
                }

                return JSON.stringify({ success: true, user });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async CalculateSalary(payloadJson) {
            try {
                const payload = JSON.parse(payloadJson);
                const users = getStoredUsers();
                const userKey = payload.username ? payload.username.trim().toLowerCase() : '';
                const user = users[userKey] || null;

                const workingDays = payload.workingDays > 0 ? Number(payload.workingDays) : 1;
                const meal12Amount = payload.otMeal12Amount > 0 ? Number(payload.otMeal12Amount) : (user && user.OtMeal12Amount > 0 ? user.OtMeal12Amount : 30000);
                const meal8Amount = payload.otMeal8Amount > 0 ? Number(payload.otMeal8Amount) : (user && user.OtMeal8Amount > 0 ? user.OtMeal8Amount : 20000);

                let bonusMealAllowance = 0;
                if (payload.otDays12 > 0) bonusMealAllowance += payload.otDays12 * meal12Amount;
                if (payload.otDays8 > 0) bonusMealAllowance += payload.otDays8 * meal8Amount;

                const basicDailySalary = payload.basicSalary / workingDays;
                const mealDailySalary = payload.mealAllowance / workingDays;
                const dailySalaryForMeal = basicDailySalary + mealDailySalary;
                const hourlyRate = Math.round((basicDailySalary / 8) * 1000) / 1000;
                const insurancePercent = payload.insurancePercent < 0 ? 0 : Number(payload.insurancePercent);

                let insuranceDeduction = 0;
                let slSalaryDeduction = 0;
                let regularSalary = 0;
                let overtime15xSalary = 0;
                let overtime2xSalary = 0;
                let overtime3xSalary = 0;
                let allowanceEligibleDays = 0;

                let w1 = 0, w2 = 0;
                let workedDays1 = 0, workedDays2 = 0;
                let basicDaily1 = 0, basicDaily2 = 0;
                let ot15xSalary1 = 0, ot2xSalary1 = 0, ot3xSalary1 = 0;
                let ot15xSalary2 = 0, ot2xSalary2 = 0, ot3xSalary2 = 0;
                let slSalaryDeduction1 = 0, slSalaryDeduction2 = 0;

                if (payload.isMidMonthSalaryChange) {
                    const midDays = getMidMonthWorkingDays(payload.month, payload.year);
                    w1 = midDays.w1;
                    w2 = midDays.w2;
                    const totalWorkingDaysMid = (w1 + w2) > 0 ? (w1 + w2) : workingDays;

                    const oldBasic = payload.oldBasicSalary > 0 ? Number(payload.oldBasicSalary) : Number(payload.basicSalary);
                    const newBasic = payload.newBasicSalary > 0 ? Number(payload.newBasicSalary) : Number(payload.basicSalary);

                    basicDaily1 = oldBasic / totalWorkingDaysMid;
                    basicDaily2 = newBasic / totalWorkingDaysMid;
                    const mealDailyMid = payload.mealAllowance / totalWorkingDaysMid;

                    const dailyRate1 = basicDaily1 + mealDailyMid;
                    const dailyRate2 = basicDaily2 + mealDailyMid;

                    const hourlyRate1 = Math.round((basicDaily1 / 8) * 1000) / 1000;
                    const hourlyRate2 = Math.round((basicDaily2 / 8) * 1000) / 1000;

                    workedDays1 = Math.max(0, w1 - (Number(payload.slDaysOff1) || 0));
                    workedDays2 = Math.max(0, w2 - (Number(payload.slDaysOff2) || 0));

                    slSalaryDeduction1 = (Number(payload.slDaysOff1) || 0) * dailyRate1;
                    slSalaryDeduction2 = (Number(payload.slDaysOff2) || 0) * dailyRate2;
                    slSalaryDeduction = slSalaryDeduction1 + slSalaryDeduction2;

                    const regularSalary1 = workedDays1 * dailyRate1;
                    const regularSalary2 = workedDays2 * dailyRate2;
                    regularSalary = regularSalary1 + regularSalary2;

                    ot15xSalary1 = Math.round((Number(payload.overtime15x1) || 0) * hourlyRate1 * 1.5);
                    ot2xSalary1 = Math.round((Number(payload.overtime2x1) || 0) * hourlyRate1 * 2.0);
                    ot3xSalary1 = Math.round((Number(payload.overtime3x1) || 0) * hourlyRate1 * 3.0);

                    ot15xSalary2 = Math.round((Number(payload.overtime15x2) || 0) * hourlyRate2 * 1.5);
                    ot2xSalary2 = Math.round((Number(payload.overtime2x2) || 0) * hourlyRate2 * 2.0);
                    ot3xSalary2 = Math.round((Number(payload.overtime3x2) || 0) * hourlyRate2 * 3.0);

                    overtime15xSalary = ot15xSalary1 + ot15xSalary2;
                    overtime2xSalary = ot2xSalary1 + ot2xSalary2;
                    overtime3xSalary = ot3xSalary1 + ot3xSalary2;

                    insuranceDeduction = Math.round(newBasic * (insurancePercent / 100));
                    allowanceEligibleDays = Math.max(0, workedDays1 + workedDays2 - (Number(payload.alDaysOff) || 0));
                } else {
                    insuranceDeduction = Math.round(payload.basicSalary * (insurancePercent / 100));
                    slSalaryDeduction = (Number(payload.slDaysOff) || 0) * dailySalaryForMeal;
                    const baseRegularSalary = workingDays * dailySalaryForMeal;
                    regularSalary = Math.max(0, baseRegularSalary - slSalaryDeduction);

                    overtime2xSalary = Math.round((Number(payload.overtime2x) || 0) * hourlyRate * 2.0);
                    overtime3xSalary = Math.round((Number(payload.overtime3x) || 0) * hourlyRate * 3.0);
                    overtime15xSalary = Math.round((Number(payload.overtime15x) || 0) * hourlyRate * 1.5);

                    allowanceEligibleDays = Math.max(0, workingDays - (Number(payload.slDaysOff) || 0) - (Number(payload.alDaysOff) || 0));
                }

                const ot2xHoursTotal = payload.isMidMonthSalaryChange ? ((Number(payload.overtime2x1) || 0) + (Number(payload.overtime2x2) || 0)) : (Number(payload.overtime2x) || 0);
                const ot3xHoursTotal = payload.isMidMonthSalaryChange ? ((Number(payload.overtime3x1) || 0) + (Number(payload.overtime3x2) || 0)) : (Number(payload.overtime3x) || 0);
                const ot15xHoursTotal = payload.isMidMonthSalaryChange ? ((Number(payload.overtime15x1) || 0) + (Number(payload.overtime15x2) || 0)) : (Number(payload.overtime15x) || 0);

                const taxThreshold = computeTaxThreshold(Number(payload.taxThreshold) || 11000000, hourlyRate, 0, ot2xHoursTotal, ot3xHoursTotal, ot15xHoursTotal, insuranceDeduction);

                const travelAllowance = computeProratedAllowance(payload.travelAllowance, workingDays, allowanceEligibleDays);
                const attendanceIncentive = computeProratedAllowance(payload.attendanceIncentive, workingDays, allowanceEligibleDays);

                const defaultPerfBonus = user && user.PerformanceBonus > 0 ? user.PerformanceBonus : 900000;
                const leaveDays = payload.isMidMonthSalaryChange ?
                    ((Number(payload.slDaysOff1) || 0) + (Number(payload.slDaysOff2) || 0) + (Number(payload.alDaysOff) || 0)) :
                    ((Number(payload.slDaysOff) || 0) + (Number(payload.alDaysOff) || 0));

                let actualPerformanceBonus = Number(payload.performanceBonus) || 0;
                const isDeductionActive = (actualPerformanceBonus === defaultPerfBonus) ||
                    (actualPerformanceBonus === 400000) ||
                    (actualPerformanceBonus === 850000) ||
                    (actualPerformanceBonus === 875000);

                if (isDeductionActive) {
                    const perfDeduct1 = (user && user.PerfDeduct1) || 500000;
                    const perfDeduct2 = (user && user.PerfDeduct2) || 700000;
                    if (leaveDays > 0 && leaveDays <= 1) {
                        actualPerformanceBonus -= perfDeduct1;
                    } else if (leaveDays > 1 && leaveDays <= 2) {
                        actualPerformanceBonus -= perfDeduct2;
                    } else if (leaveDays > 2) {
                        actualPerformanceBonus = 0;
                    }
                    if (actualPerformanceBonus < 0) actualPerformanceBonus = 0;
                }

                const totalIncentive = travelAllowance + attendanceIncentive + (Number(payload.housingAllowance) || 0) + (Number(payload.certificateBonus) || 0) + (Number(payload.otherBonus) || 0) + actualPerformanceBonus;
                const grossSalary = regularSalary + overtime2xSalary + overtime3xSalary + overtime15xSalary + bonusMealAllowance + totalIncentive;

                const taxBase = grossSalary - taxThreshold;
                let taxRate = 0;
                if (taxBase <= 0) taxRate = 0;
                else if (taxBase <= 10000000) taxRate = 0.05;
                else if (taxBase <= 30000000) taxRate = 0.10;
                else if (taxBase <= 60000000) taxRate = 0.20;
                else if (taxBase <= 100000000) taxRate = 0.30;
                else taxRate = 0.35;

                const taxDeduction = taxBase > 0 ? Math.floor(taxBase * taxRate) : 0;
                const netSalary = grossSalary - insuranceDeduction - taxDeduction;

                const net = netSalary;
                const gross = grossSalary;

                if (payload.month > 0) {
                    const detailObject = {
                        gross, net, tax: taxDeduction, insurance: insuranceDeduction,
                        basicSalary: payload.basicSalary, workingDays: payload.workingDays,
                        isMidMonthSalaryChange: payload.isMidMonthSalaryChange,
                        oldBasicSalary: payload.oldBasicSalary, newBasicSalary: payload.newBasicSalary,
                        w1, w2, workedDays1, workedDays2,
                        regularSalary1: (workedDays1 * basicDaily1),
                        regularSalary2: (workedDays2 * basicDaily2),
                        slDaysOff1: payload.slDaysOff1, slDaysOff2: payload.slDaysOff2,
                        slDeduction1: slSalaryDeduction1, slDeduction2: slSalaryDeduction2,
                        overtime15x1: payload.overtime15x1, overtime2x1: payload.overtime2x1, overtime3x1: payload.overtime3x1,
                        overtime15x2: payload.overtime15x2, overtime2x2: payload.overtime2x2, overtime3x2: payload.overtime3x2,
                        ot15xSalary1, ot2xSalary1, ot3xSalary1, ot15xSalary2, ot2xSalary2, ot3xSalary2,
                        alDaysOff: payload.alDaysOff, slDaysOff: payload.slDaysOff, slDeduction: slSalaryDeduction,
                        overtime15x: ot15xHoursTotal, overtime2x: ot2xHoursTotal, overtime3x: ot3xHoursTotal,
                        overtime15xSalary, overtime2xSalary, overtime3xSalary,
                        otDays8: payload.otDays8, otDays12: payload.otDays12,
                        otMeal8Amount: meal8Amount, otMeal12Amount: meal12Amount, bonusMeal: bonusMealAllowance,
                        mealAllowance: payload.mealAllowance, baseTravelAllowance: payload.travelAllowance,
                        travelAllowance, travelDeduction: payload.travelAllowance - travelAllowance,
                        housingAllowance: payload.housingAllowance, baseAttendanceIncentive: payload.attendanceIncentive,
                        attendanceIncentive, attendanceDeduction: payload.attendanceIncentive - attendanceIncentive,
                        certificateBonus: payload.certificateBonus, basePerformanceBonus: payload.performanceBonus,
                        performanceBonus: actualPerformanceBonus, perfDeduction: payload.performanceBonus - actualPerformanceBonus,
                        otherBonus: payload.otherBonus
                    };
                    const detailJson = JSON.stringify(detailObject);

                    if (user) {
                        const key = `${String(payload.month).padStart(2, '0')}-${payload.year}`;
                        if (!user.SalaryHistory) user.SalaryHistory = {};
                        user.SalaryHistory[key] = net;

                        if (!user.SalaryResultHistory) user.SalaryResultHistory = {};
                        user.SalaryResultHistory[key] = detailJson;

                        const now = new Date();
                        if (payload.month === (now.getMonth() + 1) && payload.year === now.getFullYear()) {
                            user.LastCalculatedMonth = payload.month;
                            user.LastCalculatedYear = payload.year;
                            user.LastNetSalary = net;
                        }

                        users[userKey] = user;
                        saveStoredUsers(users);

                        if (window.CloudSync && typeof window.CloudSync.pushUserToCloud === 'function') {
                            window.CloudSync.pushUserToCloud(user);
                        }
                    }
                }

                return JSON.stringify({ success: true, gross, tax: taxDeduction, insurance: insuranceDeduction, net });
            } catch (ex) {
                console.error("CalculateSalary error:", ex);
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async GetSalaryHistory(username) {
            try {
                const users = getStoredUsers();
                const key = username.trim().toLowerCase();
                const user = users[key];

                if (!user || !user.SalaryHistory) {
                    return JSON.stringify({ success: true, history: [] });
                }

                const historyList = [];
                for (const period in user.SalaryHistory) {
                    if (period.startsWith("00/") || period.startsWith("0/") || period.startsWith("00-") || period.startsWith("0-")) {
                        continue;
                    }
                    const netSalary = user.SalaryHistory[period];
                    const detail = user.SalaryResultHistory ? user.SalaryResultHistory[period] : null;
                    historyList.push({ period, netSalary, detail });
                }

                return JSON.stringify({ success: true, history: historyList });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async DeleteSalaryHistoryEntry(username, period) {
            try {
                const users = getStoredUsers();
                const key = username.trim().toLowerCase();
                const user = users[key];

                if (!user) return JSON.stringify({ success: false, message: "Không tìm thấy người dùng" });

                if (user.SalaryHistory && user.SalaryHistory[period] !== undefined) {
                    delete user.SalaryHistory[period];
                }
                if (user.SalaryResultHistory && user.SalaryResultHistory[period] !== undefined) {
                    delete user.SalaryResultHistory[period];
                }

                users[key] = user;
                saveStoredUsers(users);

                if (window.CloudSync && typeof window.CloudSync.pushUserToCloud === 'function') {
                    window.CloudSync.pushUserToCloud(user);
                }

                return JSON.stringify({ success: true });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async GetRanking(month, year) {
            try {
                const users = getStoredUsers();
                const periodKey = `${String(month).padStart(2, '0')}-${year}`;

                const list = [];
                for (const k in users) {
                    const u = users[k];
                    if (u.SalaryHistory && u.SalaryHistory[periodKey] && u.SalaryHistory[periodKey] > 0) {
                        list.push({
                            username: u.Username,
                            name: u.FullName || u.Username,
                            netSalary: u.SalaryHistory[periodKey]
                        });
                    }
                }

                list.sort((a, b) => b.netSalary - a.netSalary);

                const usedComments = new Set();
                const rankingList = list.map((item, idx) => {
                    const rank = idx + 1;
                    const comment = getCeoReviewComment(rank, item.name, item.netSalary, usedComments);
                    usedComments.add(comment);
                    return {
                        rank,
                        name: item.name,
                        netSalary: item.netSalary,
                        comment
                    };
                });

                return JSON.stringify({ success: true, ranking: rankingList });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async GetAllUsersJson() {
            try {
                const users = getStoredUsers();
                const userList = Object.values(users);
                return JSON.stringify({ success: true, users: userList });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        },

        async SaveRawUserJson(username, jsonStr) {
            try {
                const users = getStoredUsers();
                const key = username.trim().toLowerCase();
                users[key] = JSON.parse(jsonStr);
                saveStoredUsers(users);
                return JSON.stringify({ success: true });
            } catch (ex) {
                return JSON.stringify({ success: false, message: ex.message });
            }
        }
    };
})();
