describe('API token management', function () {
  it('setTokens almacena tokens', function () {
    API.setTokens('access-123', 'refresh-456');
    expect(API.getAccessToken()).toBe('access-123');
    expect(API.getRefreshToken()).toBe('refresh-456');
  });
  it('getAccessToken retorna el token almacenado', function () {
    API.setTokens('my-token', 'my-refresh');
    expect(API.getAccessToken()).toBe('my-token');
  });
  it('getRefreshToken retorna el refresh token almacenado', function () {
    API.setTokens('a', 'b');
    expect(API.getRefreshToken()).toBe('b');
  });
  it('clearTokens pone ambos tokens en null', function () {
    API.setTokens('x', 'y');
    API.clearTokens();
    expect(API.getAccessToken()).toBeNull();
    expect(API.getRefreshToken()).toBeNull();
  });
  it('setTokens con null por defecto usa null', function () {
    API.setTokens(null, null);
    expect(API.getAccessToken()).toBeNull();
    expect(API.getRefreshToken()).toBeNull();
  });
  it('setTokens sin argumentos usa null', function () {
    API.setTokens();
    expect(API.getAccessToken()).toBeNull();
    expect(API.getRefreshToken()).toBeNull();
  });
});

describe('API funciones existen', function () {
  it('signup es una funcion', function () { expect(typeof API.signup).toBe('function'); });
  it('login es una funcion', function () { expect(typeof API.login).toBe('function'); });
  it('logout es una funcion', function () { expect(typeof API.logout).toBe('function'); });
  it('createSecret es una funcion', function () { expect(typeof API.createSecret).toBe('function'); });
  it('getAllSecrets es una funcion', function () { expect(typeof API.getAllSecrets).toBe('function'); });
  it('getSecretById es una funcion', function () { expect(typeof API.getSecretById).toBe('function'); });
  it('deleteSecret es una funcion', function () { expect(typeof API.deleteSecret).toBe('function'); });
  it('getAllUsers es una funcion', function () { expect(typeof API.getAllUsers).toBe('function'); });
  it('changeName es una funcion', function () { expect(typeof API.changeName).toBe('function'); });
  it('changePassword es una funcion', function () { expect(typeof API.changePassword).toBe('function'); });
  it('deleteUser es una funcion', function () { expect(typeof API.deleteUser).toBe('function'); });
  it('healthCheck es una funcion', function () { expect(typeof API.healthCheck).toBe('function'); });
});
