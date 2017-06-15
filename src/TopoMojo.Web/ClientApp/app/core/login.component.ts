import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute} from '@angular/router';
import { CoreAuthService } from './auth.service';
import { AuthService } from '../auth/auth.service';

@Component({
    //moduleId: module.id,
    selector: 'login',
    templateUrl: 'login.component.html'
})
export class LoginComponent implements OnInit {
    username: string;
    password: string;
    error: boolean = false;
    url: string;
    resetMessage : string;
    allowExternalLogin: boolean;

    constructor(
        private service : CoreAuthService,
        private auth : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.url = this.route.snapshot.params['url'] || this.service.redirectUrl;
        this.allowExternalLogin = this.auth.allowExternalLogin;
    }

    oidcLogin() {
        this.auth.initiateLogin(null);
    }

    login() {
        console.log('form submitted');
        this.auth.localLogin(this.username, this.password)
        .then(result => {
            this.error = false;
            //this.service.redirectUrl = '/';
            console.log(this.url);
            this.router.navigate([this.url]);
        }, (err) => { console.error(err); this.error = true; });
    }

    reset() {
        if (!this.username) {
            this.resetMessage = "Please specify your email address";
            return;
        }

        this.router.navigate(['/reset', { account: this.username }]);

        // this.service.forgot(this.username)
        // .then(data => {
        //     this.resetMessage = "An email has been sent.";
        //     this.router.navigate(['/reset', { email: this.username }]);
        // });
    }
}