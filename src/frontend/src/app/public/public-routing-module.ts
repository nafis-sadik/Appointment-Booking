import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Public } from './public';

const routes: Routes = [  
  {
    path: '',
    component: Public,
    children: [
      {
        path: '',
        loadChildren: () => import('./site/site-module').then(m => m.SiteModule),
      },
      {
        path: 'auth',
        loadChildren: () => import('./auth/auth-module').then(m => m.AuthModule),
      }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PublicRoutingModule { }
