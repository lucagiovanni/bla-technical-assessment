import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { routes } from '../app.routes';
import { AuthService } from '../core/auth.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let fixture: ComponentFixture<RegisterComponent>;
  const auth = {
    register: vi.fn(),
    login: vi.fn(),
  };

  beforeEach(async () => {
    auth.register.mockReset();
    auth.login.mockReset();
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [provideRouter(routes), { provide: AuthService, useValue: auth }],
    }).compileComponents();
    fixture = TestBed.createComponent(RegisterComponent);
    await fixture.whenStable();
  });

  it('shows the registration card and the sign-in link', () => {
    expect(fixture.nativeElement.querySelector('.card h2')?.textContent).toContain('Create an account');
    expect(fixture.nativeElement.querySelector('a[href="/login"]')?.textContent).toContain('Sign in');
    expect(fixture.nativeElement.textContent).toContain('At least 8 characters, with a letter and a digit.');
  });

  it('does not register until the form is valid', () => {
    fixture.componentInstance.form.setValue({ email: 'ada@example.com', password: 'short' });

    fixture.componentInstance.submit();

    expect(auth.register).not.toHaveBeenCalled();
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('registers, signs in, and opens the task list', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);
    auth.register.mockReturnValue(of({ id: '1', email: 'ada@example.com' }));
    auth.login.mockReturnValue(of({ token: 'abc', id: '1', email: 'ada@example.com' }));
    fixture.componentInstance.form.setValue({ email: 'ada@example.com', password: 'Secret123' });

    fixture.componentInstance.submit();

    expect(auth.register).toHaveBeenCalledWith('ada@example.com', 'Secret123');
    expect(auth.login).toHaveBeenCalledWith('ada@example.com', 'Secret123');
    expect(router.navigateByUrl).toHaveBeenCalledWith('/tasks');
  });

  it('shows a duplicate-account error and does not sign in', async () => {
    auth.register.mockReturnValue(
      throwError(
        () => new HttpErrorResponse({ status: 409, error: { message: 'An account with that email already exists.' } }),
      ),
    );
    fixture.componentInstance.form.setValue({ email: 'ada@example.com', password: 'Secret123' });

    fixture.componentInstance.submit();
    await fixture.whenStable();

    expect(auth.login).not.toHaveBeenCalled();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'An account with that email already exists.',
    );
  });
});
