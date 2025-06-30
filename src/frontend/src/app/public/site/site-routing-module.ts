import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { Home } from './home/home';
import { Site } from './site';

const routes: Routes = [
  {
    path: '',
    component: Site,
    children: [
      {
        path: '',
        component: Home
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class SiteRoutingModule { }
