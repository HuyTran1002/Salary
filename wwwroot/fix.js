const fs = require('fs');
let content = fs.readFileSync('index.html', 'utf8');
const ids = ['regBasicSalary','regMealAllowance','regTravelAllowance','regHousingAllowance','regAttendanceIncentive','regCertificateBonus','regOtherBonus','regTaxThreshold','profBasicSalary','profMealAllowance','profTravelAllowance','profHousingAllowance','profAttendanceIncentive','profCertificateBonus','profOtherBonus','profTaxThreshold','basicSalary','mealAllowance','travelAllowance','housingAllowance','attendanceIncentive','certificateBonus','otherBonus','performanceBonus','taxThreshold'];
ids.forEach(id => {
    let regex = new RegExp('type="number" id="' + id + '"', 'g');
    content = content.replace(regex, 'type="text" inputmode="numeric" class="currency-input" id="' + id + '"');
});
fs.writeFileSync('index.html', content);
console.log('done');
