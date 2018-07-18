import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, AuthTokenState, UserProfile } from './svc/auth.service';
import { Subscription } from 'rxjs';
import { SettingsService, Layout } from './svc/settings.service';
import { TranslateService } from '@ngx-translate/core';
import { LayoutService } from './svc/layout.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    profile : UserProfile;
    showExpiring: boolean;
    showExpired: boolean;
    layout: Layout;
    layout$: Subscription;
    appName: string;

    constructor (
        private service : AuthService,
        private router: Router,
        private settingsSvc: SettingsService,
        private translator: TranslateService,
        private layoutSvc: LayoutService
    ){
        let lang = (settingsSvc.settings.lang).trim().split(' ').shift();
        translator.setDefaultLang(lang);
        translator.use(lang);
    }

    ngOnInit() {
        this.appName = this.settingsSvc.settings.branding.applicationName;

        this.service.profile$.subscribe(
            (p) =>  {
                this.profile = p;
                this.showExpiring = (p.state == AuthTokenState.expiring);
                this.showExpired = (p.state == AuthTokenState.expired);
                if (p.id && p.state == AuthTokenState.invalid) {
                    this.router.navigate(['/home']);
                }
            }
        );

        this.layoutSvc.layout$.subscribe(
            (layout : Layout) => {
                this.layout = layout;
            }
        );
        this.layout = this.settingsSvc.layout;
    }

    continue() {
        this.service.refreshToken();
    }

}
