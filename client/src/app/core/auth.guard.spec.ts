import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, UrlTree, provideRouter } from '@angular/router';
import { routes } from '../app.routes';
import { AuthService } from './auth.service';
import { authGuard, guestGuard } from './auth.guard';

describe('auth guards', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  function setup(): { auth: AuthService; router: Router } {
    TestBed.configureTestingModule({
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    });
    return { auth: TestBed.inject(AuthService), router: TestBed.inject(Router) };
  }

  it('sends anonymous people to sign in and lets them open account pages', () => {
    const { router } = setup();

    const tasks = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    const login = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));

    expect(tasks).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(tasks as UrlTree)).toBe('/login');
    expect(login).toBe(true);
  });

  it('lets a signed-in person open tasks and skips the account pages', () => {
    sessionStorage.setItem(
      'task-manager.session',
      JSON.stringify({ token: 'abc', id: '1', email: 'ada@example.com' }),
    );
    const { router } = setup();

    const tasks = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));
    const login = TestBed.runInInjectionContext(() => guestGuard({} as never, {} as never));

    expect(tasks).toBe(true);
    expect(login).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(login as UrlTree)).toBe('/tasks');
  });

  it('attaches the guards to the account and task routes', () => {
    const taskRoute = routes.find((route) => route.path === 'tasks');
    const loginRoute = routes.find((route) => route.path === 'login');
    const registerRoute = routes.find((route) => route.path === 'register');

    expect(taskRoute?.canActivate).toEqual([authGuard]);
    expect(loginRoute?.canActivate).toEqual([guestGuard]);
    expect(registerRoute?.canActivate).toEqual([guestGuard]);
  });
});
