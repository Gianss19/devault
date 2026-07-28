window.App = window.App || { showView: function () { } };

function createTestJwt(payload) {
  var header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
  var body = btoa(JSON.stringify(payload));
  return header + '.' + body + '.fake-sig';
}

var TEST_CLAIMS = {
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': 'Test User',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': 'test@test.com',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role': 'Admin'
};

describe('Auth logout', function () {
  it('logout limpia user y role', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    Auth.logout();
    expect(Auth.getUser()).toBeNull();
    expect(Auth.getRole()).toBeNull();
  });
  it('isAuthenticated retorna false despues de logout', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    Auth.logout();
    expect(Auth.isAuthenticated()).toBeFalsy();
  });
  it('getRole retorna null despues de logout', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    Auth.logout();
    expect(Auth.getRole()).toBeNull();
  });
  it('getUser retorna null despues de logout', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    Auth.logout();
    expect(Auth.getUser()).toBeNull();
  });
});

describe('Auth setSession', function () {
  it('popula user y role con JWT Admin', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    var user = Auth.getUser();
    expect(user).toBeTruthy();
    expect(user.name).toBe('Test User');
    expect(user.email).toBe('test@test.com');
    expect(user.id).toBe('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee');
    Auth.logout();
  });
  it('isAuthenticated retorna true despues de setSession', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.isAuthenticated()).toBeTruthy();
    Auth.logout();
  });
  it('isAdmin retorna true cuando role es Admin', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.isAdmin()).toBeTruthy();
    Auth.logout();
  });
  it('isAdmin retorna false cuando role es User', function () {
    var payload = Object.assign({}, TEST_CLAIMS);
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] = 'User';
    Auth.setSession(createTestJwt(payload), 'refresh');
    expect(Auth.isAdmin()).toBeFalsy();
    Auth.logout();
  });
  it('getRole retorna Admin', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.getRole()).toBe('Admin');
    Auth.logout();
  });
  it('getRole retorna User', function () {
    var payload = Object.assign({}, TEST_CLAIMS);
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] = 'User';
    Auth.setSession(createTestJwt(payload), 'refresh');
    expect(Auth.getRole()).toBe('User');
    Auth.logout();
  });
  it('extrae nameidentifier correctamente', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.getUser().id).toBe('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee');
    Auth.logout();
  });
  it('extrae name correctamente', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.getUser().name).toBe('Test User');
    Auth.logout();
  });
  it('extrae email correctamente', function () {
    Auth.setSession(createTestJwt(TEST_CLAIMS), 'refresh');
    expect(Auth.getUser().email).toBe('test@test.com');
    Auth.logout();
  });
  it('payload invalido no crashea', function () {
    Auth.setSession('not.a.jwt', 'refresh');
    expect(Auth.getUser()).toBeNull();
    Auth.logout();
  });
});
