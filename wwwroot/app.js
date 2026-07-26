// UI Elements
const screens = {
    login: document.getElementById('loginScreen'),
    register: document.getElementById('registerScreen'),
    profile: document.getElementById('profileScreen'),
    main: document.getElementById('mainScreen')
};

const loginBtn = document.getElementById('btnLogin');
const logoutBtn = document.getElementById('btnLogout');
const calcBtn = document.getElementById('btnCalculate');
const usernameInput = document.getElementById('usernameInput');
const loginError = document.getElementById('loginError');
const welcomeText = document.getElementById('welcomeText');

// Register & Profile Elements
const btnRegister = document.getElementById('btnRegister');
const btnCancelReg = document.getElementById('btnCancelReg');
const regError = document.getElementById('regError');

const btnSaveProfile = document.getElementById('btnSaveProfile');
const btnCancelProfile = document.getElementById('btnCancelProfile');
const profError = document.getElementById('profError');

// Inputs mapping
const inputs = {
    month: document.getElementById('month'),
    year: document.getElementById('year'),
    workingDays: document.getElementById('workingDays'),
    basicSalary: document.getElementById('basicSalary'),
    mealAllowance: document.getElementById('mealAllowance'),
    travelAllowance: document.getElementById('travelAllowance'),
    housingAllowance: document.getElementById('housingAllowance'),
    otherBonus: document.getElementById('otherBonus'),
    alDaysOff: document.getElementById('alDaysOff'),
    slDaysOff: document.getElementById('slDaysOff'),
    overtime15x: document.getElementById('overtime15x'),
    overtime2x: document.getElementById('overtime2x'),
    overtime3x: document.getElementById('overtime3x'),
    otDays8: document.getElementById('otDays8'),
    otDays12: document.getElementById('otDays12'),
    attendanceIncentive: document.getElementById('attendanceIncentive'),
    certificateBonus: document.getElementById('certificateBonus'),
    performanceBonus: document.getElementById('performanceBonus'),
    insurancePercent: document.getElementById('insurancePercent'),
    taxThreshold: document.getElementById('taxThreshold')
};

// Outputs
const resGross = document.getElementById('resGross');
const resInsurance = document.getElementById('resInsurance');
const resTax = document.getElementById('resTax');
const resNet = document.getElementById('resNet');

let currentUser = null;

// Utility to parse formatted numbers (e.g. 15,000,000 -> 15000000 or 1.5 -> 1.5)
function parseNumber(val) {
    if (typeof val === 'number') return val < 0 ? 0 : val;
    if (!val) return 0;
    let str = val.toString().trim();
    let num = parseFloat(str.replace(/,/g, '')) || 0;
    if (isNaN(num)) {
        num = parseFloat(str.replace(/,/g, '.')) || 0;
    }
    return num < 0 ? 0 : num;
}

function initNonNegativeInputs() {
    const sanitizeInput = (input, isBlur = false) => {
        let val = input.value;
        if (!val || val.trim() === "") {
            if (isBlur && input.id !== 'month' && input.id !== 'year') {
                input.value = input.classList.contains('currency-input') ? "0" : 0;
            }
            return;
        }
        
        if (val.includes('-') || parseNumber(val) < 0) {
            if (input.classList.contains('currency-input')) {
                let cleanVal = val.replace(/-/g, '');
                let num = parseNumber(cleanVal);
                input.value = num <= 0 ? "0" : formatCurrencyInput(num);
            } else {
                let num = parseFloat(val.replace(/,/g, '.'));
                input.value = (isNaN(num) || num < 0) ? 0 : num;
            }
        }
    };

    document.querySelectorAll('input[type="number"], input.currency-input').forEach(input => {
        // Prevent keydown of '-' key (minus sign)
        input.addEventListener('keydown', (e) => {
            if (e.key === '-' || e.key === 'Subtract') {
                e.preventDefault();
            }
        });

        input.addEventListener('input', function() { sanitizeInput(this, false); });
        input.addEventListener('change', function() { sanitizeInput(this, false); });
        input.addEventListener('blur', function() { sanitizeInput(this, true); });
    });
}

// Utility to format number as currency
function formatCurrencyInput(val) {
    if (val === undefined || val === null) return "";
    let str = val.toString().replace(/[^\d]/g, '');
    if (!str) return "";
    return str.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

function setCursorByDigitCount(input, digitCount) {
    if (digitCount <= 0) {
        input.setSelectionRange(0, 0);
        return;
    }
    const val = input.value;
    let count = 0;
    let targetIndex = val.length;
    for (let i = 0; i < val.length; i++) {
        if (/\d/.test(val[i])) {
            count++;
            if (count === digitCount) {
                targetIndex = i + 1;
                break;
            }
        }
    }
    input.setSelectionRange(targetIndex, targetIndex);
}

function initCurrencyInputs() {
    document.querySelectorAll('.currency-input').forEach(input => {
        // Handle Backspace on commas
        input.addEventListener('keydown', function(e) {
            if (e.key === 'Backspace') {
                const pos = this.selectionStart;
                if (pos > 0 && this.selectionEnd === pos) {
                    if (this.value[pos - 1] === ',') {
                        e.preventDefault();
                        const val = this.value;
                        const newVal = val.slice(0, pos - 2) + val.slice(pos);
                        const digitsBefore = (val.slice(0, pos - 2).match(/\d/g) || []).length;
                        this.value = formatCurrencyInput(newVal);
                        setCursorByDigitCount(this, digitsBefore);
                    }
                }
            }
        });

        // Format on type with precise digit-count cursor tracking
        input.addEventListener('input', function() {
            const val = this.value;
            const cursorPos = this.selectionStart;
            const digitsBeforeCursor = (val.slice(0, cursorPos).match(/\d/g) || []).length;
            
            this.value = formatCurrencyInput(val);
            setCursorByDigitCount(this, digitsBeforeCursor);
        });
        
        // Initial format
        input.value = formatCurrencyInput(input.value);
    });
}

// Auto/Manual Working Days Toggle Logic
const workingDaysModeToggle = document.getElementById('workingDaysModeToggle');
const workingDaysModeBadge = document.getElementById('workingDaysModeBadge');

const updateWorkingDaysToggleUI = () => {
    if (!workingDaysModeToggle || !workingDaysModeBadge) return;
    const isManual = workingDaysModeToggle.checked;
    
    if (isManual) {
        workingDaysModeBadge.textContent = 'MANUAL';
        workingDaysModeBadge.className = 'mode-badge mode-manual';
        inputs.workingDays.removeAttribute('readonly');
        inputs.workingDays.classList.remove('auto-mode');
        inputs.workingDays.classList.add('manual-mode');
    } else {
        workingDaysModeBadge.textContent = 'AUTO';
        workingDaysModeBadge.className = 'mode-badge mode-auto';
        inputs.workingDays.setAttribute('readonly', 'readonly');
        inputs.workingDays.classList.remove('manual-mode');
        inputs.workingDays.classList.add('auto-mode');
        calculateWorkingDays(true);
    }
};

if (workingDaysModeToggle) {
    workingDaysModeToggle.addEventListener('change', updateWorkingDaysToggleUI);
}

// Auto-calculate working days based on month and year (21st of prev to 20th of current)
const calculateWorkingDays = (force = false) => {
    if (workingDaysModeToggle && workingDaysModeToggle.checked && !force) {
        return;
    }
    const m = parseInt(inputs.month.value);
    const y = parseInt(inputs.year.value);
    if (!m || !y || m < 1 || m > 12) return;
    
    let startMonth = m === 1 ? 12 : m - 1;
    let startYear = m === 1 ? y - 1 : y;
    
    let start = new Date(startYear, startMonth - 1, 21);
    let end = new Date(y, m - 1, 20);
    
    let days = 0;
    for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        let day = d.getDay();
        if (day !== 0 && day !== 6) days++;
    }
    inputs.workingDays.value = days;
};

const handleMonthChange = (e) => {
    let val = parseInt(e.target.value);
    if (isNaN(val)) return;
    
    let currentYear = parseInt(inputs.year.value) || new Date().getFullYear();
    if (val > 12) {
        inputs.month.value = 1;
        inputs.year.value = currentYear + 1;
    } else if (val < 1) {
        inputs.month.value = 12;
        inputs.year.value = currentYear - 1;
    }
    calculateWorkingDays();
};

inputs.month.addEventListener('change', handleMonthChange);
inputs.month.addEventListener('input', handleMonthChange);
inputs.year.addEventListener('change', () => calculateWorkingDays());
inputs.year.addEventListener('input', () => calculateWorkingDays());

// Helpers
const formatCurrency = (amount) => {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
};

const switchScreen = (screenName) => {
    Object.values(screens).forEach(s => s.classList.remove('active'));
    setTimeout(() => {
        Object.values(screens).forEach(s => s.classList.add('hidden'));
        screens[screenName].classList.remove('hidden');
        setTimeout(() => screens[screenName].classList.add('active'), 50);
    }, 500); // Wait for transition
};

// C# Interop wrapper
const getBackend = () => {
    if (window.chrome && window.chrome.webview && window.chrome.webview.hostObjects) {
        return window.chrome.webview.hostObjects.sync.backend;
    }
    return null;
};

// Events
loginBtn.addEventListener('click', async () => {
    const username = usernameInput.value.trim();
    if (!username) {
        showError("Vui lòng nhập Username");
        return;
    }

    loginBtn.innerHTML = "Đang xử lý...";
    loginBtn.disabled = true;

    try {
        const backend = getBackend();
        if (backend) {
            const resultJson = await backend.Login(username);
            const result = JSON.parse(resultJson);
            
            if (result.success) {
                currentUser = result.user;
                welcomeText.textContent = `Xin chào, ${currentUser.FullName || currentUser.Username}`;
                
                // Pre-fill data
                const date = new Date();
                inputs.month.value = date.getMonth() + 1;
                inputs.year.value = date.getFullYear();
                calculateWorkingDays(); // Instead of hardcoding 22
                
                inputs.basicSalary.value = formatCurrencyInput(currentUser.BasicSalary || 0);
                inputs.mealAllowance.value = formatCurrencyInput(currentUser.MealAllowance || 0);
                inputs.travelAllowance.value = formatCurrencyInput(currentUser.TravelAllowance || 0);
                inputs.housingAllowance.value = formatCurrencyInput(currentUser.HousingAllowance || 0);
                inputs.otherBonus.value = formatCurrencyInput((currentUser.OtherBonus || 0) + (currentUser.Allowance || 0));
                
                inputs.alDaysOff.value = currentUser.AlDaysOff || 0;
                inputs.slDaysOff.value = currentUser.SlDaysOff || 0;
                
                inputs.overtime15x.value = currentUser.Overtime15x || 0;
                inputs.overtime2x.value = currentUser.Overtime2x || 0;
                inputs.overtime3x.value = currentUser.Overtime3x || 0;
                inputs.otDays8.value = currentUser.OtDays8 || 0;
                inputs.otDays12.value = currentUser.OtDays12 || 0;
                
                inputs.attendanceIncentive.value = formatCurrencyInput(currentUser.AttendanceIncentive || 0);
                inputs.certificateBonus.value = formatCurrencyInput(currentUser.CertificateBonus || 0);
                inputs.performanceBonus.value = formatCurrencyInput(currentUser.PerformanceBonus !== undefined ? currentUser.PerformanceBonus : 900000);
                
                inputs.insurancePercent.value = currentUser.InsurancePercent || 10.5;
                inputs.taxThreshold.value = formatCurrencyInput(currentUser.TaxThreshold || 15500000);
                
                switchScreen('main');
                loginError.classList.add('hidden');
            } else if (result.needsRegistration) {
                switchScreen('register');
                document.getElementById('regUsername').value = username;
                document.getElementById('regFullName').value = '';
                document.getElementById('regBasicSalary').value = '';
                document.getElementById('regMealAllowance').value = '';
                document.getElementById('regTravelAllowance').value = '';
                document.getElementById('regHousingAllowance').value = '';
                document.getElementById('regAttendanceIncentive').value = '';
                document.getElementById('regCertificateBonus').value = '';
                document.getElementById('regOtherBonus').value = '';
                document.getElementById('regInsurancePercent').value = '10.5';
                document.getElementById('regTaxThreshold').value = formatCurrencyInput(15500000);
                regError.classList.add('hidden');
                loginError.classList.add('hidden');
            } else {
                showError(result.message || "Đăng nhập thất bại");
            }
        }
    } catch (e) {
        showError("Lỗi kết nối: " + e.message);
    } finally {
        loginBtn.innerHTML = "Đăng Nhập";
        loginBtn.disabled = false;
    }
});

btnCancelReg.addEventListener('click', () => {
    switchScreen('login');
});

btnRegister.addEventListener('click', async () => {
    const username = document.getElementById('regUsername').value;
    const fullName = document.getElementById('regFullName').value.trim();
    const basicSalary = parseNumber(document.getElementById('regBasicSalary').value) || 0;
    
    if (!fullName || basicSalary <= 0) {
        regError.textContent = "Vui lòng nhập Họ tên và Lương cơ bản hợp lệ!";
        regError.classList.remove('hidden');
        return;
    }
    
    btnRegister.disabled = true;
    btnRegister.innerHTML = "Đang đăng ký...";
    
    try {
        const backend = getBackend();
        if (backend) {
            const payload = {
                username: username,
                fullName: fullName,
                basicSalary: basicSalary,
                mealAllowance: parseNumber(document.getElementById('regMealAllowance').value) || 0,
                travelAllowance: parseNumber(document.getElementById('regTravelAllowance').value) || 0,
                housingAllowance: parseNumber(document.getElementById('regHousingAllowance').value) || 0,
                attendanceIncentive: parseNumber(document.getElementById('regAttendanceIncentive').value) || 0,
                certificateBonus: parseNumber(document.getElementById('regCertificateBonus').value) || 0,
                otherBonus: parseNumber(document.getElementById('regOtherBonus').value) || 0,
                insurancePercent: parseNumber(document.getElementById('regInsurancePercent').value) || 0,
                taxThreshold: parseNumber(document.getElementById('regTaxThreshold').value) || 0
            };
            const resultJson = await backend.RegisterUser(JSON.stringify(payload));
            const result = JSON.parse(resultJson);
            
            if (result.success) {
                usernameInput.value = username;
                switchScreen('login');
                btnLogin.click();
            } else {
                regError.textContent = result.message || "Đăng ký thất bại";
                regError.classList.remove('hidden');
            }
        }
    } catch (e) {
        regError.textContent = "Lỗi: " + e.message;
        regError.classList.remove('hidden');
    } finally {
        btnRegister.innerHTML = "Đăng Ký & Đăng Nhập";
        btnRegister.disabled = false;
    }
});

logoutBtn.addEventListener('click', () => {
    currentUser = null;
    usernameInput.value = '';
    resGross.textContent = '0 VNĐ';
    resTax.textContent = '0 VNĐ';
    resInsurance.textContent = '0 VNĐ';
    resNet.textContent = '0 VNĐ';
    switchScreen('login');
});

welcomeText.addEventListener('click', () => {
    if (!currentUser) return;
    document.getElementById('profUsername').value = currentUser.Username || '';
    document.getElementById('profFullName').value = currentUser.FullName || '';
    document.getElementById('profBasicSalary').value = formatCurrencyInput(currentUser.BasicSalary || 0);
    document.getElementById('profMealAllowance').value = formatCurrencyInput(currentUser.MealAllowance || 0);
    document.getElementById('profTravelAllowance').value = formatCurrencyInput(currentUser.TravelAllowance || 0);
    document.getElementById('profHousingAllowance').value = formatCurrencyInput(currentUser.HousingAllowance || 0);
    document.getElementById('profAttendanceIncentive').value = formatCurrencyInput(currentUser.AttendanceIncentive || 0);
    document.getElementById('profCertificateBonus').value = formatCurrencyInput(currentUser.CertificateBonus || 0);
    document.getElementById('profOtherBonus').value = formatCurrencyInput(currentUser.OtherBonus || 0);
    document.getElementById('profInsurancePercent').value = currentUser.InsurancePercent || 10.5;
    document.getElementById('profTaxThreshold').value = formatCurrencyInput(currentUser.TaxThreshold || 15500000);
    
    profError.classList.add('hidden');
    switchScreen('profile');
});

btnCancelProfile.addEventListener('click', () => {
    switchScreen('main');
});

btnSaveProfile.addEventListener('click', async () => {
    const fullName = document.getElementById('profFullName').value.trim();
    const basicSalary = parseNumber(document.getElementById('profBasicSalary').value) || 0;
    
    if (!fullName || basicSalary <= 0) {
        profError.textContent = "Vui lòng nhập Họ tên và Lương cơ bản hợp lệ!";
        profError.classList.remove('hidden');
        return;
    }
    
    btnSaveProfile.disabled = true;
    btnSaveProfile.innerHTML = "Đang lưu...";
    
    try {
        const backend = getBackend();
        if (backend) {
            const payload = {
                username: currentUser.Username,
                fullName: fullName,
                basicSalary: basicSalary,
                mealAllowance: parseNumber(document.getElementById('profMealAllowance').value) || 0,
                travelAllowance: parseNumber(document.getElementById('profTravelAllowance').value) || 0,
                housingAllowance: parseNumber(document.getElementById('profHousingAllowance').value) || 0,
                attendanceIncentive: parseNumber(document.getElementById('profAttendanceIncentive').value) || 0,
                certificateBonus: parseNumber(document.getElementById('profCertificateBonus').value) || 0,
                otherBonus: parseNumber(document.getElementById('profOtherBonus').value) || 0,
                insurancePercent: parseNumber(document.getElementById('profInsurancePercent').value) || 0,
                taxThreshold: parseNumber(document.getElementById('profTaxThreshold').value) || 0
            };
            const resultJson = await backend.UpdateProfile(JSON.stringify(payload));
            const result = JSON.parse(resultJson);
            
            if (result.success) {
                // Update current user
                currentUser = result.user;
                welcomeText.innerHTML = `Xin chào, ${currentUser.FullName || currentUser.Username} ⚙️`;
                
                // Refresh main inputs
                inputs.basicSalary.value = formatCurrencyInput(currentUser.BasicSalary || 0);
                inputs.mealAllowance.value = formatCurrencyInput(currentUser.MealAllowance || 0);
                inputs.travelAllowance.value = formatCurrencyInput(currentUser.TravelAllowance || 0);
                inputs.housingAllowance.value = formatCurrencyInput(currentUser.HousingAllowance || 0);
                inputs.otherBonus.value = formatCurrencyInput(currentUser.Allowance || 0);
                inputs.attendanceIncentive.value = formatCurrencyInput(currentUser.AttendanceIncentive || 0);
                inputs.certificateBonus.value = formatCurrencyInput(currentUser.CertificateBonus || 0);
                inputs.insurancePercent.value = currentUser.InsurancePercent || 10.5;
                inputs.taxThreshold.value = formatCurrencyInput(currentUser.TaxThreshold || 15500000);
                
                setTimeout(() => {
                    switchScreen('main');
                }, 100);
            } else {
                profError.textContent = result.message || "Lưu thất bại";
                profError.classList.remove('hidden');
            }
        }
    } catch (e) {
        profError.textContent = "Lỗi: " + e.message;
        profError.classList.remove('hidden');
    } finally {
        setTimeout(() => {
            btnSaveProfile.innerHTML = "Lưu Thay Đổi";
            btnSaveProfile.disabled = false;
        }, 100);
    }
});

calcBtn.addEventListener('click', async () => {
    if (!currentUser) return;
    
    // Auto-sanitize all inputs: reset any empty or negative values to 0
    Object.values(inputs).forEach(input => {
        if (!input) return;
        let rawVal = input.value !== undefined && input.value !== null ? input.value.toString().trim() : "";
        let num = parseNumber(input.value);
        if (rawVal === "" || num < 0 || rawVal.includes('-')) {
            if (input.classList.contains('currency-input')) {
                input.value = "0";
            } else if (input.type === 'number') {
                input.value = 0;
            }
        }
    });

    const month = parseInt(inputs.month.value) || 0;
    if (month < 1 || month > 12) {
        alert("Vui lòng nhập tháng hợp lệ (1-12).");
        return;
    }
    
    calcBtn.innerHTML = "Đang tính...";
    calcBtn.disabled = true;
    
    try {
        const payload = {
            username: currentUser.Username,
            month: month,
            year: parseInt(inputs.year.value) || 0,
            workingDays: parseNumber(inputs.workingDays.value) || 0,
            basicSalary: parseNumber(inputs.basicSalary.value) || 0,
            mealAllowance: parseNumber(inputs.mealAllowance.value) || 0,
            travelAllowance: parseNumber(inputs.travelAllowance.value) || 0,
            housingAllowance: parseNumber(inputs.housingAllowance.value) || 0,
            allowance: 0,
            otherBonus: parseNumber(inputs.otherBonus.value) || 0,
            alDaysOff: parseNumber(inputs.alDaysOff.value) || 0,
            slDaysOff: parseNumber(inputs.slDaysOff.value) || 0,
            leaveDays: 0,
            overtime15x: parseNumber(inputs.overtime15x.value) || 0,
            overtime2x: parseNumber(inputs.overtime2x.value) || 0,
            overtime3x: parseNumber(inputs.overtime3x.value) || 0,
            otDays8: parseNumber(inputs.otDays8.value) || 0,
            otDays12: parseNumber(inputs.otDays12.value) || 0,
            otMeal8Amount: currentUser && currentUser.OtMeal8Amount ? currentUser.OtMeal8Amount : 20000,
            otMeal12Amount: currentUser && currentUser.OtMeal12Amount ? currentUser.OtMeal12Amount : 30000,
            attendanceIncentive: parseNumber(inputs.attendanceIncentive.value) || 0,
            certificateBonus: parseNumber(inputs.certificateBonus.value) || 0,
            recognizeCount: 0,
            ratingBonus: '',
            insurancePercent: parseNumber(inputs.insurancePercent.value) || 0,
            taxThreshold: parseNumber(inputs.taxThreshold.value) || 0,
            performanceBonus: parseNumber(inputs.performanceBonus.value) || 0,
            perfDeduct1: currentUser.PerfDeduct1 !== undefined ? currentUser.PerfDeduct1 : 500000,
            perfDeduct2: currentUser.PerfDeduct2 !== undefined ? currentUser.PerfDeduct2 : 700000
        };

        const backend = getBackend();
        if (backend) {
            const resultJson = await backend.CalculateSalary(JSON.stringify(payload));
            const result = JSON.parse(resultJson);
            
            if (result.success) {
                animateValue(resGross, 0, result.gross, 1000);
                animateValue(resInsurance, 0, result.insurance, 1200);
                animateValue(resTax, 0, result.tax, 1200);
                animateValue(resNet, 0, result.net, 1500);
            } else {
                alert(result.message);
            }
        }
    } catch (e) {
        alert("Lỗi tính toán: " + e.message);
    } finally {
        calcBtn.innerHTML = "TÍNH LƯƠNG";
        calcBtn.disabled = false;
    }
});

function showError(msg) {
    loginError.textContent = msg;
    loginError.classList.remove('hidden');
}

function animateValue(obj, start, end, duration) {
    let startTimestamp = null;
    const step = (timestamp) => {
        if (!startTimestamp) startTimestamp = timestamp;
        const progress = Math.min((timestamp - startTimestamp) / duration, 1);
        const easeOutQuart = 1 - Math.pow(1 - progress, 4);
        const currentVal = Math.floor(easeOutQuart * (end - start) + start);
        
        obj.innerHTML = formatCurrency(currentVal);
        
        if (progress < 1) {
            window.requestAnimationFrame(step);
        } else {
            obj.innerHTML = formatCurrency(end);
        }
    };
    window.requestAnimationFrame(step);
}

usernameInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        loginBtn.click();
    }
});

// Tab Logic
const tabBtns = document.querySelectorAll('.tab-btn');
const tabContents = document.querySelectorAll('.tab-content');

inputs.performanceBonus.addEventListener('dblclick', async () => {
    if (!currentUser) return;
    
    let currentPB = currentUser.PerformanceBonus !== undefined ? currentUser.PerformanceBonus : 900000;
    let currentD1 = currentUser.PerfDeduct1 !== undefined ? currentUser.PerfDeduct1 : 500000;
    let currentD2 = currentUser.PerfDeduct2 !== undefined ? currentUser.PerfDeduct2 : 700000;
    
    let newPBStr = prompt("Mức Thưởng Hiệu Suất mặc định (VNĐ):", currentPB);
    if (newPBStr === null) return;
    let newPB = parseNumber(newPBStr);
    
    let newD1Str = prompt("Số tiền bị trừ nếu tổng ngày nghỉ (Phép + Ốm/Không lương) > 0 và <= 1 ngày:", currentD1);
    if (newD1Str === null) return;
    let newD1 = parseNumber(newD1Str);
    
    let newD2Str = prompt("Số tiền bị trừ nếu tổng ngày nghỉ > 1 và <= 2 ngày:\n(Nếu nghỉ > 2 ngày sẽ bị trừ toàn bộ)", currentD2);
    if (newD2Str === null) return;
    let newD2 = parseNumber(newD2Str);
    
    if (!isNaN(newPB) && !isNaN(newD1) && !isNaN(newD2)) {
        try {
            const backend = getBackend();
            if (backend) {
                const payload = {
                    username: currentUser.Username,
                    fullName: currentUser.FullName || "",
                    basicSalary: currentUser.BasicSalary || 0,
                    mealAllowance: currentUser.MealAllowance || 0,
                    travelAllowance: currentUser.TravelAllowance || 0,
                    housingAllowance: currentUser.HousingAllowance || 0,
                    attendanceIncentive: currentUser.AttendanceIncentive || 0,
                    certificateBonus: currentUser.CertificateBonus || 0,
                    otherBonus: currentUser.Allowance || 0,
                    insurancePercent: currentUser.InsurancePercent || 0,
                    taxThreshold: currentUser.TaxThreshold || 0,
                    performanceBonus: newPB,
                    perfDeduct1: newD1,
                    perfDeduct2: newD2
                };
                const resultJson = await backend.UpdateProfile(JSON.stringify(payload));
                const result = JSON.parse(resultJson);
                if (result.success) {
                    currentUser = result.user;
                    inputs.performanceBonus.value = formatCurrencyInput(newPB);
                    alert("Đã cập nhật cấu hình Thưởng Hiệu Suất!");
                } else {
                    alert(result.message);
                }
            }
        } catch(e) {
            alert("Lỗi: " + e.message);
        }
    } else {
        alert("Giá trị nhập vào không hợp lệ!");
    }
});

// Double click to edit OT Meal Allowance rates (+30.000d / +20.000d)
let isOtModalOpen = false;
async function openOtMealModal(e) {
    if (e) {
        e.preventDefault();
        e.stopPropagation();
    }
    if (isOtModalOpen) return;
    isOtModalOpen = true;

    try {
        if (!currentUser) return;
        
        let current12 = currentUser.OtMeal12Amount !== undefined && currentUser.OtMeal12Amount > 0 ? currentUser.OtMeal12Amount : 30000;
        let current8 = currentUser.OtMeal8Amount !== undefined && currentUser.OtMeal8Amount > 0 ? currentUser.OtMeal8Amount : 20000;
        
        let new12Str = prompt("CẤU HÌNH MỨC TIỀN CƠM OT/NGÀY\n• Ô BÊN TRÁI (OT 8h/12h): Nhập số tiền phụ cấp 1 ngày (VNĐ):", current12);
        if (new12Str === null) return;
        let new12 = parseNumber(new12Str);
        
        let new8Str = prompt("CẤU HÌNH MỨC TIỀN CƠM OT/NGÀY\n• Ô BÊN PHẢI (OT +4h): Nhập số tiền phụ cấp 1 ngày (VNĐ):", current8);
        if (new8Str === null) return;
        let new8 = parseNumber(new8Str);

        if (new12 > 0 && new12 < 1000) new12 = new12 * 1000;
        if (new8 > 0 && new8 < 1000) new8 = new8 * 1000;
        
        if (!isNaN(new12) && !isNaN(new8)) {
            const backend = getBackend();
            if (backend) {
                const payload = {
                    username: currentUser.Username,
                    fullName: currentUser.FullName || "",
                    basicSalary: currentUser.BasicSalary || 0,
                    mealAllowance: currentUser.MealAllowance || 0,
                    travelAllowance: currentUser.TravelAllowance || 0,
                    housingAllowance: currentUser.HousingAllowance || 0,
                    attendanceIncentive: currentUser.AttendanceIncentive || 0,
                    certificateBonus: currentUser.CertificateBonus || 0,
                    otherBonus: currentUser.Allowance || 0,
                    insurancePercent: currentUser.InsurancePercent || 0,
                    taxThreshold: currentUser.TaxThreshold || 0,
                    otMeal12Amount: new12,
                    otMeal8Amount: new8
                };
                const resultJson = await backend.UpdateProfile(JSON.stringify(payload));
                const result = JSON.parse(resultJson);
                if (result.success) {
                    currentUser = result.user;
                    alert(`Đã cập nhật mức phụ cấp OT thành công!\n• OT 8h/12h (Bên trái): +${formatCurrency(new12)} VNĐ/ngày\n• OT +4h (Bên phải): +${formatCurrency(new8)} VNĐ/ngày`);
                } else {
                    alert(result.message || "Lỗi cập nhật");
                }
            }
        } else {
            alert("Giá trị nhập vào không hợp lệ!");
        }
    } catch(err) {
        alert("Lỗi: " + err.message);
    } finally {
        setTimeout(() => { isOtModalOpen = false; }, 300);
    }
}

const lblOtDays12 = document.getElementById('lblOtDays12');
const lblOtDays8 = document.getElementById('lblOtDays8');

if (lblOtDays12) lblOtDays12.addEventListener('dblclick', openOtMealModal);
if (lblOtDays8) lblOtDays8.addEventListener('dblclick', openOtMealModal);

tabBtns.forEach(btn => {
    btn.addEventListener('click', () => {
        // Remove active class from all
        tabBtns.forEach(b => b.classList.remove('active'));
        tabContents.forEach(c => c.classList.remove('active'));
        
        // Add active class to clicked tab
        btn.classList.add('active');
        const targetId = btn.getAttribute('data-tab');
        document.getElementById(targetId).classList.add('active');
        
        // Load data if necessary
        if (targetId === 'tab-history') {
            loadHistory();
        } else if (targetId === 'tab-ranking') {
            loadRanking();
        }
    });
});

async function loadHistory() {
    if (!currentUser) return;
    const backend = getBackend();
    if (!backend) return;
    
    try {
        const resultJson = await backend.GetSalaryHistory(currentUser.Username);
        const result = JSON.parse(resultJson);
        const tbody = document.getElementById('historyTableBody');
        tbody.innerHTML = '';
        
        if (result.success && result.history.length > 0) {
            // Sort history descending by year and month (latest month on top)
            result.history.sort((a, b) => {
                const parsePeriod = (p) => {
                    if (!p) return 0;
                    const matches = p.match(/(\d{1,2})[\/\-](\d{4})/);
                    if (matches) {
                        const m = parseInt(matches[1], 10);
                        const y = parseInt(matches[2], 10);
                        return y * 100 + m;
                    }
                    return 0;
                };
                return parsePeriod(b.period) - parsePeriod(a.period);
            });

            result.history.forEach(item => {
                const tr = document.createElement('tr');
                tr.style.cursor = 'pointer';
                tr.innerHTML = `
                    <td class="history-cell-click" title="Click để xem chi tiết tính lương">
                        <span style="display: inline-flex; align-items: center; gap: 6px;">
                            ${item.period} 
                            <span style="font-size: 0.8rem; opacity: 0.7; color: var(--primary);">🔍</span>
                        </span>
                    </td>
                    <td class="glow-text-green history-cell-click" title="Click để xem chi tiết tính lương">${formatCurrency(item.netSalary)}</td>
                    <td style="text-align: center;">
                        <button class="btn-3d btn-ghost btn-delete-history" style="padding: 4px 12px; font-size: 0.78rem; color: #ef4444; border-color: rgba(239, 68, 68, 0.3);">Xóa 🗑️</button>
                    </td>
                `;

                // Add row click handler to open detail modal
                tr.querySelectorAll('.history-cell-click').forEach(cell => {
                    cell.addEventListener('click', () => showHistoryDetail(item));
                });

                // Add delete button handler
                const btnDelete = tr.querySelector('.btn-delete-history');
                btnDelete.addEventListener('click', (e) => {
                    e.stopPropagation();
                    if (confirm(`Bạn có chắc chắn muốn xóa lịch sử lương kỳ ${item.period}?`)) {
                        deleteHistory(item.period);
                    }
                });

                tbody.appendChild(tr);
            });
        } else {
            tbody.innerHTML = '<tr><td colspan="3" style="text-align:center">Chưa có dữ liệu lịch sử</td></tr>';
        }
    } catch (e) {
        console.error("Lỗi tải lịch sử: ", e);
    }
}

function showHistoryDetail(item) {
    const modal = document.getElementById('historyDetailModal');
    const title = document.getElementById('modalPeriodTitle');
    const content = document.getElementById('modalDetailContent');
    const btnClose = document.getElementById('btnCloseModal');
    
    if (!modal || !title || !content) return;
    
    title.textContent = `Chi Tiết Tính Lương - ${item.period}`;
    
    if (!item.detail) {
        content.innerHTML = `
            <div style="text-align: center; padding: 20px 10px;">
                <p style="color: var(--text-muted); margin-bottom: 10px;">Dữ liệu tính lương kỳ này chỉ lưu tổng lương thực nhận.</p>
                <div style="font-size: 1.3rem; font-weight: bold;">Tổng Thực Nhận (NET): <span class="glow-text-green">${formatCurrency(item.netSalary)}</span></div>
            </div>
        `;
    } else {
        try {
            const d = typeof item.detail === 'string' ? JSON.parse(item.detail) : item.detail;
            const slDeductionVal = d.slDeduction !== undefined ? d.slDeduction : (d.slDaysOff > 0 ? (d.slDaysOff * ((d.basicSalary + d.mealAllowance) / (d.workingDays || 22))) : 0);

            let bonusMealVal = 0;
            if (d.bonusMeal !== undefined) {
                bonusMealVal = d.bonusMeal;
            } else {
                let m12 = d.otMeal12Amount || 30000;
                let m8 = d.otMeal8Amount || 20000;
                if (d.otDays12 > 0) bonusMealVal += d.otDays12 * m12;
                if (d.otDays8 > 0) bonusMealVal += d.otDays8 * m8;
            }

            let mealDisplayHtml = formatCurrency(d.mealAllowance || 0);
            if (bonusMealVal > 0) {
                mealDisplayHtml += `<span style="color: #10b981; font-size: 0.72rem; font-weight: 700; margin-left: 3px; background: rgba(16,185,129,0.12); padding: 1px 4px; border-radius: 4px;" title="Cộng thêm từ ngày OT">+${formatCurrency(bonusMealVal)}</span>`;
            }

            // Calculate OT monetary earnings
            const hourlyRate = (d.basicSalary || 0) / (d.workingDays || 22) / 8;
            const ot15Val = d.overtime15xSalary !== undefined ? d.overtime15xSalary : Math.round((d.overtime15x || 0) * hourlyRate * 1.5);
            const ot20Val = d.overtime2xSalary !== undefined ? d.overtime2xSalary : Math.round((d.overtime2x || 0) * hourlyRate * 2.0);
            const ot30Val = d.overtime3xSalary !== undefined ? d.overtime3xSalary : Math.round((d.overtime3x || 0) * hourlyRate * 3.0);

            let ot15Html = (d.overtime15x || 0) + 'h';
            if (ot15Val > 0) {
                ot15Html += `<span style="color: #10b981; font-size: 0.72rem; font-weight: 700; margin-left: 3px; background: rgba(16,185,129,0.12); padding: 1px 4px; border-radius: 4px;" title="Thành tiền OT 1.5x">+${formatCurrency(ot15Val)}</span>`;
            }

            let ot20Html = (d.overtime2x || 0) + 'h';
            if (ot20Val > 0) {
                ot20Html += `<span style="color: #10b981; font-size: 0.72rem; font-weight: 700; margin-left: 3px; background: rgba(16,185,129,0.12); padding: 1px 4px; border-radius: 4px;" title="Thành tiền OT 2.0x">+${formatCurrency(ot20Val)}</span>`;
            }

            let ot30Html = (d.overtime3x || 0) + 'h';
            if (ot30Val > 0) {
                ot30Html += `<span style="color: #10b981; font-size: 0.72rem; font-weight: 700; margin-left: 3px; background: rgba(16,185,129,0.12); padding: 1px 4px; border-radius: 4px;" title="Thành tiền OT 3.0x">+${formatCurrency(ot30Val)}</span>`;
            }

            const makeItem = (label, value, isHighlight = false, color = 'var(--text-main)') => `
                <div style="display: flex; justify-content: space-between; align-items: center; background: rgba(255,255,255,0.03); padding: 5px 10px; border-radius: 6px; border: 1px solid rgba(255,255,255,0.04);">
                    <span style="color:var(--text-muted); font-size: 0.8rem;">${label}</span>
                    <strong style="color: ${color}; font-size: 0.85rem; font-weight: ${isHighlight ? '700' : '600'}; display: flex; align-items: center;">${value}</strong>
                </div>
            `;

            let slDaysHtml = (d.slDaysOff || 0) + ' ngày';
            if (slDeductionVal > 0) {
                slDaysHtml += `<span style="color: #ef4444; font-size: 0.72rem; font-weight: 700; margin-left: 3px; background: rgba(239,68,68,0.12); padding: 1px 4px; border-radius: 4px;" title="Khấu trừ lương SL/NP">-${formatCurrency(slDeductionVal)}</span>`;
            }

            const dailyBasicSalary = Math.round((d.basicSalary || 0) / (d.workingDays || 22));

            content.innerHTML = `
                <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 6px; background: rgba(0,0,0,0.2); padding: 10px; border-radius: 12px; border: 1px solid rgba(255,255,255,0.08);">
                    ${makeItem('Lương Cơ Bản', formatCurrency(d.basicSalary || 0))}
                    ${makeItem('Ngày công chuẩn', (d.workingDays || 0) + ' ngày')}
                    ${makeItem('Lương CB 1 ngày', formatCurrency(dailyBasicSalary))}
                    ${makeItem('Tiền Cơm', mealDisplayHtml)}
                    ${makeItem('Trợ cấp Đi lại', formatCurrency(d.travelAllowance || 0))}
                    ${makeItem('Trợ cấp Nhà ở', formatCurrency(d.housingAllowance || 0))}
                    ${makeItem('Chuyên cần', formatCurrency(d.attendanceIncentive || 0))}
                    ${makeItem('Tiền Cert', formatCurrency(d.certificateBonus || 0))}
                    ${makeItem('Thưởng Hiệu suất', formatCurrency(d.performanceBonus || 0))}
                    ${makeItem('Thưởng & PC khác', formatCurrency(d.otherBonus || 0))}
                    ${makeItem('Nghỉ Phép (AL)', (d.alDaysOff || 0) + ' ngày')}
                    ${makeItem('Nghỉ SL/NP', slDaysHtml)}
                    ${makeItem('Tăng ca 1.5x', ot15Html)}
                    ${makeItem('Tăng ca 2.0x', ot20Html)}
                    ${makeItem('Tăng ca 3.0x', ot30Html)}
                </div>

                <div style="display: grid; grid-template-columns: 1fr 1fr 1fr 1fr; gap: 8px; margin-top: 10px; background: rgba(0,0,0,0.3); padding: 10px 14px; border-radius: 12px; border: 1px solid rgba(255,255,255,0.1); text-align: center;">
                    <div>
                        <div style="color:var(--text-muted); font-size: 0.76rem; margin-bottom: 2px;">Gross Thu Nhập</div>
                        <strong style="color: #60a5fa; font-size: 0.95rem;">${formatCurrency(d.gross || 0)}</strong>
                    </div>
                    <div>
                        <div style="color:var(--text-muted); font-size: 0.76rem; margin-bottom: 2px;">Bảo Hiểm (BHXH)</div>
                        <strong style="color: #f87171; font-size: 0.95rem;">-${formatCurrency(d.insurance || 0)}</strong>
                    </div>
                    <div>
                        <div style="color:var(--text-muted); font-size: 0.76rem; margin-bottom: 2px;">Thuế TNCN</div>
                        <strong style="color: #f87171; font-size: 0.95rem;">-${formatCurrency(d.tax || 0)}</strong>
                    </div>
                    <div style="border-left: 1px dashed rgba(255,255,255,0.2); padding-left: 8px;">
                        <div style="color:var(--text-muted); font-size: 0.76rem; margin-bottom: 2px;">Thực Nhận (NET)</div>
                        <strong class="glow-text-green" style="font-size: 1.1rem;">${formatCurrency(d.net || item.netSalary)}</strong>
                    </div>
                </div>
            `;
        } catch (e) {
            content.innerHTML = `<p style="color: var(--text-muted);">Dữ liệu chi tiết kỳ này không thể định dạng.</p><p>Tổng nhận: <strong class="glow-text-green">${formatCurrency(item.netSalary)}</strong></p>`;
        }
    }
    
    modal.style.display = 'flex';
    
    btnClose.onclick = () => {
        modal.style.display = 'none';
    };
    
    modal.onclick = (e) => {
        if (e.target === modal) modal.style.display = 'none';
    };
}

async function deleteHistory(period) {
    if (!currentUser) return;
    const backend = getBackend();
    if (!backend) return;
    
    try {
        const resultJson = await backend.DeleteSalaryHistoryEntry(currentUser.Username, period);
        const result = JSON.parse(resultJson);
        if (result.success) {
            loadHistory();
        } else {
            alert("Lỗi khi xóa: " + (result.message || "Không xác định"));
        }
    } catch (e) {
        console.error("Lỗi xóa lịch sử: ", e);
    }
}

async function loadRanking() {
    if (!currentUser) return;
    const backend = getBackend();
    if (!backend) return;
    
    try {
        const month = parseInt(inputs.month.value) || new Date().getMonth() + 1;
        const year = parseInt(inputs.year.value) || new Date().getFullYear();
        
        const resultJson = await backend.GetRanking(month, year);
        const result = JSON.parse(resultJson);
        const tbody = document.getElementById('rankingTableBody');
        tbody.innerHTML = '';
        
        if (result.success && result.ranking.length > 0) {
            result.ranking.forEach(item => {
                const isTop3 = item.rank <= 3;
                const tr = document.createElement('tr');
                if (isTop3) tr.style.background = 'rgba(99, 102, 241, 0.15)';
                tr.innerHTML = `
                    <td style="font-weight:bold; color: ${isTop3 ? 'var(--primary)' : 'inherit'}">${item.rank}</td>
                    <td>${item.name}</td>
                    <td class="glow-text-green">${formatCurrency(item.netSalary)}</td>
                    <td style="color: var(--text-muted)">${item.comment}</td>
                `;
                tbody.appendChild(tr);
            });
        } else {
            tbody.innerHTML = '<tr><td colspan="4" style="text-align:center">Chưa có dữ liệu xếp hạng tháng này</td></tr>';
        }
    } catch (e) {
        console.error("Lỗi tải xếp hạng: ", e);
    }
}

function initCompanyLogoModals() {
    const companyModal = document.getElementById('companyInfoModal');
    const companyLogoIcon = document.getElementById('companyLogoIcon');
    const companyModalName = document.getElementById('companyModalName');
    const companyModalDesc = document.getElementById('companyModalDesc');
    const btnCloseCompanyModal = document.getElementById('btnCloseCompanyModal');

    const logoManpower = document.querySelector('.logo-left');
    const logoIntel = document.querySelector('.logo-right');

    if (logoManpower && companyModal) {
        logoManpower.addEventListener('click', () => {
            companyModalName.textContent = 'ManpowerGroup Vietnam';
            companyModalDesc.innerHTML = `
                <div style="font-size:0.88rem; line-height:1.55; color:#cbd5e1;">
                    <p style="margin:0 0 10px 0;"><strong>ManpowerGroup</strong> là tập đoàn giải pháp nhân sự toàn cầu hàng đầu thế giới với hơn 75 năm kinh nghiệm hoạt động tại 75+ quốc gia.</p>
                    
                    <div style="background:rgba(0,0,0,0.25); padding:10px 12px; border-radius:10px; border:1px solid rgba(255,255,255,0.08); margin-bottom:10px;">
                        <strong style="color:var(--primary); display:block; margin-bottom:6px; font-size:0.9rem;">🤝 Các Đối Tác Chiến Lược Hợp Tác Tiêu Biểu:</strong>
                        <div style="display:grid; grid-template-columns:1fr 1fr; gap:6px; font-size:0.83rem;">
                            <div>• 🌐 <strong>Intel Products VN</strong></div>
                            <div>• 📱 <strong>Samsung Electronics</strong></div>
                            <div>• 🥤 <strong>Coca-Cola Beverages</strong></div>
                            <div>• 🧴 <strong>Unilever Vietnam</strong></div>
                            <div>• 💻 <strong>Microsoft Vietnam</strong></div>
                            <div>• 📺 <strong>LG Electronics</strong></div>
                            <div>• 🛒 <strong>Shopee & Lazada</strong></div>
                            <div>• 🏦 <strong>Standard Chartered</strong></div>
                        </div>
                    </div>

                    <div style="font-size:0.82rem; color:var(--text-muted);">
                        ✨ <em>Chuyên cung cấp dịch vụ quản trị nhân sự, giải pháp khoán dịch vụ lao động (BPO) & tư vấn nhân tài chiến lược cho các tập đoàn đa quốc gia.</em>
                    </div>
                </div>
            `;
            companyLogoIcon.innerHTML = `
                <svg width="42" height="42" viewBox="0 0 100 100" fill="none">
                    <path d="M15 70 L15 30 C15 30, 25 20, 38 35 C50 50, 60 20, 72 35 C80 45, 85 55, 85 70" stroke="url(#mpGrad1_m)" stroke-width="12" stroke-linecap="round" stroke-linejoin="round"/>
                    <path d="M25 75 C35 65, 45 75, 55 60 C65 45, 75 60, 85 55" stroke="url(#mpGrad2_m)" stroke-width="8" stroke-linecap="round"/>
                    <defs>
                        <linearGradient id="mpGrad1_m" x1="0%" y1="0%" x2="100%" y2="100%">
                            <stop offset="0%" stop-color="#38bdf8"/>
                            <stop offset="100%" stop-color="#6366f1"/>
                        </linearGradient>
                        <linearGradient id="mpGrad2_m" x1="0%" y1="0%" x2="100%" y2="100%">
                            <stop offset="0%" stop-color="#f43f5e"/>
                            <stop offset="100%" stop-color="#fb923c"/>
                        </linearGradient>
                    </defs>
                </svg>
            `;
            companyModal.classList.remove('hidden');
        });
    }

    if (logoIntel && companyModal) {
        logoIntel.addEventListener('click', () => {
            companyModalName.textContent = 'Intel Corporation';
            companyModalDesc.innerHTML = `
                <div style="font-size:0.88rem; line-height:1.55; color:#cbd5e1;">
                    <p style="margin:0 0 10px 0;"><strong>Intel Corporation</strong> là tập đoàn công nghệ vi xử lý & bán dẫn hàng đầu thế giới, tiên phong dẫn dắt kỷ nguyên AI PC và điện toán đám mây.</p>

                    <div style="background:rgba(0,0,0,0.25); padding:10px 12px; border-radius:10px; border:1px solid rgba(255,255,255,0.08); margin-bottom:10px;">
                        <strong style="color:#38bdf8; display:block; margin-bottom:4px; font-size:0.88rem;">🔥 Sản Phẩm Hot Nổi Bật Hiện Nay:</strong>
                        <ul style="margin:0; padding-left:18px; font-size:0.83rem;">
                            <li style="margin-bottom:3px;"><strong>Core Ultra 200V (Lunar Lake):</strong> Vi xử lý AI PC tối ưu năng lượng đỉnh cao.</li>
                            <li style="margin-bottom:3px;"><strong>Xeon 6 (Granite Rapids):</strong> Chip máy chủ Data Center siêu hiệu năng.</li>
                            <li><strong>Gaudi 3 AI Accelerator:</strong> Chip tăng tốc huấn luyện AI thế hệ mới.</li>
                        </ul>
                    </div>

                    <div style="background:rgba(0,0,0,0.25); padding:10px 12px; border-radius:10px; border:1px solid rgba(255,255,255,0.08);">
                        <strong style="color:#6366f1; display:block; margin-bottom:4px; font-size:0.88rem;">🚀 Công Nghệ Tương Lai Roadmap:</strong>
                        <ul style="margin:0; padding-left:18px; font-size:0.83rem;">
                            <li style="margin-bottom:3px;"><strong>Tiến trình Intel 18A (1.8nm):</strong> Tiến trình sản xuất chip 2025 đột phá với RibbonFET & PowerVia.</li>
                            <li style="margin-bottom:3px;"><strong>Panther Lake & Nova Lake:</strong> Kiến trúc vi xử lý AI PC tương lai.</li>
                            <li><strong>Glass Substrates & Quantum:</strong> Đế chip bằng thủy tinh & Điện toán lượng tử.</li>
                        </ul>
                    </div>
                </div>
            `;
            companyLogoIcon.innerHTML = `
                <svg width="45" height="45" viewBox="0 0 120 120" fill="none">
                    <ellipse cx="60" cy="60" rx="50" ry="32" stroke="url(#intelGrad_m)" stroke-width="7" transform="rotate(-15 60 60)" stroke-dasharray="240" stroke-dashoffset="20"/>
                    <text x="60" y="68" font-family="'Inter', sans-serif" font-weight="900" font-size="28" fill="#38bdf8" text-anchor="middle" letter-spacing="-1">intel</text>
                    <circle cx="95" cy="38" r="4" fill="#38bdf8"/>
                    <defs>
                        <linearGradient id="intelGrad_m" x1="0%" y1="0%" x2="100%" y2="100%">
                            <stop offset="0%" stop-color="#0068b5"/>
                            <stop offset="100%" stop-color="#38bdf8"/>
                        </linearGradient>
                    </defs>
                </svg>
            `;
            companyModal.classList.remove('hidden');
        });
    }

    if (btnCloseCompanyModal && companyModal) {
        btnCloseCompanyModal.onclick = () => companyModal.classList.add('hidden');
        companyModal.onclick = (e) => {
            if (e.target === companyModal) companyModal.classList.add('hidden');
        };
    }
}

initCurrencyInputs();
initNonNegativeInputs();
initCompanyLogoModals();

