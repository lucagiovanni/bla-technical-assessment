import { Routes } from '@angular/router';
import { LoginComponent } from './account/login.component';
import { RegisterComponent } from './account/register.component';
import { authGuard, guestGuard } from './core/auth.guard';
import { TaskPageComponent } from './tasks/task-page.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'tasks' },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'tasks', component: TaskPageComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'tasks' },
];
