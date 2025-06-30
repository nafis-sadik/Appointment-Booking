import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Dashboard } from './dashboard';
import { AuthGuard } from '../shared/auth-guard';
import { Home } from './home/home';

const routes: Routes = [  
  {
    path: '',
    canActivate: [AuthGuard], // here we tell Angular to check the access with our AuthGuard
    component: Dashboard,
    children: [
      {
        path: '',
        component: Home
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
