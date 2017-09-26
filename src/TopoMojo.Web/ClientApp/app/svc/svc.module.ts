import { NgModule } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AuthService } from './auth.service';
import { AuthInterceptor } from './http-auth-interceptor';
import { AuthGuard } from './auth-guard.service';
import { AdminGuard } from './admin-guard.service';
import { NotificationService } from './notification.service';
import { SettingsService, ORIGIN_URL, getOriginUrl, createTranslateLoader,
    SIGNALR_CONFIG, createSignalRConfig,
    SHOWDOWN_OPTS, getShowdownOpts } from './settings.service';

@NgModule({
    providers: [
        AuthService,
        AuthGuard,
        AdminGuard,
        NotificationService,
        SettingsService,
        {
            provide: ORIGIN_URL,
            useFactory: (getOriginUrl)
        },
        {
            provide: SHOWDOWN_OPTS,
            useFactory: (getShowdownOpts)
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true,
        }
    ]
})
export class SvcModule { }