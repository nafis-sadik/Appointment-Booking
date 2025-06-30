import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  standalone: false,
  templateUrl: './signup.html',
  styleUrl: './signup.scss'
})
export class Signup {
  constructor(router: Router) {
    if(localStorage.getItem('token')){
      router.navigate(['/dashboard']);
      return;
    }
  }
}