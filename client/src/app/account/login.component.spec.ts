import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../app.routes';
import { AuthService } from '../core/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  const auth = {
    login: vi.fn(),
  };

  beforeEach(async () => {
    auth.login.mockReset();
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideRouter(routes), { provide: AuthService, useValue: auth }],
    }).compileComponents();
    fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
  });

  it('shows the sign-in card and the registration link', () => {
    expect(fixture.nativeElement.querySelector('.card h2')?.textContent).toContain('Sign in');
    expect(fixture.nativeElement.querySelector('a[href="/register"]')?.textContent).toContain('Create an account');
  });

  it('does not sign in until the form is valid', () => {
    fixture.componentInstance.submit();

    expect(auth.login).not.toHaveBeenCalled();
  });

  it('signs in and opens the task list', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    auth.login.mockReturnValue(of({ token: 'abc', id: '1', email: 'ada@example.com' }));
    fixture.componentInstance.form.setValue({ email: 'ada@example.com', password: 'Secret123' });

    fixture.componentInstance.submit();

    expect(auth.login).toHaveBeenCalledWith('ada@example.com', 'Secret123');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/tasks');
  });

  it('shows the API error when the credentials are rejected', async () => {
    auth.login.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 401, error: { message: 'Invalid email or password.' } })),
    );
    fixture.componentInstance.form.setValue({ email: 'ada@example.com', password: 'Secret123' });

    fixture.componentInstance.submit();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'Invalid email or password.',
    );
  });
});
