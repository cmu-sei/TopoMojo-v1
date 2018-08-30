import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import {  PageNotFoundComponent } from './ui/core/page-not-found/page-not-found.component';
import { WelcomeComponent } from './ui/core/welcome/welcome.component';
import { ApiModule } from './api/gen/api.module';
import { SvcModule } from './svc/svc.module';
import { ExpiringDialogComponent } from './ui/shared/expiring-dialog/expiring-dialog.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatInputModule } from '@angular/material/input';
import { CoreModule } from './ui/core/core.module';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule,
    ApiModule,
    SvcModule,
    CoreModule,
    MatSidenavModule,
    MatInputModule,
    RouterModule.forRoot([
      {
        path: 'admin',
        loadChildren: './ui/admin/admin.module#AdminModule'
      },
      { path: '', component: WelcomeComponent, pathMatch: 'full' },
      { path: '**', component: PageNotFoundComponent }
    ])
  ],
  exports: [
    RouterModule
  ],
  providers: [],
  bootstrap: [AppComponent],
  entryComponents: [
    ExpiringDialogComponent
  ]
})
export class AppModule { }
