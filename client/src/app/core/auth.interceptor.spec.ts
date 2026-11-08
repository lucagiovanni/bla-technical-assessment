import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from './auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let auth: AuthService;
  let backend: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    auth = TestBed.inject(AuthService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    backend.verify();
    sessionStorage.clear();
  });

  it('adds the bearer token to API requests after login', () => {
    auth.login('ada@example.com', 'Secret123').subscribe();
    const login = backend.expectOne('/api/users/login');
    expect(login.request.headers.has('Authorization')).toBe(false);
    login.flush({ token: 'abc', id: '1', email: 'ada@example.com' });

    http.get('/api/users/me').subscribe();
    const me = backend.expectOne('/api/users/me');
    expect(me.request.headers.get('Authorization')).toBe('Bearer abc');
    me.flush({ id: '1', email: 'ada@example.com' });
  });

  it('leaves requests alone when nobody is signed in', () => {
    http.get('/api/users/me').subscribe({ error: () => undefined });
    const me = backend.expectOne('/api/users/me');
    expect(me.request.headers.has('Authorization')).toBe(false);
    me.flush({}, { status: 401, statusText: 'Unauthorized' });
  });
});
