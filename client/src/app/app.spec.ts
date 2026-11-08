import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { routes } from './app.routes';
import { AuthService } from './core/auth.service';

describe('App', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  async function create() {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    return fixture;
  }

  it('offers sign in when nobody is signed in', async () => {
    const fixture = await create();

    expect(fixture.nativeElement.querySelector('.topbar .brand')?.textContent).toContain('Task Manager');
    expect(fixture.nativeElement.textContent).toContain('Sign in');
    expect(fixture.nativeElement.textContent).not.toContain('Sign out');
  });

  it('signs out the current person', async () => {
    sessionStorage.setItem(
      'task-manager.session',
      JSON.stringify({ token: 'abc', id: '1', email: 'ada@example.com' }),
    );
    const fixture = await create();

    expect(fixture.nativeElement.textContent).toContain('ada@example.com');
    fixture.nativeElement.querySelector('button')?.click();
    await fixture.whenStable();

    expect(TestBed.inject(AuthService).isLoggedIn()).toBe(false);
  });
});
