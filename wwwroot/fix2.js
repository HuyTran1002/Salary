const fs = require('fs');
let content = fs.readFileSync('index.html', 'utf8');
const ids = ['regInsurancePercent', 'profInsurancePercent', 'insurancePercent'];
ids.forEach(id => {
    let regex = new RegExp('type="text" inputmode="numeric" class="currency-input" id="' + id + '"', 'g');
    content = content.replace(regex, 'type="number" step="any" id="' + id + '"');
});
fs.writeFileSync('index.html', content);
console.log('done');
