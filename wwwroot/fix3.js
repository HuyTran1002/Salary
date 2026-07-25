const fs = require('fs');

// Fix CSS - rename .3d-btn to .btn-3d
let css = fs.readFileSync('style.css', 'utf8');
css = css.replace(/\.3d-btn/g, '.btn-3d');
fs.writeFileSync('style.css', css);

// Fix HTML - rename all class references from 3d-btn to btn-3d
let html = fs.readFileSync('index.html', 'utf8');
html = html.replace(/\b3d-btn\b/g, 'btn-3d');
fs.writeFileSync('index.html', html);

let cssCnt = (css.match(/\.btn-3d/g) || []).length;
let htmlCnt = (html.match(/btn-3d/g) || []).length;
console.log('CSS .btn-3d:', cssCnt);
console.log('HTML btn-3d:', htmlCnt);
