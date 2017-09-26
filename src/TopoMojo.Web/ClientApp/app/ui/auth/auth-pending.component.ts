import { Component, OnInit } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../../svc/auth.service';

@Component({
    //moduleId: module.id,
    selector: 'auth-pending',
    templateUrl: 'auth-pending.component.html'
})
export class AuthPendingComponent implements OnInit {

    errorMessage: string;

    constructor(
        private route: ActivatedRouteSnapshot,
        private router: Router,
        private service: AuthService
    ) { }

    ngOnInit() {
        this.service.externalLoginCallback(this.route.fragment)
        .then(
            (user) => {
                // console.log(user);
                this.router.navigate([user.state || "/home"]);
            },
            (err) => { this.errorMessage = err }
        );
    }

}
