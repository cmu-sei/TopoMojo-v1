import { OnInit, Component } from '@angular/core';
import { Router } from '@angular/router';
import { UserProfile, AuthService } from './core/auth.service';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    profile : UserProfile = new UserProfile();
    profileSubscription: Subscription;

    constructor (
        private service : AuthService,
        private router: Router
    ){ }

    ngOnInit() {
        this.profileSubscription = this.service.profile$
            .subscribe(p =>  {
                //console.log(p);
                this.profile = p;
            });
        this.service.init();
    }

    logout() {
        this.service.logout()
        .then(result => {
            this.router.navigate(['/']);
        }, (err) => { this.router.navigate(['/']) });
    }

    ngOnDestroy() {
        this.profileSubscription.unsubscribe();
    }
}
