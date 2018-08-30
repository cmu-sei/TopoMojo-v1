import { NgModule, APP_INITIALIZER } from '@angular/core';
import { HTTP_INTERCEPTORS } from '@angular/common/http'
import { AuthService } from './auth.service';
import { AuthInterceptor } from './http-auth-interceptor';
import { AuthGuard } from './auth-guard.service';
import { AdminGuard } from './admin-guard.service';
import { NotificationService } from './notification.service';
import { ClipboardService } from './clipboard.service';
import { SettingsService, ORIGIN_URL, getOriginUrl, SHOWDOWN_OPTS, getShowdownOpts } from './settings.service';
import { UserService } from './user.service';

@NgModule({
    providers: [
        SettingsService,
        AuthService,
        AuthGuard,
        AdminGuard,
        ClipboardService,
        NotificationService,
        UserService,
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
        },
        {
            provide: APP_INITIALIZER,
            useFactory: initSettings,
            deps: [SettingsService],
            multi: true
        }
    ]
})
export class SvcModule { }

export function initSettings(settings: SettingsService) {
    return () => settings.load();
}
