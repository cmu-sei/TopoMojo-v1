import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from './auth.service';
import { UserManager, Log, MetadataService, User } from 'oidc-client';

@Component({
    //moduleId: module.id,
    selector: 'auth-pending',
    templateUrl: 'auth-pending.component.html'
})
export class AuthPendingComponent implements OnInit {

    errorMessage: string;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private service: AuthService
    ) { }

    ngOnInit() {
        this.route.fragment.subscribe(frag => {this.validate(frag)});
    }

    validate(frag) {
        this.service.validateLogin(frag)
        .then(user => {
                this.router.navigate([user.state]);
            },
            err => { this.errorMessage = err }
        );
    }
}
