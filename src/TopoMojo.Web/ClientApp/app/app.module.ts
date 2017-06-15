import { NgModule } from '@angular/core';
import { RouterModule , Router, PreloadAllModules, PreloadingStrategy} from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_BASE_HREF } from '@angular/common';
import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { AuthModule } from './auth/auth.module';
import { CoreModule } from './core/core.module';
import { TopoModule } from './topo/topo.module';
import { ConsoleModule } from './console/console.module';
import { AdminModule } from './admin/admin.module';
import { AppComponent } from './app.component'

export function getOriginUrl() {
  return window.location.origin;
}

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        AuthModule,
        CoreModule,
        ConsoleModule,
        TopoModule,
        AdminModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'topo', pathMatch: 'full' },
            { path: '**', redirectTo: 'notfound' }
        ])
    ],
    providers: [
        {
            provide: ORIGIN_URL,
            useFactory: (getOriginUrl)
        }
    ]
})
export class AppModule {
    // Diagnostic only: inspect router configuration
//   constructor(router: Router) {
//     console.log('Routes: ', JSON.stringify(router.config, undefined, 2));
//   }
}
