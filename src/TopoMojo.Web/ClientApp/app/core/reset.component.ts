import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute, ActivatedRouteSnapshot, Params} from '@angular/router';
import { CoreAuthService } from './auth.service';
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
        private service : CoreAuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.username = this.route.snapshot.params["account"];
    }

    reset() {
        if (!this.username) {
            this.errorMessage = "Please specify your email address.";
            return;
        }

        if (this.pass1 == null || this.pass2 == null || this.pass1 != this.pass2) {
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
            //todo: store jwt
            this.router.navigate(['/']);
        }, (err) => { console.log(err); this.errorMessage = JSON.parse(err.text()).message;});
    }
}