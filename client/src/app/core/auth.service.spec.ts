import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.clear();
  });

  it('registers without storing a session', () => {
    let responseEmail = '';
    auth.register('ada@example.com', 'Secret123').subscribe((user) => {
      responseEmail = user.email;
    });

    const request = http.expectOne('/api/users/register');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ email: 'ada@example.com', password: 'Secret123' });
    request.flush({ id: '1', email: 'ada@example.com' });

    expect(responseEmail).toBe('ada@example.com');
    expect(auth.isLoggedIn()).toBe(false);
  });

  it('stores a session on login and clears it on logout', () => {
    let signedIn = false;
    auth.login('Ada@Example.com', 'Secret123').subscribe(() => {
      signedIn = true;
    });

    const request = http.expectOne('/api/users/login');
    expect(request.request.body).toEqual({ email: 'Ada@Example.com', password: 'Secret123' });
    request.flush({ token: 'abc', id: '1', email: 'ada@example.com' });

    expect(signedIn).toBe(true);
    expect(auth.isLoggedIn()).toBe(true);
    expect(auth.token()).toBe('abc');
    expect(auth.user()).toEqual({ token: 'abc', id: '1', email: 'ada@example.com' });
    expect(sessionStorage.getItem('task-manager.session')).toContain('abc');

    auth.logout();
    expect(auth.isLoggedIn()).toBe(false);
    expect(auth.token()).toBeNull();
    expect(sessionStorage.getItem('task-manager.session')).toBeNull();
  });

  it('restores a valid session and ignores a broken one', () => {
    sessionStorage.setItem(
      'task-manager.session',
      JSON.stringify({ token: 'saved', id: '9', email: 'ada@example.com' }),
    );
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    const restored = TestBed.inject(AuthService);
    expect(restored.isLoggedIn()).toBe(true);
    expect(restored.user()?.email).toBe('ada@example.com');

    sessionStorage.setItem('task-manager.session', '{');
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(false);
  });
});
