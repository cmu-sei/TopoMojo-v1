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

    constructor (
        private service : AuthService,
        private router: Router,
        private settings: SettingsService,
        private translate: TranslateService
    ){ }

    ngOnInit() {
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

    lang(code) {
        this.translate.use(code);
    }
}
