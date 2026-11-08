import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { messageFrom } from '../core/http-error';
import { layoutForWidth } from '../layout';
import { TaskItem, TaskStatus, statusLabel } from './task.model';
import { TaskService } from './task.service';

@Component({
  selector: 'app-task-page',
  imports: [ReactiveFormsModule],
  templateUrl: './task-page.component.html',
  styleUrl: './task-page.component.css',
})
export class TaskPageComponent implements OnInit {
  private readonly tasksApi = inject(TaskService);
  private readonly destroyRef = inject(DestroyRef);

  readonly tasks = signal<TaskItem[]>([]);
  readonly editingId = signal<string | null>(null);
  readonly pendingDeleteId = signal<string | null>(null);
  readonly error = signal('');
  readonly layout = signal(layoutForWidth(window.innerWidth));
  readonly statusLabel = statusLabel;

  constructor() {
    const onResize = () => this.layout.set(layoutForWidth(window.innerWidth));
    window.addEventListener('resize', onResize);
    this.destroyRef.onDestroy(() => window.removeEventListener('resize', onResize));
  }
  readonly form = new FormGroup({
    title: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(4000)] }),
    status: new FormControl<TaskStatus>('Pending', { nonNullable: true, validators: [Validators.required] }),
    dueDate: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });

  ngOnInit(): void {
    this.reload();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const draft = this.form.getRawValue();
    const editingId = this.editingId();
    const request = editingId ? this.tasksApi.update(editingId, draft) : this.tasksApi.create(draft);
    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.error.set('');
        this.resetForm();
        this.reload();
      },
      error: (error: HttpErrorResponse) => this.error.set(messageFrom(error, 'The task could not be saved.')),
    });
  }

  edit(task: TaskItem): void {
    this.editingId.set(task.id);
    this.pendingDeleteId.set(null);
    this.form.setValue({
      title: task.title,
      description: task.description,
      status: task.status,
      dueDate: task.dueDate,
    });
  }

  cancelEdit(): void {
    this.resetForm();
  }

  requestDelete(id: string): void {
    this.pendingDeleteId.set(id);
  }

  cancelDelete(): void {
    this.pendingDeleteId.set(null);
  }

  confirmDelete(id: string): void {
    this.tasksApi
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          if (this.editingId() === id) {
            this.resetForm();
          }
          this.pendingDeleteId.set(null);
          this.error.set('');
          this.reload();
        },
        error: (error: HttpErrorResponse) => this.error.set(messageFrom(error, 'The task could not be deleted.')),
      });
  }

  private reload(): void {
    this.tasksApi
      .list()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (tasks) => this.tasks.set(tasks),
        error: (error: HttpErrorResponse) => this.error.set(messageFrom(error, 'Tasks could not be loaded.')),
      });
  }

  private resetForm(): void {
    this.editingId.set(null);
    this.form.reset({ title: '', description: '', status: 'Pending', dueDate: '' });
  }
}
