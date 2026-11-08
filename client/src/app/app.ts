import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { API_BASE } from './core/api-base';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  protected readonly apiBase = API_BASE;
}
