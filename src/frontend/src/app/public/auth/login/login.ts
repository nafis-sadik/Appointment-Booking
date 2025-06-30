import { Component } from '@angular/core';
import { UserService } from '../../../services/user-service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { UserModel } from '../../../models/user.model';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  loginForm: FormGroup;
  userModel: UserModel = new UserModel();
  
  constructor(
    private fb: FormBuilder, 
    private userService: UserService,
    private toastr: ToastrService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', Validators.required],
      password: ['', Validators.required]
    });

    if(localStorage.getItem('token')){
      this.router.navigate(['/dashboard']);
      return;
    }
  }

  ngOnInit(): void {
    // Listen for changes
    this.loginForm.valueChanges.subscribe(form => {
      this.userModel.email = form.email;
      this.userModel.password = form.password;
    });
  }

  login(): void {
    if (this.loginForm.invalid) {
      console.log('invalid');
      return;
    }

    this.userService
      .login(this.userModel)
      .subscribe({
        next: (res) => {
          this.toastr.success('Login success');
          localStorage.setItem('token', res.response);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.log(err.error);
          this.toastr.error(err.error, 'Login failed');
        }
      });
  }
}
