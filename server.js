const http = require('http');
const fs = require('fs');
const path = require('path');
const os = require('os');

const PORT = process.env.PORT || 5050;
const WWWROOT = path.join(__dirname, 'wwwroot');
const ROOT_DIR = __dirname;

const MIME_TYPES = {
    '.html': 'text/html; charset=utf-8',
    '.css': 'text/css; charset=utf-8',
    '.js': 'application/javascript; charset=utf-8',
    '.json': 'application/json; charset=utf-8',
    '.png': 'image/png',
    '.jpg': 'image/jpeg',
    '.jpeg': 'image/jpeg',
    '.gif': 'image/gif',
    '.svg': 'image/svg+xml',
    '.ico': 'image/x-icon',
    '.webp': 'image/webp',
    '.apk': 'application/vnd.android.package-archive',
    '.woff2': 'font/woff2',
    '.woff': 'font/woff',
    '.ttf': 'font/ttf'
};

function getLocalIpAddresses() {
    const interfaces = os.networkInterfaces();
    const ips = [];
    for (const name of Object.keys(interfaces)) {
        for (const net of interfaces[name] || []) {
            if (net.family === 'IPv4' && !net.internal) {
                ips.push(net.address);
            }
        }
    }
    return ips;
}

const server = http.createServer((req, res) => {
    // Add permissive CORS
    res.setHeader('Access-Control-Allow-Origin', '*');
    res.setHeader('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
    res.setHeader('Access-Control-Allow-Headers', '*');

    if (req.method === 'OPTIONS') {
        res.writeHead(204);
        res.end();
        return;
    }

    let parsedUrl = req.url.split('?')[0];
    if (parsedUrl === '/' || parsedUrl === '') {
        parsedUrl = '/simulator.html';
    }

    // Try finding file in wwwroot first, then in project root (for .apk, etc.)
    let filePath = path.join(WWWROOT, parsedUrl);
    if (!fs.existsSync(filePath)) {
        filePath = path.join(ROOT_DIR, parsedUrl);
    }

    if (!fs.existsSync(filePath)) {
        res.writeHead(404, { 'Content-Type': 'text/html; charset=utf-8' });
        res.end(`<h2>404 Not Found: ${parsedUrl}</h2><p><a href="/simulator.html">Quay về Trình giả lập điện thoại</a></p>`);
        return;
    }

    const stat = fs.statSync(filePath);
    if (stat.isDirectory()) {
        filePath = path.join(filePath, 'index.html');
        if (!fs.existsSync(filePath)) {
            res.writeHead(403);
            res.end('Directory listing forbidden');
            return;
        }
    }

    const ext = path.extname(filePath).toLowerCase();
    const contentType = MIME_TYPES[ext] || 'application/octet-stream';

    res.writeHead(200, {
        'Content-Type': contentType,
        'Content-Length': fs.statSync(filePath).size,
        'Cache-Control': 'no-cache'
    });

    const stream = fs.createReadStream(filePath);
    stream.pipe(res);
});

server.listen(PORT, '0.0.0.0', () => {
    const ips = getLocalIpAddresses();
    console.log('\n======================================================');
    console.log('📱 SALARY CALCULATOR - LOCAL MOBILE SIMULATOR SERVER');
    console.log('======================================================');
    console.log(`\n  🖥️  Giả Lập Điện Thoại (PC): http://localhost:${PORT}/simulator.html`);
    console.log(`  🌐  Bản Web Trực Tiếp (F12): http://localhost:${PORT}/index.html`);
    
    if (ips.length > 0) {
        console.log(`\n  📲  Mở trên Điện Thoại Thật (Cùng Wifi):`);
        ips.forEach(ip => {
            console.log(`      ➜ http://${ip}:${PORT}/index.html`);
        });
    }
    console.log('\n======================================================\n');
});
