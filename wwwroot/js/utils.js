const Utils = (() => {

  const ENTITY_MAP = {
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#x27;',
    '/': '&#x2F;',
    '`': '&#96;'
  };

  function escapeHtml(str) {
    if (typeof str !== 'string') return '';
    return str.replace(/[&<>"'\/`]/g, c => ENTITY_MAP[c]);
  }

  function $(selector) {
    return document.querySelector(selector);
  }

  function $$(selector) {
    return document.querySelectorAll(selector);
  }

  function createElement(tag, attrs, children) {
    const el = document.createElement(tag);
    if (attrs) {
      for (const [key, val] of Object.entries(attrs)) {
        if (key === 'className') {
          el.className = val;
        } else if (key === 'textContent') {
          el.textContent = val;
        } else if (key === 'type') {
          el.type = val;
        } else if (key === 'placeholder') {
          el.placeholder = val;
        } else if (key === 'disabled') {
          el.disabled = val;
        } else if (key === 'dataset') {
          for (const [dk, dv] of Object.entries(val)) {
            el.dataset[dk] = dv;
          }
        } else if (key.startsWith('on') && typeof val === 'function') {
          el.addEventListener(key.slice(2).toLowerCase(), val);
        } else {
          el.setAttribute(key, val);
        }
      }
    }
    if (children) {
      if (!Array.isArray(children)) children = [children];
      children.forEach(child => {
        if (typeof child === 'string') {
          el.appendChild(document.createTextNode(child));
        } else if (child instanceof Node) {
          el.appendChild(child);
        }
      });
    }
    return el;
  }

  function clearElement(el) {
    while (el.firstChild) {
      el.removeChild(el.firstChild);
    }
  }

  function show(el) {
    if (typeof el === 'string') el = $(el);
    if (el) el.classList.remove('hidden');
  }

  function hide(el) {
    if (typeof el === 'string') el = $(el);
    if (el) el.classList.add('hidden');
  }

  function setMessage(containerId, text, type) {
    const container = $(containerId);
    if (!container) return;
    clearElement(container);
    container.className = 'message message-' + escapeHtml(type);
    container.textContent = text;
    show(container);
  }

  function clearMessage(containerId) {
    const container = $(containerId);
    if (!container) return;
    clearElement(container);
    hide(container);
  }

  function validateEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  function validatePassword(pw) {
    return /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[\W_]).{8,}$/.test(pw);
  }

  function formatDate(isoStr) {
    try {
      return new Date(isoStr).toLocaleDateString('es-ES', {
        year: 'numeric', month: 'short', day: 'numeric',
        hour: '2-digit', minute: '2-digit'
      });
    } catch {
      return isoStr;
    }
  }

  function debounce(fn, ms) {
    let timer;
    return function (...args) {
      clearTimeout(timer);
      timer = setTimeout(() => fn.apply(this, args), ms);
    };
  }

  function maskValue(val) {
    if (!val || typeof val !== 'string') return '';
    if (val.length <= 4) return '****';
    return val.substring(0, 2) + '*'.repeat(Math.min(val.length - 4, 20)) + val.substring(val.length - 2);
  }

  return {
    escapeHtml,
    $,
    $$,
    createElement,
    clearElement,
    show,
    hide,
    setMessage,
    clearMessage,
    validateEmail,
    validatePassword,
    formatDate,
    debounce,
    maskValue
  };

})();
