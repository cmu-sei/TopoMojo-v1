import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../svc/auth.service';

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
        console.log(this.route.snapshot.fragment);
        this.service.externalLoginCallback(this.route.snapshot.fragment)
        .then(
            (user) => {
                // console.log(user);
                this.router.navigate([user.state || "/home"]);
            },
            (err) => { this.errorMessage = err }
        );
    }

}
