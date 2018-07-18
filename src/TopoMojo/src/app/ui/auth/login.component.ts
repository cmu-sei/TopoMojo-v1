import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute} from '@angular/router';
import { AuthService } from '../../svc/auth.service';
import { HttpErrorResponse } from '@angular/common/http';

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
    authority: string;
    errorMessage : string;
    infoMessage : string;
    mode: number = 0;
    title: string = "Login";
    action: string = "login";
    codeSent: boolean;

    constructor(
        private authSvc : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.url = this.authSvc.cleanUrl(this.authSvc.redirectUrl || this.route.snapshot.params['url'] || "/");
        this.authority = this.authSvc.authority;
        if (this.showExternal() && !this.showLocal())
            this.oidcLogin();
    }

    oidcLogin() {
        this.authSvc.externalLogin(this.url);
    }

    onSubmit() {
        this.infoMessage = "";
        this.errorMessage = "";

        if (this.mode > 2) {
            if (this.pass1 == null || this.pass2 == null || this.pass1 != this.pass2) {
                this.errorMessage = "AUTH.PASSWORDS-MUST-MATCH";
                return;
            }
        }

        // this.auth.localLogin(this.action, this.getCreds())
        // .then(result => {
        //     this.router.navigateByUrl(this.url);
        // }).catch((response: HttpErrorResponse) => {

        //     console.error(response);
        //     this.errorMessage = response.error.message;
        // });
    }

    requestCode() {
        // if (!this.username) {
        //     this.infoMessage = "AUTH.CONFIRMATION-NEEDS-USERNAME";
        //     return;
        // }

        // this.auth.sendAuthCode(this.username).subscribe(result => {
        //     this.infoMessage = "AUTH.CODE-SENT";
        //     this.codeSent = true;
        // })
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
                this.title = "Register Account";
                this.action = "register";
                break;
        }
    }

    showLocal() : boolean {
        return false; //this.auth.loginSettings.allowLocalLogin
    }
    showExternal() : boolean {
        return true; //this.auth.loginSettings.allowExternalLogin
    }
    showBoth() : boolean {
        return false; //this.auth.loginSettings.allowLocalLogin && this.auth.loginSettings.allowExternalLogin
    }
}
