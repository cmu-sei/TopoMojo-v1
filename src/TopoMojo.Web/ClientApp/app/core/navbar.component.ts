import { OnInit, Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';
import { Subscription } from 'rxjs/Subscription';
import { SettingsService, Layout } from '../auth/settings.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: 'navbar',
    templateUrl: './navbar.component.html',
    styleUrls: ['./navbar.component.css']
})
export class NavbarComponent {
    profile : any;
    profileSubscription: Subscription;
    status: Subscription;
    appName: string = "TopoMojo";
    @Input() layout: Layout;
    currentLanguage: string = "LANG.EN";
    dropDownVisible: boolean = false;
    lang: string[] = [];

    constructor (
        private service : AuthService,
        private router: Router,
        private settings: SettingsService,
        private translate: TranslateService
    ){
        // let lang = (settings.lang || "en").split(' ').shift().toUpperCase();
        // this.currentLanguage = "LANG." + lang;
    }

    ngOnInit() {
        this.lang = (this.settings.lang || 'en').trim().split(' ');
        this.setCulture(this.lang[0]);
        this.appName = this.settings.branding.applicationName || this.appName;
        this.profileSubscription = this.service.user$
        .subscribe(p =>  {
            this.profile = (p) ? p.profile : p;
        });
        this.profile = this.service.currentUser && this.service.currentUser.profile;
    }

    logout() {
        this.service.logout();
        this.router.navigate(['/home']);
    }

    setCulture(code: string) : void {
        this.translate.use(code);
        this.currentLanguage = "LANG." + code.toUpperCase();
        this.dropDownVisible = false;

    }

    contribute() {
        let url = `/lang/${this.currentLanguage.split('.').pop().toLowerCase()}.json`;
        window.open(url);
        this.dropDownVisible = false;
    }

    toggleDropdown() : void {
        this.dropDownVisible = !this.dropDownVisible;
    }
}
