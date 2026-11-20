/**
 * Build script for Once Human: Event Timer Website
 * Assembles modular HTML components into production docs/website/index.html.
 */

const fs = require('fs');
const path = require('path');

const ROOT_DIR = path.resolve(__dirname, '..'); // docs/website
const SRC_DIR = path.resolve(ROOT_DIR, 'src');
const COMPONENTS_DIR = path.resolve(SRC_DIR, 'components');
const TEMPLATE_PATH = path.resolve(SRC_DIR, 'template.html');
const OUTPUT_PATH = path.resolve(ROOT_DIR, 'index.html');

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

  fs.writeFileSync(OUTPUT_PATH, html, 'utf8');
  const stats = fs.statSync(OUTPUT_PATH);
  console.log(`✅ Successfully generated ${OUTPUT_PATH} (${(stats.size / 1024).toFixed(2)} KB)`);
}

if (require.main === module) {
  build();
}

module.exports = { build };
