import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { PublicRoutingModule } from './public-routing-module';
import { Public } from './public';
import { NotFound } from './not-found/not-found';


@NgModule({
  declarations: [
    Public,
    NotFound,
  ],
  imports: [
    CommonModule,
    PublicRoutingModule
  ]
})
export class PublicModule { }
