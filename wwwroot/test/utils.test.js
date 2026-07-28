describe('escapeHtml', function () {
  it('escapa & a &amp;', function () { expect(Utils.escapeHtml('&')).toBe('&amp;'); });
  it('escapa < a &lt;', function () { expect(Utils.escapeHtml('<')).toBe('&lt;'); });
  it('escapa > a &gt;', function () { expect(Utils.escapeHtml('>')).toBe('&gt;'); });
  it('escapa " a &quot;', function () { expect(Utils.escapeHtml('"')).toBe('&quot;'); });
  it('escapa \' a &#x27;', function () { expect(Utils.escapeHtml("'")).toBe('&#x27;'); });
  it('escapa / a &#x2F;', function () { expect(Utils.escapeHtml('/')).toBe('&#x2F;'); });
  it('escapa ` a &#96;', function () { expect(Utils.escapeHtml('`')).toBe('&#96;'); });
  it('retorna string vacio para input no-string', function () { expect(Utils.escapeHtml(123)).toBe(''); });
  it('retorna string original sin caracteres especiales', function () { expect(Utils.escapeHtml('hola mundo')).toBe('hola mundo'); });
  it('escapa multiples caracteres en un string', function () { expect(Utils.escapeHtml('<script>alert("xss")</script>')).toBe('&lt;script&gt;alert(&quot;xss&quot;)&lt;&#x2F;script&gt;'); });
});

describe('validateEmail', function () {
  it('retorna true para email valido', function () { expect(Utils.validateEmail('test@example.com')).toBeTruthy(); });
  it('retorna true para email con subdominio', function () { expect(Utils.validateEmail('user@sub.domain.com')).toBeTruthy(); });
  it('retorna true para email con +', function () { expect(Utils.validateEmail('user+tag@domain.com')).toBeTruthy(); });
  it('retorna false para sin @', function () { expect(Utils.validateEmail('noatsign.com')).toBeFalsy(); });
  it('retorna false para @ al inicio', function () { expect(Utils.validateEmail('@domain.com')).toBeFalsy(); });
  it('retorna false para sin dominio', function () { expect(Utils.validateEmail('user@')).toBeFalsy(); });
  it('retorna false para string vacio', function () { expect(Utils.validateEmail('')).toBeFalsy(); });
  it('retorna false para email con espacios', function () { expect(Utils.validateEmail('user @domain.com')).toBeFalsy(); });
});

describe('validatePassword', function () {
  it('retorna true para password segura', function () { expect(Utils.validatePassword('Test@1234')).toBeTruthy(); });
  it('retorna false para password muy corto', function () { expect(Utils.validatePassword('T@1a')).toBeFalsy(); });
  it('retorna false sin mayuscula', function () { expect(Utils.validatePassword('test@1234')).toBeFalsy(); });
  it('retorna false sin minuscula', function () { expect(Utils.validatePassword('TEST@1234')).toBeFalsy(); });
  it('retorna false sin digito', function () { expect(Utils.validatePassword('Test@abcd')).toBeFalsy(); });
  it('retorna false sin caracter especial', function () { expect(Utils.validatePassword('Test1234a')).toBeFalsy(); });
});

describe('maskValue', function () {
  it('enmascara string largo mostrando primeros 2 y ultimos 2', function () { expect(Utils.maskValue('ABCDEFGHIJ')).toBe('AB******IJ'); });
  it('retorna **** para string de 4 o menos', function () { expect(Utils.maskValue('AB')).toBe('****'); });
  it('retorna **** para string de exactamente 4', function () { expect(Utils.maskValue('ABCD')).toBe('****'); });
  it('retorna string vacio para null', function () { expect(Utils.maskValue(null)).toBe(''); });
  it('retorna string vacio para undefined', function () { expect(Utils.maskValue(undefined)).toBe(''); });
  it('retorna string vacio para no-string', function () { expect(Utils.maskValue(12345)).toBe(''); });
});

describe('formatDate', function () {
  it('formatea una fecha ISO valida', function () {
    var result = Utils.formatDate('2026-01-15T10:30:00Z');
    expect(typeof result).toBe('string');
    expect(result.length).toBeGreaterThan(0);
  });
  it('retorna el string original para input invalido', function () {
    expect(Utils.formatDate('not-a-date')).toBe('not-a-date');
  });
});

describe('createElement', function () {
  it('crea un elemento con el tag correcto', function () {
    var el = Utils.createElement('div');
    expect(el.tagName).toBe('DIV');
  });
  it('establece className', function () {
    var el = Utils.createElement('div', { className: 'test-class' });
    expect(el.className).toBe('test-class');
  });
  it('establece textContent', function () {
    var el = Utils.createElement('span', { textContent: 'hola' });
    expect(el.textContent).toBe('hola');
  });
  it('establece type', function () {
    var el = Utils.createElement('input', { type: 'password' });
    expect(el.type).toBe('password');
  });
  it('establece placeholder', function () {
    var el = Utils.createElement('input', { placeholder: 'Escribe aqui' });
    expect(el.placeholder).toBe('Escribe aqui');
  });
  it('establece dataset', function () {
    var el = Utils.createElement('div', { dataset: { id: '123', name: 'test' } });
    expect(el.dataset.id).toBe('123');
    expect(el.dataset.name).toBe('test');
  });
  it('agrega hijos de tipo string como text node', function () {
    var el = Utils.createElement('div', null, ['texto']);
    expect(el.childNodes.length).toBe(1);
    expect(el.childNodes[0].textContent).toBe('texto');
  });
  it('agrega hijos de tipo Node', function () {
    var child = Utils.createElement('span', { textContent: 'hi' });
    var parent = Utils.createElement('div', null, [child]);
    expect(parent.childNodes.length).toBe(1);
    expect(parent.childNodes[0].tagName).toBe('SPAN');
  });
  it('agrega listener via onClick', function () {
    var clicked = false;
    var el = Utils.createElement('button', { onClick: function () { clicked = true; } });
    el.click();
    expect(clicked).toBeTruthy();
  });
});

describe('clearElement', function () {
  it('elimina todos los hijos', function () {
    var el = document.createElement('div');
    el.appendChild(document.createElement('span'));
    el.appendChild(document.createElement('p'));
    expect(el.childNodes.length).toBe(2);
    Utils.clearElement(el);
    expect(el.childNodes.length).toBe(0);
  });
});

describe('show / hide', function () {
  it('hide agrega clase hidden', function () {
    var el = document.createElement('div');
    Utils.hide(el);
    expect(el.classList.contains('hidden')).toBeTruthy();
  });
  it('show remueve clase hidden', function () {
    var el = document.createElement('div');
    el.classList.add('hidden');
    Utils.show(el);
    expect(el.classList.contains('hidden')).toBeFalsy();
  });
  it('hide con selector string funciona', function () {
    var el = document.createElement('div');
    el.id = 'test-hide-el';
    document.body.appendChild(el);
    Utils.hide('#test-hide-el');
    expect(el.classList.contains('hidden')).toBeTruthy();
    document.body.removeChild(el);
  });
});
