import { OnInit, Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, AuthTokenState } from './auth/auth.service';
import { Subscription } from 'rxjs/Subscription';
import { SettingsService, Layout } from './auth/settings.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'app',
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
    appName: string = "TopoMojo";

    constructor (
        private service : AuthService,
        private router: Router,
        private settings: SettingsService,
        private translator: TranslateService
    ){
        let lang = (settings.lang || "en").trim().split(' ').shift();
        translator.setDefaultLang(lang);
        translator.use(lang);
    }

    ngOnInit() {
        this.appName = this.settings.branding.applicationName || "TopoMojo";

        this.profile$ = this.service.user$
        .subscribe(p =>  {
            this.profile = (p) ? p.profile : p;
        });
        this.profile = this.service.currentUser && this.service.currentUser.profile;

        this.status$ = this.service.tokenStatus$
        .subscribe(status => {
            console.log(status);
            this.showExpiring = (status == AuthTokenState.expiring);
            this.showExpired = (status == AuthTokenState.expired);
            if (status == AuthTokenState.invalid)
                this.router.navigate(['/home']);
        });

        this.layout$ = this.settings.layout$.subscribe((layout : Layout) => {
            this.layout = layout;
        });
        this.layout = this.settings.layout;
        //this.service.init();
    }

    // login() {
    //     this.service.externalLogin(null);
    // }

    // logout() {
    //     this.service.logout();
    //     this.router.navigate(['/home']);
    // }

    continue() {
        this.service.refreshToken();
    }

    ngOnDestroy() {
        this.profile$.unsubscribe();
        this.status$.unsubscribe();
    }
}
