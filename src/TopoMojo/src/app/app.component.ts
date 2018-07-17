import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, AuthTokenState } from './svc/auth.service';
import { Subscription } from 'rxjs';
import { SettingsService, Layout } from './svc/settings.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    profile : any;
    profile$: Subscription;
    status$: Subscription;
    showExpiring: boolean;
    showExpired: boolean;
    layout: Layout;
    layout$: Subscription;
    appName: string;

    constructor (
        private service : AuthService,
        private router: Router,
        private settingsSvc: SettingsService,
        private translator: TranslateService
    ){
        let lang = (settingsSvc.settings.lang).trim().split(' ').shift();
        translator.setDefaultLang(lang);
        translator.use(lang);
    }

    ngOnInit() {
        this.appName = this.settingsSvc.settings.branding.applicationName;

        this.profile$ = this.service.user$.subscribe(
            (p) =>  {
                this.profile = (p) ? p.profile : p;
            }
        );
        this.profile = this.service.currentUser && this.service.currentUser.profile;

        this.status$ = this.service.tokenStatus$
        .subscribe(status => {
            //console.log(status);
            this.showExpiring = (status == AuthTokenState.expiring);
            this.showExpired = (status == AuthTokenState.expired);
            if (status == AuthTokenState.invalid)
                this.router.navigate(['/home']);
        });

        this.layout$ = this.settingsSvc.layout$.subscribe((layout : Layout) => {
            this.layout = layout;
        });
        this.layout = this.settingsSvc.layout;
        //this.service.init();
    }

    continue() {
        this.service.refreshToken();
    }

    ngOnDestroy() {
        this.profile$.unsubscribe();
        this.status$.unsubscribe();
    }
}
