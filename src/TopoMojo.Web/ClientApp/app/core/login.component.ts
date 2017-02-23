import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute} from '@angular/router';
import { AuthService } from './auth.service';

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

    constructor(
        private service : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.url = this.route.snapshot.queryParams['url'] || this.service.redirectUrl;
    }

    login() {
        this.service.login(this.username, this.password)
        .then(result => {
            this.error = false;
            this.service.redirectUrl = '/';
            this.router.navigate([this.url]);
        }, (err) => { console.error(err); this.error = true; });
    }

    reset() {
        if (!this.username) {
            this.resetMessage = "Please specify your email address";
            return;
        }

        this.service.forgot(this.username)
        .then(data => {
            this.resetMessage = "An email has been sent.";
            this.router.navigate(['/reset', { email: this.username }]);
        });
    }
}