import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute, Params} from '@angular/router';
import { AuthService } from './auth.service';
import 'rxjs/add/operator/switchMap';

@Component({
    //moduleId: module.id,
    selector: 'reset',
    templateUrl: 'reset.component.html'
})
export class ResetComponent implements OnInit {
    username: string;
    pass1: string;
    pass2: string;
    code: string;
    url: string;
    errorMessage : string;

    constructor(
        private service : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        // this.route.params
        //     .switchMap((params: Params) => this.setUser(params['email']))
        //     .subscribe(data => {
        //     });
    }

    setUser(user) {
        this.username = user;
    }
    reset() {
        if (!this.username) {
            this.errorMessage = "Please specify your email address.";
            return;
        }

        if (this.pass1 == null || this.pass2==null || this.pass1 != this.pass2) {
            this.errorMessage = "Passwords need to match.";
            return;
        }

        if (this.code == null) {
            this.errorMessage = "Please enter the reset code.";
            return;
        }

        this.service.reset({
            email : this.username,
            password : this.pass1,
            code: this.code
        })
        .then(data => {
            this.router.navigate(['/']);
        }, (err) => { this.errorMessage = 'Invalid login credentials'});
    }
}