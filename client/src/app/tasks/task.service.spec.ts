import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TaskDraft } from './task.model';
import { TaskService } from './task.service';

describe('TaskService', () => {
  let tasks: TaskService;
  let http: HttpTestingController;
  const draft: TaskDraft = {
    title: 'Write the brief',
    description: 'Demo notes',
    status: 'Pending',
    dueDate: '2026-11-09',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    tasks = TestBed.inject(TaskService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('lists tasks', () => {
    let received: unknown;
    tasks.list().subscribe((body) => {
      received = body;
    });

    const request = http.expectOne('/api/tasks');
    expect(request.request.method).toBe('GET');
    request.flush([{ id: '1', ...draft }]);
    expect(received).toEqual([{ id: '1', ...draft }]);
  });

  it('creates a task', () => {
    tasks.create(draft).subscribe();

    const request = http.expectOne('/api/tasks');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(draft);
    request.flush({ id: '1', ...draft });
  });

  it('updates a task', () => {
    const updated = { ...draft, title: 'Write the notes', status: 'Completed' as const };
    tasks.update('1', updated).subscribe();

    const request = http.expectOne('/api/tasks/1');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(updated);
    request.flush({ id: '1', ...updated });
  });

  it('deletes a task', () => {
    tasks.delete('1').subscribe();

    const request = http.expectOne('/api/tasks/1');
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });
});
