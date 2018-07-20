import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../svc/auth.service';
import { Subscription } from 'rxjs';
import { SettingsService, Layout } from '../../svc/settings.service';
import { TranslateService } from '@ngx-translate/core';
import { UserService } from '../../svc/user.service';
import { Profile } from '../../api/gen/models';

@Component({
    selector: 'app-navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
    profile: Profile;
    profileSubscription: Subscription;
    status: Subscription;
    appName: string;
    @Input() layout: Layout;
    currentLanguage = 'LANG.EN';
    dropDownVisible = false;
    lang: string[] = [];
    maintMessage = '';

    constructor (
        private authSvc: AuthService,
        private userSvc: UserService,
        private router: Router,
        private settingsSvc: SettingsService,
        private translate: TranslateService
    ) {
    }

    ngOnInit() {
        this.lang = (this.settingsSvc.settings.lang).trim().split(' ');
        this.setCulture(this.lang[0]);
        this.appName = this.settingsSvc.settings.branding.applicationName;
        this.maintMessage = this.settingsSvc.settings.maintMessage;
        this.userSvc.profile$.subscribe(
            p =>  {
                this.profile = p;
            }
        );
    }

    logout() {
        this.authSvc.logout();
        this.router.navigate(['/home']);
    }

    setCulture(code: string): void {
        this.translate.use(code);
        this.currentLanguage = 'LANG.' + code.toUpperCase();
        this.dropDownVisible = false;

    }

    contribute() {
        const url = `/lang/${this.currentLanguage.split('.').pop().toLowerCase()}.json`;
        window.open(url);
        this.dropDownVisible = false;
    }

    toggleDropdown(): void {
        this.dropDownVisible = !this.dropDownVisible;
    }

    isUser(): boolean {
        return !!this.profile.id;
    }

    isAdmin(): boolean {
        return this.profile.isAdmin;
    }
}
