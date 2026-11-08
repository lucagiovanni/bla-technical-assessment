import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE } from '../core/api-base';
import { TaskDraft, TaskItem } from './task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);

  list(): Observable<TaskItem[]> {
    return this.http.get<TaskItem[]>(`${API_BASE}/tasks`);
  }

  create(draft: TaskDraft): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${API_BASE}/tasks`, draft);
  }

  update(id: string, draft: TaskDraft): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${API_BASE}/tasks/${id}`, draft);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${API_BASE}/tasks/${id}`);
  }
}
