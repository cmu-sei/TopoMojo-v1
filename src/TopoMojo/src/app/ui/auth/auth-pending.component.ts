import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../svc/auth.service';

@Component({
    templateUrl: 'auth-pending.component.html'
})
export class AuthPendingComponent implements OnInit {

    errorMessage: string;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private authSvc: AuthService
    ) {
    }

    ngOnInit() {
        this.authSvc.externalLoginCallback(this.route.snapshot.fragment)
        .then(
            (user) => {
                this.router.navigateByUrl(this.authSvc.cleanUrl(user.state) || '/home');
            },
            (err) => { this.errorMessage = err; }
        );
    }

}
