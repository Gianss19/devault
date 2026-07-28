const API = (() => {

  const BASE_URL = (location.hostname === 'localhost' || location.hostname === '127.0.0.1')
    ? 'http://localhost:5164'
    : '';

  let _accessToken = null;
  let _refreshToken = null;

  function setTokens(access, refresh) {
    _accessToken = access || null;
    _refreshToken = refresh || null;
  }

  function getAccessToken() {
    return _accessToken;
  }

  function getRefreshToken() {
    return _refreshToken;
  }

  function clearTokens() {
    _accessToken = null;
    _refreshToken = null;
  }

  async function request(method, path, body, requiresAuth) {
    const url = BASE_URL + path;
    const headers = { 'Content-Type': 'application/json' };

    if (requiresAuth && _accessToken) {
      headers['Authorization'] = 'Bearer ' + _accessToken;
    }

    const config = { method, headers };
    if (body !== undefined && body !== null) {
      config.body = JSON.stringify(body);
    }

    const response = await fetch(url, config);
    const status = response.status;

    if (status === 204) {
      return { ok: true, status, data: null };
    }

    let data = null;
    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
      try {
        data = await response.json();
      } catch {
        data = null;
      }
    } else {
      const text = await response.text();
      if (text) data = text;
    }

    if (status === 401) {
      clearTokens();
      if (typeof Auth !== 'undefined' && Auth.onSessionExpired) {
        Auth.onSessionExpired();
      }
    }

    return { ok: response.ok, status, data };
  }

  function signup(name, email, password) {
    return request('POST', '/api/Auth/signup', { name, email, password }, false);
  }

  function login(email, password) {
    return request('POST', '/api/Auth/login', { email, password }, false);
  }

  function logout() {
    return request('POST', '/api/Auth/logout', { refreshToken: _refreshToken }, true);
  }

  function createSecret(name, value) {
    return request('POST', '/api/Secrets/create', { name, value }, true);
  }

  function getAllSecrets() {
    return request('GET', '/api/Secrets/all', null, true);
  }

  function getSecretById(id) {
    return request('GET', '/api/Secrets/' + encodeURIComponent(id), null, true);
  }

  function revealSecret(id) {
    return request('GET', '/api/Secrets/' + encodeURIComponent(id) + '/reveal', null, true);
  }

  function updateSecret(id, name, value) {
    const body = {};
    if (name !== undefined && name !== null) body.name = name;
    if (value !== undefined && value !== null) body.value = value;
    return request('PUT', '/api/Secrets/' + encodeURIComponent(id), body, true);
  }

  function deleteSecret(id) {
    return request('DELETE', '/api/Secrets/' + encodeURIComponent(id), null, true);
  }

  function getAllUsers() {
    return request('GET', '/api/Users/all', null, true);
  }

  function getProfile() {
    return request('GET', '/api/Users/me', null, true);
  }

  function changeName(newName) {
    return request('POST', '/api/Users/change-name', { newName }, true);
  }

  function changePassword(newPassword) {
    return request('POST', '/api/Users/change-password', { newPassword }, true);
  }

  function deleteUser(id) {
    return request('DELETE', '/api/Users/' + encodeURIComponent(id), null, true);
  }

  function healthCheck() {
    return request('GET', '/health', null, false);
  }

  return {
    setTokens,
    getAccessToken,
    getRefreshToken,
    clearTokens,
    signup,
    login,
    logout,
    createSecret,
    getAllSecrets,
    getSecretById,
    revealSecret,
    updateSecret,
    deleteSecret,
    getAllUsers,
    getProfile,
    changeName,
    changePassword,
    deleteUser,
    healthCheck
  };

})();
