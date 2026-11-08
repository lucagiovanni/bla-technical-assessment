import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE } from './api-base';

export interface UserProfile {
  id: string;
  email: string;
}

export interface Session extends UserProfile {
  token: string;
}

const sessionKey = 'task-manager.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly session = signal<Session | null>(readSession());

  readonly user = computed(() => this.session());

  isLoggedIn(): boolean {
    return this.session() !== null;
  }

  token(): string | null {
    return this.session()?.token ?? null;
  }

  register(email: string, password: string): Observable<UserProfile> {
    return this.http.post<UserProfile>(`${API_BASE}/users/register`, { email, password });
  }

  login(email: string, password: string): Observable<Session> {
    return this.http.post<Session>(`${API_BASE}/users/login`, { email, password }).pipe(
      tap((session) => this.store(session)),
    );
  }

  logout(): void {
    sessionStorage.removeItem(sessionKey);
    this.session.set(null);
  }

  private store(session: Session): void {
    sessionStorage.setItem(sessionKey, JSON.stringify(session));
    this.session.set(session);
  }
}

function readSession(): Session | null {
  const raw = sessionStorage.getItem(sessionKey);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as Partial<Session>;
    if (!parsed.token || !parsed.id || !parsed.email) {
      return null;
    }

    return { token: parsed.token, id: parsed.id, email: parsed.email };
  } catch {
    return null;
  }
}
