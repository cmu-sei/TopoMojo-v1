import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { UiModule } from './ui/ui.module';
import { PageNotFoundComponent } from './ui/pages/page-not-found/page-not-found.component';
import { WelcomeComponent } from './ui/pages/welcome/welcome.component';
import { ApiModule } from './api/gen/api.module';
import { SvcModule } from './svc/svc.module';
import { ExpiringDialogComponent } from './ui/comps/expiring-dialog/expiring-dialog.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatInputModule } from '@angular/material/input';

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    ApiModule,
    SvcModule,
    MatSidenavModule,
    MatInputModule,
    UiModule,
    RouterModule.forRoot([
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
