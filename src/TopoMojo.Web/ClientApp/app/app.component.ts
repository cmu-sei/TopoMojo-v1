import { OnInit, Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth/auth.service';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    profile : any;
    profileSubscription: Subscription;

    constructor (
        private service : AuthService,
        private router: Router
    ){ }

    ngOnInit() {
        this.profileSubscription = this.service.user$
            .subscribe(p =>  {
                this.profile = (p) ? p.profile : p;
                if (!p) {
                    this.router.navigate(["login"]);
                }
            });
        this.service.init();
    }

    login() {
        this.service.initiateLogin(null);
    }

    logout() {
        this.service.initiateLogout();
    }

    ngOnDestroy() {
        this.profileSubscription.unsubscribe();
    }
}
