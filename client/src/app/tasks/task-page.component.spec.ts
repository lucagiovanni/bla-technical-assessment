import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { layoutForWidth } from '../layout';
import { TaskItem } from './task.model';
import { TaskPageComponent } from './task-page.component';
import { TaskService } from './task.service';

describe('TaskPageComponent', () => {
  let fixture: ComponentFixture<TaskPageComponent>;
  const api = {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  };
  const existing: TaskItem = {
    id: '1',
    title: 'Write the brief',
    description: 'Demo notes',
    status: 'Pending',
    dueDate: '2026-11-09',
  };

  beforeEach(async () => {
    api.list.mockReset();
    api.create.mockReset();
    api.update.mockReset();
    api.delete.mockReset();
    api.list.mockReturnValue(of([existing]));
    await TestBed.configureTestingModule({
      imports: [TaskPageComponent],
      providers: [{ provide: TaskService, useValue: api }],
    }).compileComponents();
    fixture = TestBed.createComponent(TaskPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => {
    fixture.destroy();
    vi.restoreAllMocks();
  });

  it('marks the list for the current viewport', () => {
    const section = fixture.nativeElement.querySelector('.tasks') as HTMLElement;
    expect(section.classList.contains(`is-${layoutForWidth(window.innerWidth)}`)).toBe(true);
  });

  it('switches between mobile cards and the desktop table', () => {
    const width = vi.spyOn(window, 'innerWidth', 'get').mockReturnValue(390);
    window.dispatchEvent(new Event('resize'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.tasks')?.classList.contains('is-mobile')).toBe(true);

    width.mockReturnValue(1024);
    window.dispatchEvent(new Event('resize'));
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.tasks')?.classList.contains('is-desktop')).toBe(true);
    width.mockRestore();
  });

  it('shows the tasks loaded from the service', () => {
    expect(api.list).toHaveBeenCalled();
    expect(fixture.nativeElement.textContent).toContain('Write the brief');
    expect(fixture.nativeElement.textContent).toContain('Demo notes');
    expect(fixture.nativeElement.textContent).toContain('2026-11-09');
  });

  it('shows an empty state', async () => {
    api.list.mockReturnValue(of([]));
    fixture.destroy();
    fixture = TestBed.createComponent(TaskPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('No tasks yet.');
  });

  it('creates a task from the form', () => {
    api.create.mockReturnValue(of({ ...existing, id: '2', title: 'New task' }));
    fixture.componentInstance.form.setValue({
      title: 'New task',
      description: 'Notes',
      status: 'InProgress',
      dueDate: '2026-12-01',
    });

    fixture.componentInstance.submit();

    expect(api.create).toHaveBeenCalledWith({
      title: 'New task',
      description: 'Notes',
      status: 'InProgress',
      dueDate: '2026-12-01',
    });
    expect(api.list).toHaveBeenCalledTimes(2);
  });

  it('does not save an invalid task', () => {
    fixture.componentInstance.form.setValue({
      title: '',
      description: '',
      status: 'Pending',
      dueDate: '',
    });

    fixture.componentInstance.submit();

    expect(api.create).not.toHaveBeenCalled();
  });

  it('updates the task being edited', () => {
    api.update.mockReturnValue(of({ ...existing, title: 'Updated' }));
    fixture.componentInstance.edit(existing);
    fixture.componentInstance.form.controls.title.setValue('Updated');

    fixture.componentInstance.submit();

    expect(api.update).toHaveBeenCalledWith('1', {
      title: 'Updated',
      description: 'Demo notes',
      status: 'Pending',
      dueDate: '2026-11-09',
    });
  });

  it('deletes a task after confirmation', () => {
    api.delete.mockReturnValue(of(undefined));

    fixture.componentInstance.requestDelete('1');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Confirm delete');

    fixture.componentInstance.confirmDelete('1');

    expect(api.delete).toHaveBeenCalledWith('1');
    expect(api.list).toHaveBeenCalledTimes(2);
  });

  it('shows an API error', async () => {
    api.list.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500, error: { message: 'Tasks could not be loaded.' } })),
    );
    fixture.destroy();
    fixture = TestBed.createComponent(TaskPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain('Tasks could not be loaded.');
  });
});