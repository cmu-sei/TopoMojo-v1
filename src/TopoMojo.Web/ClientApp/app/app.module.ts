import { NgModule } from '@angular/core';
import { RouterModule , Router, PreloadAllModules, PreloadingStrategy} from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { APP_BASE_HREF } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { TranslateModule, TranslateLoader} from '@ngx-translate/core';
import { TranslateHttpLoader} from '@ngx-translate/http-loader';
import { ORIGIN_URL } from './shared/constants/baseurl.constants';
import { AuthModule } from './auth/auth.module';
import { CoreModule } from './core/core.module';
import { WorkspaceModule } from './workspace/workspace.module';
import { GamespaceModule } from './gamespace/gamespace.module';
import { AdminModule } from './admin/admin.module';
import { SharedModule } from './shared/shared.module';
import { AppComponent } from './app.component'
import { SignalRModule } from 'ng2-signalr';
import { SignalRConfiguration } from 'ng2-signalr';
import { ChatModule } from './chat/chat.module';
import { ApiModule } from './api/api.module';
import { AuthInterceptor } from './auth/http-auth-interceptor';

export function getOriginUrl() {
  return window.location.origin;
}

export function createConfig(): SignalRConfiguration {
    const c = new SignalRConfiguration();
    c.hubName = 'TopologyHub';
    c.qs = { user: 'jam' };
    c.url = getOriginUrl();
    c.logging = false;
    return c;
}
export function createTranslateLoader(http: HttpClient, baseHref) {

    // Temporary Azure hack
    if (baseHref === 'undefined' && typeof window !== 'undefined') {
        baseHref = window.location.origin;
    }

    // i18n files are in `wwwroot/lang/`
    return new TranslateHttpLoader(http, `/lang/`, '.json');
}

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        SharedModule,
        ApiModule,
        AuthModule,
        WorkspaceModule,
        GamespaceModule,
        AdminModule,
        CoreModule,
        ChatModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: (createTranslateLoader),
                deps: [HttpClient]
            }
        }),
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
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        }
    ]
})
export class AppModule {
    // Diagnostic only: inspect router configuration
//   constructor(router: Router) {
//     console.log('Routes: ', JSON.stringify(router.config, undefined, 2));
//   }
}
