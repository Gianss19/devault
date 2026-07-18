const App = (() => {

  let _currentView = 'login';

  function init() {
    _bindNavigation();
    showView('login');
    _checkHealth();
  }

  function _checkHealth() {
    API.healthCheck().then(res => {
      const indicator = Utils.$('#health-status');
      if (!indicator) return;
      if (res.ok) {
        indicator.textContent = 'API: Conectada';
        indicator.className = 'health-ok';
      } else {
        indicator.textContent = 'API: No disponible';
        indicator.className = 'health-err';
      }
    }).catch(() => {
      const indicator = Utils.$('#health-status');
      if (indicator) {
        indicator.textContent = 'API: No disponible';
        indicator.className = 'health-err';
      }
    });
  }

  function _bindNavigation() {
    Utils.$('#nav-secrets')?.addEventListener('click', () => showView('secrets'));
    Utils.$('#nav-users')?.addEventListener('click', () => showView('users'));
    Utils.$('#nav-profile')?.addEventListener('click', () => showView('profile'));
    Utils.$('#nav-logout')?.addEventListener('click', _handleLogout);
  }

  function showView(viewName) {
    _currentView = viewName;

    Utils.$$('.view-section').forEach(el => el.classList.add('hidden'));
    Utils.$$('.nav-btn').forEach(el => el.classList.remove('active'));

    if (!Auth.isAuthenticated()) {
      Utils.$('#app-bar')?.classList.add('hidden');
      Utils.$('#auth-sections')?.classList.remove('hidden');

      if (viewName === 'signup') {
        Utils.$('#signup-view')?.classList.remove('hidden');
        _renderSignupForm();
      } else {
        Utils.$('#login-view')?.classList.remove('hidden');
        _renderLoginForm();
      }
      return;
    }

    Utils.$('#app-bar')?.classList.remove('hidden');
    Utils.$('#auth-sections')?.classList.add('hidden');
    _updateUserInfo();

    const navMap = {
      secrets: '#nav-secrets',
      users: '#nav-users',
      profile: '#nav-profile'
    };
    if (navMap[viewName]) {
      Utils.$(navMap[viewName])?.classList.add('active');
    }

    switch (viewName) {
      case 'login':
        Utils.$('#login-view')?.classList.remove('hidden');
        _renderLoginForm();
        break;
      case 'signup':
        Utils.$('#signup-view')?.classList.remove('hidden');
        _renderSignupForm();
        break;
      case 'secrets':
        Utils.$('#secrets-view')?.classList.remove('hidden');
        _loadSecrets();
        break;
      case 'users':
        Utils.$('#users-view')?.classList.remove('hidden');
        if (Auth.isAdmin()) {
          _loadUsers();
        } else {
          _renderAccessDenied();
        }
        break;
      case 'profile':
        Utils.$('#profile-view')?.classList.remove('hidden');
        _renderProfile();
        break;
    }
  }

  function _updateUserInfo() {
    const user = Auth.getUser();
    const el = Utils.$('#user-info');
    if (el && user) {
      el.textContent = user.name + ' (' + Auth.getRole() + ')';
    }
  }

  function _handleLogout() {
    API.logout().then(() => {
      Auth.logout();
      showView('login');
      Utils.setMessage('#global-message', 'Sesion cerrada correctamente.', 'success');
    }).catch(() => {
      Auth.logout();
      showView('login');
    });
  }

  // ==================== LOGIN ====================

  function _renderLoginForm() {
    const container = Utils.$('#login-view');
    Utils.clearElement(container);

    const form = Utils.createElement('form', { className: 'auth-form', id: 'login-form' });
    const title = Utils.createElement('h2', { textContent: 'Iniciar Sesion' });
    form.appendChild(title);

    form.appendChild(_createInput('login-email', 'email', 'Correo electronico', true));
    form.appendChild(_createInput('login-password', 'password', 'Contrasena', true));

    const btn = Utils.createElement('button', { type: 'submit', className: 'btn btn-primary', textContent: 'Entrar' });
    form.appendChild(btn);

    const link = Utils.createElement('p', { className: 'auth-link' });
    link.textContent = 'No tienes cuenta? ';
    const a = Utils.createElement('a', {
      textContent: 'Registrate',
      onClick: (e) => { e.preventDefault(); showView('signup'); }
    });
    link.appendChild(a);
    form.appendChild(link);

    form.addEventListener('submit', _handleLogin);
    container.appendChild(form);
  }

  async function _handleLogin(e) {
    e.preventDefault();
    Utils.clearMessage('#login-msg');

    const email = Utils.$('#login-email')?.value?.trim();
    const password = Utils.$('#login-password')?.value;

    if (!email || !password) {
      Utils.setMessage('#login-msg', 'Completa todos los campos.', 'error');
      return;
    }

    if (!Utils.validateEmail(email)) {
      Utils.setMessage('#login-msg', 'Formato de correo invalido.', 'error');
      return;
    }

    const btn = Utils.$('#login-form button[type="submit"]');
    if (btn) { btn.disabled = true; btn.textContent = 'Entrando...'; }

    try {
      const res = await API.login(email, password);
      if (res.ok && res.data) {
        Auth.setSession(res.data.accessToken, res.data.refreshToken);
        showView('secrets');
        Utils.setMessage('#global-message', 'Bienvenido, ' + Auth.getUser().name + '!', 'success');
      } else {
        Utils.setMessage('#login-msg', res.data || 'Credenciales invalidas.', 'error');
      }
    } catch (err) {
      Utils.setMessage('#login-msg', 'Error de conexion con el servidor.', 'error');
    } finally {
      if (btn) { btn.disabled = false; btn.textContent = 'Entrar'; }
    }
  }

  // ==================== SIGNUP ====================

  function _renderSignupForm() {
    const container = Utils.$('#signup-view');
    Utils.clearElement(container);

    const form = Utils.createElement('form', { className: 'auth-form', id: 'signup-form' });
    const title = Utils.createElement('h2', { textContent: 'Crear Cuenta' });
    form.appendChild(title);

    form.appendChild(_createInput('signup-name', 'text', 'Nombre (3-100 caracteres)', true));
    form.appendChild(_createInput('signup-email', 'email', 'Correo electronico', true));
    form.appendChild(_createInput('signup-password', 'password', 'Contrasena', true));

    const pwHint = Utils.createElement('p', { className: 'hint', textContent: 'Min 8 caracteres, 1 mayuscula, 1 minuscula, 1 digito, 1 especial.' });
    form.appendChild(pwHint);

    form.appendChild(_createInput('signup-password-confirm', 'password', 'Confirmar contrasena', true));

    const btn = Utils.createElement('button', { type: 'submit', className: 'btn btn-primary', textContent: 'Registrarse' });
    form.appendChild(btn);

    const link = Utils.createElement('p', { className: 'auth-link' });
    link.textContent = 'Ya tienes cuenta? ';
    const a = Utils.createElement('a', {
      textContent: 'Inicia sesion',
      onClick: (e) => { e.preventDefault(); showView('login'); }
    });
    link.appendChild(a);
    form.appendChild(link);

    form.addEventListener('submit', _handleSignup);
    container.appendChild(form);
  }

  async function _handleSignup(e) {
    e.preventDefault();
    Utils.clearMessage('#signup-msg');

    const name = Utils.$('#signup-name')?.value?.trim();
    const email = Utils.$('#signup-email')?.value?.trim();
    const password = Utils.$('#signup-password')?.value;
    const confirm = Utils.$('#signup-password-confirm')?.value;

    if (!name || !email || !password || !confirm) {
      Utils.setMessage('#signup-msg', 'Completa todos los campos.', 'error');
      return;
    }

    if (name.length < 3 || name.length > 100) {
      Utils.setMessage('#signup-msg', 'El nombre debe tener entre 3 y 100 caracteres.', 'error');
      return;
    }

    if (!Utils.validateEmail(email)) {
      Utils.setMessage('#signup-msg', 'Formato de correo invalido.', 'error');
      return;
    }

    if (!Utils.validatePassword(password)) {
      Utils.setMessage('#signup-msg', 'La contrasena no cumple los requisitos de seguridad.', 'error');
      return;
    }

    if (password !== confirm) {
      Utils.setMessage('#signup-msg', 'Las contrasenas no coinciden.', 'error');
      return;
    }

    const btn = Utils.$('#signup-form button[type="submit"]');
    if (btn) { btn.disabled = true; btn.textContent = 'Registrando...'; }

    try {
      const res = await API.signup(name, email, password);
      if (res.ok) {
        Utils.setMessage('#global-message', 'Cuenta creada. Ahora inicia sesion.', 'success');
        showView('login');
      } else {
        const msg = typeof res.data === 'string' ? res.data : (res.data?.error || 'Error al registrar.');
        Utils.setMessage('#signup-msg', msg, 'error');
      }
    } catch (err) {
      Utils.setMessage('#signup-msg', 'Error de conexion con el servidor.', 'error');
    } finally {
      if (btn) { btn.disabled = false; btn.textContent = 'Registrarse'; }
    }
  }

  // ==================== SECRETS ====================

  async function _loadSecrets() {
    const container = Utils.$('#secrets-content');
    Utils.clearElement(container);
    container.appendChild(Utils.createElement('p', { textContent: 'Cargando secretos...', className: 'loading' }));

    try {
      const res = await API.getAllSecrets();
      if (!res.ok) {
        Utils.clearElement(container);
        Utils.setMessage('#secrets-msg', 'Error al cargar secretos.', 'error');
        return;
      }

      Utils.clearElement(container);
      const secrets = Array.isArray(res.data) ? res.data : [];

      const header = Utils.createElement('div', { className: 'section-header' });
      header.appendChild(Utils.createElement('h3', { textContent: 'Mis Secretos (' + secrets.length + ')' }));
      const addBtn = Utils.createElement('button', {
        className: 'btn btn-primary btn-sm',
        textContent: '+ Nuevo Secreto',
        onClick: () => _showCreateSecretForm()
      });
      header.appendChild(addBtn);
      container.appendChild(header);

      const createFormContainer = Utils.createElement('div', { id: 'create-secret-form-container' });
      container.appendChild(createFormContainer);

      if (secrets.length === 0) {
        container.appendChild(Utils.createElement('p', { className: 'empty-state', textContent: 'No tienes secretos aun.' }));
        return;
      }

      const list = Utils.createElement('div', { className: 'card-grid' });
      secrets.forEach(secret => {
        const card = _createSecretCard(secret);
        list.appendChild(card);
      });
      container.appendChild(list);

    } catch (err) {
      Utils.clearElement(container);
      Utils.setMessage('#secrets-msg', 'Error de conexion.', 'error');
    }
  }

  function _createSecretCard(secret) {
    const card = Utils.createElement('div', { className: 'card' });

    const nameEl = Utils.createElement('h4', { textContent: Utils.escapeHtml(secret.name) });
    card.appendChild(nameEl);

    const dateEl = Utils.createElement('p', { className: 'card-meta', textContent: 'Creado: ' + Utils.formatDate(secret.createdAt) });
    card.appendChild(dateEl);

    const idEl = Utils.createElement('p', { className: 'card-id', textContent: 'ID: ' + secret.id });
    card.appendChild(idEl);

    const actions = Utils.createElement('div', { className: 'card-actions' });

    const deleteBtn = Utils.createElement('button', {
      className: 'btn btn-danger btn-sm',
      textContent: 'Eliminar',
      dataset: { id: secret.id },
      onClick: () => _handleDeleteSecret(secret.id, secret.name)
    });
    actions.appendChild(deleteBtn);

    card.appendChild(actions);
    return card;
  }

  function _showCreateSecretForm() {
    const container = Utils.$('#create-secret-form-container');
    if (!container) return;
    Utils.clearElement(container);

    const form = Utils.createElement('form', { className: 'inline-form', id: 'create-secret-form' });
    form.appendChild(_createInput('new-secret-name', 'text', 'Nombre del secreto (3-100 chars)', true));
    form.appendChild(_createInput('new-secret-value', 'text', 'Valor del secreto', true));

    const actions = Utils.createElement('div', { className: 'form-actions' });
    const saveBtn = Utils.createElement('button', { type: 'submit', className: 'btn btn-primary btn-sm', textContent: 'Guardar' });
    const cancelBtn = Utils.createElement('button', {
      className: 'btn btn-secondary btn-sm',
      textContent: 'Cancelar',
      onClick: () => Utils.clearElement(container)
    });
    actions.appendChild(saveBtn);
    actions.appendChild(cancelBtn);
    form.appendChild(actions);

    form.addEventListener('submit', _handleCreateSecret);
    container.appendChild(form);
    Utils.$('#new-secret-name')?.focus();
  }

  async function _handleCreateSecret(e) {
    e.preventDefault();
    Utils.clearMessage('#secrets-msg');

    const name = Utils.$('#new-secret-name')?.value?.trim();
    const value = Utils.$('#new-secret-value')?.value;

    if (!name || !value) {
      Utils.setMessage('#secrets-msg', 'Nombre y valor son requeridos.', 'error');
      return;
    }

    if (name.length < 3 || name.length > 100) {
      Utils.setMessage('#secrets-msg', 'El nombre debe tener entre 3 y 100 caracteres.', 'error');
      return;
    }

    const btn = Utils.$('#create-secret-form button[type="submit"]');
    if (btn) { btn.disabled = true; }

    try {
      const res = await API.createSecret(name, value);
      if (res.ok || res.status === 201) {
        Utils.setMessage('#secrets-msg', 'Secreto creado correctamente.', 'success');
        _loadSecrets();
      } else {
        const msg = typeof res.data === 'string' ? res.data : (res.data?.error || 'Error al crear secreto.');
        Utils.setMessage('#secrets-msg', msg, 'error');
      }
    } catch (err) {
      Utils.setMessage('#secrets-msg', 'Error de conexion.', 'error');
    } finally {
      if (btn) { btn.disabled = false; }
    }
  }

  async function _handleDeleteSecret(id, name) {
    if (!confirm('Eliminar el secreto "' + Utils.escapeHtml(name) + '"? Esta accion no se puede deshacer.')) return;

    Utils.clearMessage('#secrets-msg');
    try {
      const res = await API.deleteSecret(id);
      if (res.ok) {
        Utils.setMessage('#secrets-msg', 'Secreto eliminado.', 'success');
        _loadSecrets();
      } else {
        Utils.setMessage('#secrets-msg', 'Error al eliminar.', 'error');
      }
    } catch (err) {
      Utils.setMessage('#secrets-msg', 'Error de conexion.', 'error');
    }
  }

  // ==================== USERS (ADMIN) ====================

  async function _loadUsers() {
    const container = Utils.$('#users-content');
    Utils.clearElement(container);
    container.appendChild(Utils.createElement('p', { textContent: 'Cargando usuarios...', className: 'loading' }));

    try {
      const res = await API.getAllUsers();
      if (!res.ok) {
        Utils.clearElement(container);
        Utils.setMessage('#users-msg', 'Error al cargar usuarios.', 'error');
        return;
      }

      Utils.clearElement(container);
      const users = Array.isArray(res.data) ? res.data : [];

      const header = Utils.createElement('div', { className: 'section-header' });
      header.appendChild(Utils.createElement('h3', { textContent: 'Usuarios (' + users.length + ')' }));
      container.appendChild(header);

      if (users.length === 0) {
        container.appendChild(Utils.createElement('p', { className: 'empty-state', textContent: 'No hay usuarios registrados.' }));
        return;
      }

      const table = Utils.createElement('table', { className: 'data-table' });
      const thead = Utils.createElement('thead');
      const headerRow = Utils.createElement('tr');
      ['Nombre', 'Email', 'ID', 'Acciones'].forEach(h => {
        headerRow.appendChild(Utils.createElement('th', { textContent: h }));
      });
      thead.appendChild(headerRow);
      table.appendChild(thead);

      const tbody = Utils.createElement('tbody');
      users.forEach(user => {
        const row = Utils.createElement('tr');
        row.appendChild(Utils.createElement('td', { textContent: Utils.escapeHtml(user.name) }));
        row.appendChild(Utils.createElement('td', { textContent: Utils.escapeHtml(user.email) }));
        row.appendChild(Utils.createElement('td', { className: 'id-cell', textContent: user.id }));

        const actionsCell = Utils.createElement('td');
        const currentUser = Auth.getUser();
        if (user.id !== currentUser?.id) {
          const delBtn = Utils.createElement('button', {
            className: 'btn btn-danger btn-sm',
            textContent: 'Eliminar',
            onClick: () => _handleDeleteUser(user.id, user.name)
          });
          actionsCell.appendChild(delBtn);
        } else {
          actionsCell.appendChild(Utils.createElement('span', { className: 'badge', textContent: 'Tu' }));
        }
        row.appendChild(actionsCell);
        tbody.appendChild(row);
      });

      table.appendChild(tbody);
      container.appendChild(table);

    } catch (err) {
      Utils.clearElement(container);
      Utils.setMessage('#users-msg', 'Error de conexion.', 'error');
    }
  }

  function _renderAccessDenied() {
    const container = Utils.$('#users-content');
    Utils.clearElement(container);
    container.appendChild(Utils.createElement('div', { className: 'access-denied', textContent: 'Acceso restringido a administradores.' }));
  }

  async function _handleDeleteUser(id, name) {
    if (!confirm('Eliminar al usuario "' + Utils.escapeHtml(name) + '"? Se eliminaran todos sus datos y secretos.')) return;

    Utils.clearMessage('#users-msg');
    try {
      const res = await API.deleteUser(id);
      if (res.ok) {
        Utils.setMessage('#users-msg', 'Usuario eliminado.', 'success');
        _loadUsers();
      } else {
        Utils.setMessage('#users-msg', 'Error al eliminar usuario.', 'error');
      }
    } catch (err) {
      Utils.setMessage('#users-msg', 'Error de conexion.', 'error');
    }
  }

  // ==================== PROFILE ====================

  function _renderProfile() {
    const container = Utils.$('#profile-view');
    Utils.clearElement(container);

    const user = Auth.getUser();
    if (!user) return;

    const card = Utils.createElement('div', { className: 'profile-card' });
    card.appendChild(Utils.createElement('h2', { textContent: 'Mi Perfil' }));

    const info = Utils.createElement('div', { className: 'profile-info' });
    info.appendChild(_createInfoRow('Nombre', user.name));
    info.appendChild(_createInfoRow('Email', user.email));
    info.appendChild(_createInfoRow('Rol', Auth.getRole()));
    card.appendChild(info);

    const hr1 = Utils.createElement('hr');
    card.appendChild(hr1);

    card.appendChild(Utils.createElement('h3', { textContent: 'Cambiar Nombre' }));
    const nameForm = Utils.createElement('form', { id: 'change-name-form', className: 'inline-form' });
    nameForm.appendChild(_createInput('profile-new-name', 'text', 'Nuevo nombre (3-100 chars)', true));
    nameForm.appendChild(Utils.createElement('button', { type: 'submit', className: 'btn btn-primary btn-sm', textContent: 'Actualizar Nombre' }));
    nameForm.addEventListener('submit', _handleChangeName);
    card.appendChild(nameForm);

    const hr2 = Utils.createElement('hr');
    card.appendChild(hr2);

    card.appendChild(Utils.createElement('h3', { textContent: 'Cambiar Contrasena' }));
    const pwForm = Utils.createElement('form', { id: 'change-pw-form', className: 'inline-form' });
    pwForm.appendChild(_createInput('profile-new-pw', 'password', 'Nueva contrasena', true));
    pwForm.appendChild(Utils.createElement('button', { type: 'submit', className: 'btn btn-primary btn-sm', textContent: 'Actualizar Contrasena' }));
    pwForm.addEventListener('submit', _handleChangePassword);
    card.appendChild(pwForm);

    const hint = Utils.createElement('p', { className: 'hint', textContent: 'Min 8 caracteres, 1 mayuscula, 1 minuscula, 1 digito, 1 especial.' });
    card.appendChild(hint);

    const msgContainer = Utils.createElement('div', { id: 'profile-msg' });
    card.appendChild(msgContainer);

    container.appendChild(card);
  }

  function _createInfoRow(label, value) {
    const row = Utils.createElement('div', { className: 'info-row' });
    row.appendChild(Utils.createElement('span', { className: 'info-label', textContent: label + ':' }));
    row.appendChild(Utils.createElement('span', { className: 'info-value', textContent: Utils.escapeHtml(value) }));
    return row;
  }

  async function _handleChangeName(e) {
    e.preventDefault();
    Utils.clearMessage('#profile-msg');

    const newName = Utils.$('#profile-new-name')?.value?.trim();
    if (!newName || newName.length < 3 || newName.length > 100) {
      Utils.setMessage('#profile-msg', 'El nombre debe tener entre 3 y 100 caracteres.', 'error');
      return;
    }

    try {
      const res = await API.changeName(newName);
      if (res.ok) {
        const user = Auth.getUser();
        if (user) user.name = newName;
        _updateUserInfo();
        Utils.setMessage('#profile-msg', 'Nombre actualizado.', 'success');
        Utils.$('#profile-new-name').value = '';
      } else {
        const msg = typeof res.data === 'string' ? res.data : (res.data?.error || 'Error al cambiar nombre.');
        Utils.setMessage('#profile-msg', msg, 'error');
      }
    } catch (err) {
      Utils.setMessage('#profile-msg', 'Error de conexion.', 'error');
    }
  }

  async function _handleChangePassword(e) {
    e.preventDefault();
    Utils.clearMessage('#profile-msg');

    const newPw = Utils.$('#profile-new-pw')?.value;
    if (!newPw) {
      Utils.setMessage('#profile-msg', 'Ingresa la nueva contrasena.', 'error');
      return;
    }

    if (!Utils.validatePassword(newPw)) {
      Utils.setMessage('#profile-msg', 'La contrasena no cumple los requisitos de seguridad.', 'error');
      return;
    }

    try {
      const res = await API.changePassword(newPw);
      if (res.ok) {
        Utils.setMessage('#profile-msg', 'Contrasena actualizada.', 'success');
        Utils.$('#profile-new-pw').value = '';
      } else {
        const msg = typeof res.data === 'string' ? res.data : (res.data?.error || 'Error al cambiar contrasena.');
        Utils.setMessage('#profile-msg', msg, 'error');
      }
    } catch (err) {
      Utils.setMessage('#profile-msg', 'Error de conexion.', 'error');
    }
  }

  // ==================== HELPERS ====================

  function _createInput(id, type, placeholder, required) {
    return Utils.createElement('div', { className: 'form-group' }, [
      Utils.createElement('input', {
        id: id,
        type: type,
        placeholder: placeholder,
        className: 'form-input'
      })
    ]);
  }

  return {
    init,
    showView
  };

})();

document.addEventListener('DOMContentLoaded', App.init);
