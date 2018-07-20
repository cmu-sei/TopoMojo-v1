import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../svc/auth.service';

@Component({
    template: ''
})
export class AuthSilentComponent implements OnInit {

    constructor(
        private authSvc: AuthService
    ) { }

    ngOnInit() {
        this.authSvc.silentLoginCallback();
    }

}
