import { NgModule } from '@angular/core';
import { RouterModule , Router, PreloadAllModules, PreloadingStrategy} from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_BASE_HREF } from '@angular/common';
import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { AuthModule } from './auth/auth.module';
import { CoreModule } from './core/core.module';
import { TopoModule } from './topo/topo.module';
import { GamespaceModule } from './gamespace/gamespace.module';
import { ConsoleModule } from './console/console.module';
import { AdminModule } from './admin/admin.module';
import { ProfileModule } from './profile/profile.module';
import { SharedModule } from './shared/shared.module';
import { AppComponent } from './app.component'
import { SignalRModule } from 'ng2-signalr';
import { SignalRConfiguration } from 'ng2-signalr';
import { ChatModule } from './chat/chat.module';

export function getOriginUrl() {
  return window.location.origin;
}

export function createConfig(): SignalRConfiguration {
    const c = new SignalRConfiguration();
    c.hubName = 'TopologyHub';
    c.qs = { user: 'jam' };
    c.url = getOriginUrl();
    c.logging = true;
    return c;
}

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        SharedModule,
        AuthModule,
        //ProfileModule,
        //ConsoleModule,
        TopoModule,
        GamespaceModule,
        AdminModule,
        CoreModule,
        ChatModule,
        SignalRModule.forRoot(createConfig),
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'home/notfound' }
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
