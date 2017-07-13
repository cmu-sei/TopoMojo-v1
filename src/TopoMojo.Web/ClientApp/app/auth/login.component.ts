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
    pass1: string;
    pass2: string;
    code: string;
    url: string;
    errorMessage : string;
    infoMessage : string;
    allowExternalLogin: boolean;
    showLocalLogin: boolean;
    mode: number = 0;
    title: string = "Login";
    action: string = "login";
    codeSent: boolean;

    constructor(
        private auth : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.url = this.auth.redirectUrl || this.route.snapshot.params['url'] || "/";
        this.allowExternalLogin = this.auth.allowExternalLogin;
        this.showLocalLogin = !this.allowExternalLogin;
    }

    oidcLogin() {
        this.auth.initiateLogin(null);
    }

    onSubmit() {
        this.infoMessage = "";
        this.errorMessage = "";

        if (this.mode > 2) {
            if (this.pass1 == null || this.pass2 == null || this.pass1 != this.pass2) {
                this.errorMessage = "Passwords need to match.";
                return;
            }
        }

        this.auth.localLogin(this.action, this.getCreds())
        .then(result => {
            this.router.navigate([this.url]);
        }, (err) => {
            console.error(err);
            this.errorMessage = "Invalid credentials";
        });
    }

    requestCode() {
        if (!this.username) {
            this.infoMessage = "Please specify the username to confirm.";
            return;
        }

        this.auth.confirm(this.username).subscribe(result => {
            this.infoMessage = "Code sent to " + this.username;
            this.codeSent = true;
        })
    }

    getCreds() {
        return {
            username : this.username,
            password : this.pass1,
            code : this.code
        }
    }

    setMode(i : number) : void {
        this.infoMessage = "";
        this.errorMessage = "";
        this.mode = i;
        switch (i) {
            case 0:
                this.title = "Login";
                this.action = "login";
                break;
            case 1:
                this.title = "Login with One-Time-Passcode";
                this.action = "otp";
                break;
            case 2:
                this.title = "Login with Two-Factor-Auth";
                this.action = "login";
                break;
            case 3:
                this.title = "Reset Password";
                this.action = "reset";
                break;
            case 4:
                this.title = "Register New Account";
                this.action = "register";
                break;
        }
    }
}