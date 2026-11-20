/**
 * Build script for Once Human: Event Timer Website
 * Assembles modular HTML components into production website/index.html and docs/index.html.
 */

const fs = require('fs');
const path = require('path');

const ROOT_DIR = path.resolve(__dirname, '..');
const REPO_ROOT = path.resolve(ROOT_DIR, '..');
const SRC_DIR = path.resolve(ROOT_DIR, 'src');
const COMPONENTS_DIR = path.resolve(SRC_DIR, 'components');
const TEMPLATE_PATH = path.resolve(SRC_DIR, 'template.html');
const OUTPUT_PATH = path.resolve(ROOT_DIR, 'index.html');
const DOCS_DIR = path.resolve(REPO_ROOT, 'docs');
const DOCS_OUTPUT_PATH = path.resolve(DOCS_DIR, 'index.html');

const COMPONENTS = [
  { name: 'header', file: 'header.html' },
  { name: 'hero', file: 'hero.html' },
  { name: 'metrics', file: 'metrics.html' },
  { name: 'features', file: 'features.html' },
  { name: 'screenshots', file: 'screenshots.html' },
  { name: 'download', file: 'download.html' },
  { name: 'faq', file: 'faq.html' },
  { name: 'footer', file: 'footer.html' }
];

function copyFolderRecursiveSync(source, target) {
  if (!fs.existsSync(target)) {
    fs.mkdirSync(target, { recursive: true });
  }
  const files = fs.readdirSync(source);
  files.forEach(file => {
    const curSource = path.join(source, file);
    const curTarget = path.join(target, file);
    if (fs.lstatSync(curSource).isDirectory()) {
      copyFolderRecursiveSync(curSource, curTarget);
    } else {
      fs.copyFileSync(curSource, curTarget);
    }
  });
}

function build() {
  console.log('🚀 Building Once Human: Event Timer Website...');

  if (!fs.existsSync(TEMPLATE_PATH)) {
    console.error(`❌ Template not found at: ${TEMPLATE_PATH}`);
    process.exit(1);
  }

  let html = fs.readFileSync(TEMPLATE_PATH, 'utf8');

  for (const comp of COMPONENTS) {
    const compPath = path.resolve(COMPONENTS_DIR, comp.file);
    if (!fs.existsSync(compPath)) {
      console.error(`❌ Component not found: ${comp.file} at ${compPath}`);
      process.exit(1);
    }
    const compContent = fs.readFileSync(compPath, 'utf8').trim();
    const placeholder = `{{${comp.name}}}`;
    if (!html.includes(placeholder)) {
      console.warn(`⚠️ Warning: Placeholder ${placeholder} not found in template.`);
    }
    html = html.replace(placeholder, compContent);
    console.log(`  ✓ Inserted ${comp.name} (${comp.file})`);
  }

  // Check for any unreplaced placeholders
  const remainingMatches = html.match(/\{\{([a-zA-Z0-9_-]+)\}\}/g);
  if (remainingMatches) {
    console.warn(`⚠️ Warning: Unreplaced placeholders found: ${remainingMatches.join(', ')}`);
  }

  // 1. Output to website/index.html
  fs.writeFileSync(OUTPUT_PATH, html, 'utf8');
  const stats = fs.statSync(OUTPUT_PATH);
  console.log(`✅ Successfully generated ${OUTPUT_PATH} (${(stats.size / 1024).toFixed(2)} KB)`);

  // 2. Output to docs/ for zero-config GitHub Pages (Deploy from a branch -> /docs)
  if (fs.existsSync(DOCS_DIR)) {
    fs.writeFileSync(DOCS_OUTPUT_PATH, html, 'utf8');
    console.log(`✅ Synchronized to ${DOCS_OUTPUT_PATH}`);

    // Sync assets, CNAME, .nojekyll, robots.txt, sitemap.xml, favicon.ico to docs/
    const assetsSrc = path.join(ROOT_DIR, 'assets');
    const assetsDst = path.join(DOCS_DIR, 'assets');
    copyFolderRecursiveSync(assetsSrc, assetsDst);

    const staticFiles = ['CNAME', '.nojekyll', 'robots.txt', 'sitemap.xml', 'favicon.ico'];
    staticFiles.forEach(f => {
      const srcFile = path.join(ROOT_DIR, f);
      const dstFile = path.join(DOCS_DIR, f);
      if (fs.existsSync(srcFile)) {
        fs.copyFileSync(srcFile, dstFile);
      }
    });
    console.log(`✅ Synchronized static assets to ${DOCS_DIR}`);
  }
}

if (require.main === module) {
  build();
}

module.exports = { build };
