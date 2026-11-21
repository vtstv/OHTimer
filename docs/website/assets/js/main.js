/**
 * Once Human: Event Timer — Official Landing Page JavaScript
 * Features: Dark/Light theme toggle, interactive gallery lightbox,
 * hero preview switcher, hotkey copy helper, FAQ accordion, smooth scrolling.
 */

(function () {
  'use strict';

  // ========================================================================
  // 1. Theme Management (Dark / Light Theme with localStorage persistence)
  // ========================================================================
  const THEME_STORAGE_KEY = 'ohtimer-theme';
  const themeToggleBtn = document.getElementById('theme-toggle-btn');

  function getPreferredTheme() {
    const savedTheme = localStorage.getItem(THEME_STORAGE_KEY);
    if (savedTheme === 'light' || savedTheme === 'dark') {
      return savedTheme;
    }
    // Default to dark theme for Once Human aesthetic
    return 'dark';
  }

  function applyTheme(theme) {
    if (theme === 'light') {
      document.documentElement.setAttribute('data-theme', 'light');
      const metaTheme = document.querySelector('meta[name="theme-color"]');
      if (metaTheme) metaTheme.setAttribute('content', '#f8fafc');
    } else {
      document.documentElement.removeAttribute('data-theme');
      const metaTheme = document.querySelector('meta[name="theme-color"]');
      if (metaTheme) metaTheme.setAttribute('content', '#070a0f');
    }
    if (themeToggleBtn) {
      themeToggleBtn.setAttribute('aria-label', theme === 'light' ? 'Switch to dark theme' : 'Switch to light theme');
    }
  }

  // Initialize theme
  const currentTheme = getPreferredTheme();
  applyTheme(currentTheme);

  if (themeToggleBtn) {
    themeToggleBtn.addEventListener('click', () => {
      const isLight = document.documentElement.getAttribute('data-theme') === 'light';
      const newTheme = isLight ? 'dark' : 'light';
      localStorage.setItem(THEME_STORAGE_KEY, newTheme);
      applyTheme(newTheme);
    });
  }

  // ========================================================================
  // 2. Multilingual Language Management (EN default, DE, RU)
  // ========================================================================
  const LANG_STORAGE_KEY = 'ohtimer-lang';
  const langSelector = document.getElementById('lang-selector');
  const langBtn = document.getElementById('lang-btn');
  const langCurrent = document.getElementById('lang-current');
  const langOptions = document.querySelectorAll('.lang-option');

  const LANG_NAMES = {
    en: 'English',
    de: 'Deutsch',
    ru: 'Русский'
  };

  let currentLang = 'en';

  function getPreferredLanguage() {
    const savedLang = localStorage.getItem(LANG_STORAGE_KEY);
    if (savedLang === 'en' || savedLang === 'de' || savedLang === 'ru') {
      return savedLang;
    }
    const navLang = (navigator.language || navigator.userLanguage || '').toLowerCase();
    if (navLang.startsWith('de')) return 'de';
    if (navLang.startsWith('ru')) return 'ru';
    return 'en';
  }

  function applyLanguage(lang) {
    if (!LANG_NAMES[lang]) {
      lang = 'en';
    }
    currentLang = lang;
    localStorage.setItem(LANG_STORAGE_KEY, lang);
    document.documentElement.setAttribute('lang', lang);

    if (langCurrent) {
      langCurrent.textContent = LANG_NAMES[lang];
    }

    langOptions.forEach(opt => {
      if (opt.getAttribute('data-lang') === lang) {
        opt.classList.add('active');
      } else {
        opt.classList.remove('active');
      }
    });

    const dict = (window.OH_TRANSLATIONS && window.OH_TRANSLATIONS[lang]) || (window.OH_TRANSLATIONS && window.OH_TRANSLATIONS.en);
    if (!dict) return;

    // Apply translations to all [data-i18n] elements
    const translatableElements = document.querySelectorAll('[data-i18n]');
    translatableElements.forEach(el => {
      const key = el.getAttribute('data-i18n');
      if (dict[key]) {
        el.innerHTML = dict[key];
      }
    });
  }

  // Initialize Language
  currentLang = getPreferredLanguage();
  applyLanguage(currentLang);

  // Dropdown UI Interaction
  if (langBtn && langSelector) {
    const optionsArray = Array.from(langOptions);

    langBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      const isOpen = langSelector.classList.toggle('open');
      langBtn.setAttribute('aria-expanded', isOpen);
      if (isOpen) {
        const activeOpt = langSelector.querySelector('.lang-option.active') || optionsArray[0];
        if (activeOpt) activeOpt.focus();
      }
    });

    optionsArray.forEach((opt, idx) => {
      opt.addEventListener('click', (e) => {
        e.stopPropagation();
        const selectedLang = opt.getAttribute('data-lang');
        applyLanguage(selectedLang);
        langSelector.classList.remove('open');
        langBtn.setAttribute('aria-expanded', 'false');
        langBtn.focus();
      });

      opt.addEventListener('keydown', (e) => {
        if (e.key === 'ArrowDown') {
          e.preventDefault();
          const next = optionsArray[(idx + 1) % optionsArray.length];
          next.focus();
        } else if (e.key === 'ArrowUp') {
          e.preventDefault();
          const prev = optionsArray[(idx - 1 + optionsArray.length) % optionsArray.length];
          prev.focus();
        } else if (e.key === 'Home') {
          e.preventDefault();
          optionsArray[0].focus();
        } else if (e.key === 'End') {
          e.preventDefault();
          optionsArray[optionsArray.length - 1].focus();
        }
      });
    });

    // Close on outside click
    document.addEventListener('click', (e) => {
      if (!langSelector.contains(e.target)) {
        langSelector.classList.remove('open');
        langBtn.setAttribute('aria-expanded', 'false');
      }
    });

    // Close when focus leaves the language selector
    langSelector.addEventListener('focusout', (e) => {
      if (!langSelector.contains(e.relatedTarget)) {
        langSelector.classList.remove('open');
        langBtn.setAttribute('aria-expanded', 'false');
      }
    });

    // Close on Escape key
    document.addEventListener('keydown', (e) => {
      if (e.key === 'Escape' && langSelector.classList.contains('open')) {
        langSelector.classList.remove('open');
        langBtn.setAttribute('aria-expanded', 'false');
        langBtn.focus();
      }
    });
  }

  // ========================================================================
  // 3. Mobile Navigation Menu
  // ========================================================================
  const mobileToggleBtn = document.getElementById('nav-mobile-toggle');
  const navLinksList = document.getElementById('nav-links');

  if (mobileToggleBtn && navLinksList) {
    mobileToggleBtn.addEventListener('click', () => {
      const isOpen = navLinksList.classList.toggle('mobile-open');
      mobileToggleBtn.setAttribute('aria-expanded', isOpen);
    });

    // Close menu when clicking any nav link
    const links = navLinksList.querySelectorAll('.nav-link');
    links.forEach(link => {
      link.addEventListener('click', () => {
        navLinksList.classList.remove('mobile-open');
        mobileToggleBtn.setAttribute('aria-expanded', 'false');
      });
    });

    // Close menu on document click outside
    document.addEventListener('click', (e) => {
      if (!navLinksList.contains(e.target) && !mobileToggleBtn.contains(e.target)) {
        navLinksList.classList.remove('mobile-open');
        mobileToggleBtn.setAttribute('aria-expanded', 'false');
      }
    });
  }

  // ========================================================================
  // 4. Hero Mockup Preview Switcher
  // ========================================================================
  const mockupTabs = document.querySelectorAll('.mockup-tab-btn');
  const mockupImage = document.getElementById('hero-mockup-img');
  const mockupCaption = document.getElementById('hero-mockup-caption');

  const MOCKUP_DATA = {
    multitimer: {
      src: 'assets/images/hud-multitimer.png',
      alt: 'Multi-Timer HUD Overlay',
      caption: 'Full HUD tracking multiple concurrent events with circular countdown rings & progress bars'
    },
    mini: {
      src: 'assets/images/hud-mini.png',
      alt: 'Minimal Floating Mini-HUD',
      caption: 'Ultra-compact floating Mini-HUD mode for high-intensity boss encounters'
    },
    settings: {
      src: 'assets/images/settings-events.jpg',
      alt: 'Event Presets & Server Synchronization',
      caption: 'Universal event scheduling engine with 15 Once Human meta presets and zero-drift calibration'
    }
  };

  mockupTabs.forEach(tab => {
    tab.addEventListener('click', () => {
      mockupTabs.forEach(t => t.classList.remove('active'));
      tab.classList.add('active');

      const view = tab.getAttribute('data-view');
      const data = MOCKUP_DATA[view];
      if (data && mockupImage) {
        mockupImage.style.opacity = '0';
        setTimeout(() => {
          mockupImage.src = data.src;
          mockupImage.alt = data.alt;
          const dict = (window.OH_TRANSLATIONS && window.OH_TRANSLATIONS[currentLang]) || (window.OH_TRANSLATIONS && window.OH_TRANSLATIONS.en);
          const captionKey = 'hero.mockup.caption.' + view;
          if (mockupCaption) {
            mockupCaption.textContent = (dict && dict[captionKey]) || data.caption;
            mockupCaption.setAttribute('data-i18n', captionKey);
          }
          mockupImage.style.opacity = '1';
        }, 150);
      }
    });
  });

  // ========================================================================
  // 5. Interactive Screenshots Lightbox & Filter Bar
  // ========================================================================
  const galleryCards = document.querySelectorAll('.gallery-card');
  const filterBtns = document.querySelectorAll('.filter-btn');
  const lightboxModal = document.getElementById('lightbox-modal');
  const lightboxImg = document.getElementById('lightbox-img');
  const lightboxTitle = document.getElementById('lightbox-title');
  const lightboxDesc = document.getElementById('lightbox-desc');
  const lightboxCloseBtn = document.getElementById('lightbox-close-btn');
  const lightboxPrevBtn = document.getElementById('lightbox-prev-btn');
  const lightboxNextBtn = document.getElementById('lightbox-next-btn');

  let currentGalleryIndex = 0;
  let lastFocusedElement = null;
  const visibleCards = [];

  function updateVisibleCards() {
    visibleCards.length = 0;
    galleryCards.forEach(card => {
      if (window.getComputedStyle(card).display !== 'none') {
        visibleCards.push(card);
      }
    });
  }

  // Filter functionality
  filterBtns.forEach(btn => {
    btn.addEventListener('click', () => {
      filterBtns.forEach(b => b.classList.remove('active'));
      btn.classList.add('active');

      const filter = btn.getAttribute('data-filter');
      galleryCards.forEach(card => {
        const cat = card.getAttribute('data-category');
        if (filter === 'all' || cat === filter) {
          card.style.display = 'flex';
        } else {
          card.style.display = 'none';
        }
      });
      updateVisibleCards();
    });
  });

  // Lightbox open
  function openLightbox(index) {
    if (index < 0 || index >= visibleCards.length) return;
    currentGalleryIndex = index;
    lastFocusedElement = document.activeElement;

    const card = visibleCards[currentGalleryIndex];
    const imgEl = card.querySelector('img');
    const titleEl = card.querySelector('.gallery-title');
    const descEl = card.querySelector('.gallery-desc');

    if (lightboxImg && imgEl) {
      lightboxImg.src = imgEl.src;
      lightboxImg.alt = imgEl.alt;
    }
    if (lightboxTitle && titleEl) {
      lightboxTitle.textContent = titleEl.textContent;
    }
    if (lightboxDesc && descEl) {
      lightboxDesc.textContent = descEl.textContent;
    }

    if (lightboxModal) {
      lightboxModal.classList.add('open');
      lightboxModal.setAttribute('aria-hidden', 'false');
      document.body.style.overflow = 'hidden';
      if (lightboxCloseBtn) {
        lightboxCloseBtn.focus();
      }
    }
  }

  function closeLightbox() {
    if (lightboxModal) {
      lightboxModal.classList.remove('open');
      lightboxModal.setAttribute('aria-hidden', 'true');
      document.body.style.overflow = '';
      if (lastFocusedElement && typeof lastFocusedElement.focus === 'function') {
        lastFocusedElement.focus();
      }
    }
  }

  function nextLightbox() {
    if (visibleCards.length === 0) return;
    const nextIndex = (currentGalleryIndex + 1) % visibleCards.length;
    openLightbox(nextIndex);
  }

  function prevLightbox() {
    if (visibleCards.length === 0) return;
    const prevIndex = (currentGalleryIndex - 1 + visibleCards.length) % visibleCards.length;
    openLightbox(prevIndex);
  }

  updateVisibleCards();

  galleryCards.forEach(card => {
    const handleTrigger = () => {
      updateVisibleCards();
      const idx = visibleCards.indexOf(card);
      if (idx !== -1) {
        openLightbox(idx);
      }
    };

    card.addEventListener('click', handleTrigger);
    card.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        handleTrigger();
      }
    });
  });

  if (lightboxCloseBtn) lightboxCloseBtn.addEventListener('click', closeLightbox);
  if (lightboxNextBtn) lightboxNextBtn.addEventListener('click', nextLightbox);
  if (lightboxPrevBtn) lightboxPrevBtn.addEventListener('click', prevLightbox);

  if (lightboxModal) {
    lightboxModal.addEventListener('click', (e) => {
      if (e.target === lightboxModal) {
        closeLightbox();
      }
    });
  }

  // Keyboard navigation & Focus trap for Lightbox
  document.addEventListener('keydown', (e) => {
    if (!lightboxModal || !lightboxModal.classList.contains('open')) return;

    if (e.key === 'Escape') {
      closeLightbox();
    } else if (e.key === 'ArrowRight') {
      nextLightbox();
    } else if (e.key === 'ArrowLeft') {
      prevLightbox();
    } else if (e.key === 'Tab') {
      // Focus trap within modal
      const focusableElements = [lightboxCloseBtn, lightboxPrevBtn, lightboxNextBtn].filter(Boolean);
      if (focusableElements.length === 0) return;

      const firstEl = focusableElements[0];
      const lastEl = focusableElements[focusableElements.length - 1];

      if (e.shiftKey) {
        if (document.activeElement === firstEl) {
          e.preventDefault();
          lastEl.focus();
        }
      } else {
        if (document.activeElement === lastEl) {
          e.preventDefault();
          firstEl.focus();
        }
      }
    }
  });

  // ========================================================================
  // 6. Hotkey Copy Helper (with modern navigator.clipboard and fallback)
  // ========================================================================
  function copyTextToClipboard(text) {
    if (navigator.clipboard && window.isSecureContext) {
      return navigator.clipboard.writeText(text);
    }
    // Fallback for non-HTTPS local tests or older browsers
    return new Promise((resolve, reject) => {
      try {
        const tempInput = document.createElement('textarea');
        tempInput.value = text;
        tempInput.style.position = 'fixed';
        tempInput.style.left = '-9999px';
        tempInput.style.top = '-9999px';
        document.body.appendChild(tempInput);
        tempInput.focus();
        tempInput.select();
        const successful = document.execCommand('copy');
        document.body.removeChild(tempInput);
        if (successful) resolve();
        else reject(new Error('execCommand copy failed'));
      } catch (err) {
        reject(err);
      }
    });
  }

  const copyHotkeyBtns = document.querySelectorAll('.copy-hotkey-btn');

  copyHotkeyBtns.forEach(btn => {
    btn.addEventListener('click', (e) => {
      e.stopPropagation();
      const shortcut = btn.getAttribute('data-shortcut');
      if (!shortcut) return;

      copyTextToClipboard(shortcut).then(() => {
        const originalText = btn.innerHTML;
        btn.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="width:14px;height:14px;color:var(--emerald-primary)"><polyline points="20 6 9 17 4 12"></polyline></svg> Copied!`;
        btn.style.color = 'var(--emerald-primary)';
        btn.style.borderColor = 'var(--emerald-primary)';

        setTimeout(() => {
          btn.innerHTML = originalText;
          btn.style.color = '';
          btn.style.borderColor = '';
        }, 1800);
      }).catch(err => {
        console.error('Failed to copy hotkey:', err);
      });
    });
  });

  // ========================================================================
  // 7. FAQ Accordion
  // ========================================================================
  const faqItems = document.querySelectorAll('.faq-item');

  faqItems.forEach(item => {
    const question = item.querySelector('.faq-question');
    if (!question) return;

    question.addEventListener('click', () => {
      const isActive = item.classList.contains('active');

      // Close all other items
      faqItems.forEach(other => {
        if (other !== item) {
          other.classList.remove('active');
          const otherQ = other.querySelector('.faq-question');
          if (otherQ) otherQ.setAttribute('aria-expanded', 'false');
        }
      });

      // Toggle current
      if (isActive) {
        item.classList.remove('active');
        question.setAttribute('aria-expanded', 'false');
      } else {
        item.classList.add('active');
        question.setAttribute('aria-expanded', 'true');
      }
    });
  });

  // ========================================================================
  // 8. Active Navigation Link on Scroll
  // ========================================================================
  const sections = document.querySelectorAll('section[id]');
  const navLinks = document.querySelectorAll('.nav-link[href^="#"]');

  function updateActiveNavLink() {
    const scrollY = window.pageYOffset;

    sections.forEach(sec => {
      const sectionHeight = sec.offsetHeight;
      const sectionTop = sec.offsetTop - 120;
      const sectionId = sec.getAttribute('id');

      if (scrollY >= sectionTop && scrollY < sectionTop + sectionHeight) {
        navLinks.forEach(link => {
          if (link.getAttribute('href') === `#${sectionId}`) {
            link.classList.add('active');
          } else {
            link.classList.remove('active');
          }
        });
      }
    });
  }

  window.addEventListener('scroll', updateActiveNavLink, { passive: true });

})();
