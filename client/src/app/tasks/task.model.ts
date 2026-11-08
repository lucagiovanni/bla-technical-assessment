export type TaskStatus = 'Pending' | 'InProgress' | 'Completed';

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export interface TaskDraft {
  title: string;
  description: string;
  status: TaskStatus;
  dueDate: string;
}

export function statusLabel(status: TaskStatus): string {
  switch (status) {
    case 'Pending':
      return 'Pending';
    case 'InProgress':
      return 'In progress';
    case 'Completed':
      return 'Completed';
  }
}
