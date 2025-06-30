import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SiteRoutingModule } from './site-routing-module';
import { Home } from './home/home';
import { Site } from './site';


@NgModule({
  declarations: [
    Home,
    Site
  ],
  imports: [
    CommonModule,
    SiteRoutingModule
  ]
})
export class SiteModule { }
