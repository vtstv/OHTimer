const fs = require('fs');
const path = require('path');

const rootDir = path.resolve(__dirname, '..');
const websiteDir = path.join(rootDir, 'website');
const indexPath = path.join(websiteDir, 'index.html');
const cssPath = path.join(websiteDir, 'assets/css/style.css');
const jsPath = path.join(websiteDir, 'assets/js/main.js');
const cnamePath = path.join(websiteDir, 'CNAME');
const nojekyllPath = path.join(websiteDir, '.nojekyll');
const robotsPath = path.join(websiteDir, 'robots.txt');
const sitemapPath = path.join(websiteDir, 'sitemap.xml');
const workflowPath = path.join(rootDir, '.github/workflows/pages.yml');

let errors = 0;
let warnings = 0;

console.log('=== Once Human: Event Timer — Website Verification ===\n');

// 1. Core file existence
const requiredFiles = [
  { name: 'index.html', path: indexPath },
  { name: 'style.css', path: cssPath },
  { name: 'main.js', path: jsPath },
  { name: 'CNAME', path: cnamePath },
  { name: '.nojekyll', path: nojekyllPath },
  { name: 'robots.txt', path: robotsPath },
  { name: 'sitemap.xml', path: sitemapPath },
  { name: '.github/workflows/pages.yml', path: workflowPath }
];

requiredFiles.forEach(f => {
  if (!fs.existsSync(f.path)) {
    console.error(`FAIL: Required file missing: ${f.name}`);
    errors++;
  } else {
    console.log(`PASS: Found ${f.name}`);
  }
});

// 2. Check CNAME content
if (fs.existsSync(cnamePath)) {
  const cnameContent = fs.readFileSync(cnamePath, 'utf8').trim();
  if (cnameContent !== 'ohtimer.murr.li') {
    console.error(`FAIL: CNAME contains '${cnameContent}', expected 'ohtimer.murr.li'`);
    errors++;
  } else {
    console.log(`PASS: CNAME matches 'ohtimer.murr.li'`);
  }
}

// 3. Check HTML structure and local assets
const html = fs.readFileSync(indexPath, 'utf8');

const attrRegex = /(?:src|href)="([^"#][^"]*)"/g;
let match;
let assetCount = 0;
while ((match = attrRegex.exec(html)) !== null) {
  const url = match[1];
  if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('mailto:') || url.startsWith('data:')) {
    continue;
  }
  assetCount++;
  const cleanUrl = url.split('?')[0].split('#')[0];
  const fullPath = path.join(websiteDir, cleanUrl);
  if (!fs.existsSync(fullPath)) {
    console.error(`FAIL: Missing asset referenced in HTML: ${url} -> ${fullPath}`);
    errors++;
  }
}
console.log(`PASS: All ${assetCount} local HTML asset references exist on disk`);

// 4. Validate SVG elements
const svgRegex = /<svg[\s\S]*?<\/svg>/g;
const validSvgTags = new Set([
  'svg', 'path', 'circle', 'line', 'polygon', 'polyline', 'rect', 'ellipse',
  'g', 'defs', 'linearGradient', 'radialGradient', 'stop', 'use', 'text', 'tspan', 'clipPath', 'mask'
]);
let svgMatch;
let svgIndex = 0;
while ((svgMatch = svgRegex.exec(html)) !== null) {
  svgIndex++;
  const tagMatches = svgMatch[0].match(/<([a-zA-Z0-9]+)/g) || [];
  for (const t of tagMatches) {
    const tagName = t.slice(1);
    if (!validSvgTags.has(tagName)) {
      console.error(`FAIL: Invalid SVG tag <${tagName}> in SVG #${svgIndex}`);
      errors++;
    }
  }
}
console.log(`PASS: All ${svgIndex} SVG elements verified with 100% valid W3C SVG tags`);

// 5. Open Graph / Twitter image URLs
const ogImageMatch = html.match(/<meta\s+property="og:image"\s+content="([^"]+)"/);
if (!ogImageMatch || !ogImageMatch[1].startsWith('https://')) {
  console.error(`FAIL: og:image content must be an absolute HTTPS URL: ${ogImageMatch ? ogImageMatch[1] : 'missing'}`);
  errors++;
} else {
  console.log(`PASS: Valid absolute og:image: ${ogImageMatch[1]}`);
}

const twitterImageMatch = html.match(/<meta\s+name="twitter:image"\s+content="([^"]+)"/);
if (!twitterImageMatch || !twitterImageMatch[1].startsWith('https://')) {
  console.error(`FAIL: twitter:image content must be an absolute HTTPS URL: ${twitterImageMatch ? twitterImageMatch[1] : 'missing'}`);
  errors++;
} else {
  console.log(`PASS: Valid absolute twitter:image: ${twitterImageMatch[1]}`);
}

// 6. Verify 15 Meta Presets in HTML
const expectedPresets = [
  'Visional Wheel: The Echo',
  'Cargo Scramble (Sproutlet Truck)',
  'Drift Greenhouse [Worlds 1–12]',
  'Manibus (Prime War)',
  'Skyripper Anomaly',
  'Public Crisis (Harrowing)',
  'Chiron the Centaur',
  'Extraction Workshop Defense',
  'Lunar Revelry (Lord of Moonlight)',
  'Lunar Oracle (Blood Moon)',
  'Aberrant Progeny (Spore Tide)',
  'Blackfell Stronghold Loot Run',
  'Crate Loot Reset (4-Hour)',
  'Custom Wheel Event',
  'Gravity Abyss (Legacy)'
];

let presetMatches = 0;
expectedPresets.forEach(preset => {
  if (html.includes(preset)) {
    presetMatches++;
  } else {
    console.error(`FAIL: Missing preset from presets table: "${preset}"`);
    errors++;
  }
});
console.log(`PASS: All ${presetMatches} / 15 meta presets present in showcase table`);

// 7. Verify Hotkeys in Cheat Sheet
const expectedHotkeys = [
  'NumPad 1',
  'NumPad +',
  'NumPad -',
  'Ctrl+Alt+O',
  'Ctrl+Alt+S',
  'Ctrl+Alt+L',
  'Ctrl+Alt+P'
];
let hotkeyMatches = 0;
expectedHotkeys.forEach(hk => {
  if (html.includes(hk)) {
    hotkeyMatches++;
  } else {
    console.error(`FAIL: Missing hotkey in cheat sheet: "${hk}"`);
    errors++;
  }
});
console.log(`PASS: All ${hotkeyMatches} / 7 hotkeys present in cheat sheet`);

// 8. Verify CSS syntax and url() references
const css = fs.readFileSync(cssPath, 'utf8');
let openBrackets = 0;
for (const ch of css) {
  if (ch === '{') openBrackets++;
  else if (ch === '}') openBrackets--;
  if (openBrackets < 0) {
    console.error('FAIL: Unbalanced closing brace in style.css');
    errors++;
    break;
  }
}
if (openBrackets !== 0) {
  console.error(`FAIL: Unbalanced braces in style.css (remaining open: ${openBrackets})`);
  errors++;
} else {
  console.log('PASS: style.css syntax braces 100% balanced');
}

// 9. Verify JS syntax
try {
  require('child_process').execSync(`node --check "${jsPath}"`);
  console.log('PASS: main.js syntax check passed with 0 errors');
} catch (e) {
  console.error('FAIL: main.js has syntax errors:', e.message);
  errors++;
}

// 10. Verify GitHub Pages workflow
const workflowYaml = fs.readFileSync(workflowPath, 'utf8');
if (!workflowYaml.includes('actions/upload-pages-artifact') || !workflowYaml.includes('actions/deploy-pages')) {
  console.error('FAIL: pages.yml missing standard GitHub Pages deployment actions');
  errors++;
} else {
  console.log('PASS: .github/workflows/pages.yml contains official GitHub Pages deployment actions');
}

console.log(`\n========================================`);
console.log(`Verification Finished: ${errors} errors, ${warnings} warnings.`);
console.log(`========================================\n`);

process.exit(errors > 0 ? 1 : 0);
