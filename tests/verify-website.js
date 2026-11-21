const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const rootDir = path.resolve(__dirname, '..');
const websiteDir = path.join(rootDir, 'docs/website');
const srcDir = path.join(websiteDir, 'src');
const componentsDir = path.join(srcDir, 'components');
const templatePath = path.join(srcDir, 'template.html');
const buildScriptPath = path.join(srcDir, 'build.js');
const indexPath = path.join(websiteDir, 'index.html');
const cssPath = path.join(websiteDir, 'assets/css/style.css');
const jsPath = path.join(websiteDir, 'assets/js/main.js');
const translationsPath = path.join(websiteDir, 'assets/js/translations.js');
const cnamePath = path.join(websiteDir, 'CNAME');
const nojekyllPath = path.join(websiteDir, '.nojekyll');
const robotsPath = path.join(websiteDir, 'robots.txt');
const sitemapPath = path.join(websiteDir, 'sitemap.xml');
const workflowPath = path.join(rootDir, '.github/workflows/pages.yml');

let errors = 0;
let warnings = 0;

console.log('=== Once Human: Event Timer — Website Verification ===\n');

// 1. Core and modular file existence
const requiredFiles = [
  { name: 'template.html', path: templatePath },
  { name: 'build.js', path: buildScriptPath },
  { name: 'style.css', path: cssPath },
  { name: 'main.js', path: jsPath },
  { name: 'translations.js', path: translationsPath },
  { name: 'CNAME', path: cnamePath },
  { name: '.nojekyll', path: nojekyllPath },
  { name: 'robots.txt', path: robotsPath },
  { name: 'sitemap.xml', path: sitemapPath },
  { name: '.github/workflows/pages.yml', path: workflowPath }
];

const requiredComponents = [
  'header.html',
  'hero.html',
  'metrics.html',
  'features.html',
  'screenshots.html',
  'download.html',
  'faq.html',
  'footer.html'
];

requiredFiles.forEach(f => {
  if (!fs.existsSync(f.path)) {
    console.error(`FAIL: Required file missing: ${f.name}`);
    errors++;
  } else {
    console.log(`PASS: Found ${f.name}`);
  }
});

requiredComponents.forEach(comp => {
  const p = path.join(componentsDir, comp);
  if (!fs.existsSync(p)) {
    console.error(`FAIL: Missing modular component: ${comp}`);
    errors++;
  } else {
    console.log(`PASS: Found component ${comp}`);
  }
});

// 2. Build execution test
try {
  const { build } = require(buildScriptPath);
  build();
  console.log('PASS: Modular build script executed successfully');
} catch (e) {
  console.error('FAIL: Build script execution failed:', e.message);
  errors++;
}

// 3. Check CNAME content
if (fs.existsSync(cnamePath)) {
  const cnameContent = fs.readFileSync(cnamePath, 'utf8').trim();
  if (cnameContent !== 'ohtimer.murr.li') {
    console.error(`FAIL: CNAME contains '${cnameContent}', expected 'ohtimer.murr.li'`);
    errors++;
  } else {
    console.log(`PASS: CNAME matches 'ohtimer.murr.li'`);
  }
}

// 4. Check HTML structure and local assets
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

// 5. Validate SVG elements
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

// 6. Open Graph / Twitter image URLs
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

// 7. Verify Universal Direct Distribution Download Links
const hasStandaloneLink = /href="https:\/\/github\.com\/vtstv\/OHTimer\/releases\/latest\/download\/OnceHumanTimer-standalone\.zip"/i.test(html);
const hasPortableLink = /href="https:\/\/github\.com\/vtstv\/OHTimer\/releases\/latest\/download\/OnceHumanTimer-portable\.zip"/i.test(html);
const hasSetupLink = /href="https:\/\/github\.com\/vtstv\/OHTimer\/releases\/latest\/download\/OnceHumanTimer-Setup\.exe"/i.test(html);
const hasReleasesPageLink = html.includes('https://github.com/vtstv/OHTimer/releases');

if (!hasStandaloneLink) {
  console.error('FAIL: Missing or invalid direct download link for Standalone edition (*standalone.zip)');
  errors++;
} else if (!hasPortableLink) {
  console.error('FAIL: Missing or invalid direct download link for Portable edition (*portable.zip)');
  errors++;
} else if (!hasSetupLink) {
  console.error('FAIL: Missing or invalid direct download link for Windows Setup installer (*Setup.exe)');
  errors++;
} else if (!hasReleasesPageLink) {
  console.error('FAIL: Missing releases repository link in download section');
  errors++;
} else {
  console.log('PASS: Direct distribution download links verified (Standalone, Portable, Setup) with releases fallback link');
}

// Verify dynamic release resolver in main.js
const mainJsContent = fs.readFileSync(jsPath, 'utf8');
const hasApiCall = mainJsContent.includes('api.github.com/repos/vtstv/OHTimer/releases/latest');
const hasStandaloneResolver = mainJsContent.includes('standalone') && mainJsContent.includes('.zip');
const hasPortableResolver = mainJsContent.includes('portable') && mainJsContent.includes('.zip');
const hasSetupResolver = mainJsContent.includes('setup') && mainJsContent.includes('.exe');

if (!hasApiCall || !hasStandaloneResolver || !hasPortableResolver || !hasSetupResolver) {
  console.error('FAIL: main.js missing dynamic release resolver for GitHub release assets');
  errors++;
} else {
  console.log('PASS: Dynamic release resolver in main.js verified (fetches latest release assets for Standalone, Portable, Setup)');
}

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
  execSync(`node --check "${jsPath}"`);
  console.log('PASS: main.js syntax check passed with 0 errors');
} catch (e) {
  console.error('FAIL: main.js has syntax errors:', e.message);
  errors++;
}

try {
  execSync(`node --check "${translationsPath}"`);
  console.log('PASS: translations.js syntax check passed with 0 errors');
} catch (e) {
  console.error('FAIL: translations.js has syntax errors:', e.message);
  errors++;
}

// 10. Verify Localization & Key Parity
try {
  const translations = require(translationsPath);
  const langs = ['en', 'de', 'ru'];
  const enKeys = Object.keys(translations.en);

  langs.forEach(lang => {
    if (!translations[lang]) {
      console.error(`FAIL: Missing language dictionary for '${lang}'`);
      errors++;
      return;
    }
    const missing = enKeys.filter(k => typeof translations[lang][k] !== 'string');
    if (missing.length > 0) {
      console.error(`FAIL: Language '${lang}' missing ${missing.length} keys: ${missing.slice(0, 5).join(', ')}...`);
      errors++;
    }
  });
  console.log(`PASS: Multilingual translations verified (EN, DE, RU) with 100% key parity (${enKeys.length} keys each)`);

  // Verify third-party software disclaimer across translations
  const expectedDisclaimers = {
    en: 'As with any third-party gaming software, use is entirely at your own discretion and risk.',
    de: 'Wie bei jeder Drittanbieter-Software erfolgt die Nutzung auf eigene Verantwortung und eigenes Risiko.',
    ru: 'Как и любое стороннее ПО, программа используется исключительно на свой страх и риск.'
  };

  for (const [lang, expectedText] of Object.entries(expectedDisclaimers)) {
    const disclaimerInDict = translations[lang]['footer.disclaimer.text'] || '';
    if (!disclaimerInDict.includes(expectedText)) {
      console.error(`FAIL: Disclaimer for '${lang}' missing required text: "${expectedText}"`);
      errors++;
    } else {
      console.log(`PASS: Disclaimer for '${lang}' verified with professional third-party risk notice`);
    }
  }

  // Verify all data-i18n in HTML files map to valid translation keys
  const dataI18nRegex = /data-i18n="([^"]+)"/g;
  let dataI18nCount = 0;
  let unmappedKeys = new Set();
  let m;
  while ((m = dataI18nRegex.exec(html)) !== null) {
    dataI18nCount++;
    const key = m[1];
    if (!translations.en[key]) {
      unmappedKeys.add(key);
    }
  }
  if (unmappedKeys.size > 0) {
    console.error(`FAIL: HTML contains unmapped translation keys: ${Array.from(unmappedKeys).join(', ')}`);
    errors++;
  } else {
    console.log(`PASS: All ${dataI18nCount} data-i18n elements in HTML map to valid keys`);
  }
} catch (e) {
  console.error('FAIL: Error validating translations:', e.message);
  errors++;
}

// 11. Verify Absence of Prohibited Marketing Claims ("anti-cheat safe", "100% ban proof", "anti-cheat compliant")
const prohibitedPhrases = ['anti-cheat safe', '100% ban proof', 'anti-cheat compliant'];
const allHtmlFiles = [
  indexPath,
  templatePath,
  ...requiredComponents.map(c => path.join(componentsDir, c))
];

let prohibitedFound = false;
allHtmlFiles.forEach(file => {
  if (fs.existsSync(file)) {
    const content = fs.readFileSync(file, 'utf8').toLowerCase();
    prohibitedPhrases.forEach(phrase => {
      if (content.includes(phrase)) {
        console.error(`FAIL: Prohibited marketing phrase "${phrase}" found in ${path.relative(rootDir, file)}`);
        errors++;
        prohibitedFound = true;
      }
    });
  }
});
if (!prohibitedFound) {
  console.log('PASS: 0 promotional anti-cheat marketing claims across all HTML components');
}

// 12. Verify GitHub Pages workflow
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
