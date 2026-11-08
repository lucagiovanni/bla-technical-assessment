import { HttpErrorResponse } from '@angular/common/http';

export function messageFrom(error: HttpErrorResponse, fallback: string): string {
  const message = error.error?.message;
  return typeof message === 'string' && message.length > 0 ? message : fallback;
}
