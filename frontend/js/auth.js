const Auth = (() => {

  let _user = null;
  let _role = null;

  function parseJwtPayload(token) {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch {
      return null;
    }
  }

  function setSession(accessToken, refreshToken) {
    API.setTokens(accessToken, refreshToken);
    const payload = parseJwtPayload(accessToken);
    if (payload) {
      _user = {
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.sub || null,
        name: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || ''
      };
      _role = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] || 'User';
    }
  }

  function getUser() {
    return _user;
  }

  function getRole() {
    return _role;
  }

  function isAdmin() {
    return _role === 'Admin';
  }

  function isAuthenticated() {
    return !!API.getAccessToken();
  }

  function logout() {
    _user = null;
    _role = null;
    API.clearTokens();
  }

  function onSessionExpired() {
    logout();
    App.showView('login');
    Utils.setMessage('#global-message', 'Sesion expirada. Por favor, inicia sesion de nuevo.', 'error');
  }

  return {
    setSession,
    getUser,
    getRole,
    isAdmin,
    isAuthenticated,
    logout,
    onSessionExpired
  };

})();
