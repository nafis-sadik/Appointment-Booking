import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AuthRoutingModule } from './auth-routing-module';
import { Auth } from './auth';
import { Login } from './login/login';
import { Signup } from './signup/signup';
import { ReactiveFormsModule } from '@angular/forms';


@NgModule({
  declarations: [
    Auth,
    Login,
    Signup
  ],
  imports: [
    ReactiveFormsModule,
    CommonModule,
    AuthRoutingModule
  ]
})
export class AuthModule { }
