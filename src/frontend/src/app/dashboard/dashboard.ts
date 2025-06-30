import { Component } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
  animations: [
    trigger('sidebarAnimation', [
      state('open', style({ width: '250px' })),
      state('closed', style({ width: '0px', overflow: 'hidden' })),
      transition('open <=> closed', [animate('300ms ease-in-out')]),
    ])
  ]
})
export class Dashboard {
  sidebarOpen = false;

  /**
   *
   */
  constructor(private router: Router) {
    
  }

  toggleSidebar() {
    this.sidebarOpen = !this.sidebarOpen;
  }

  logout() : void {
    localStorage.clear();
    this.router.navigate(['/']);
  }
}
