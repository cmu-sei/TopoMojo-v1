import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../svc/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: 'auth-test.component.html',
  styleUrls: ['auth-test.component.css']
})
export class AuthTestComponent implements OnInit {
  _user: any = {};

    constructor(
      private authService: AuthService,
      private router: Router
    ) { }

  ngOnInit() {
  }
  clearState() {
    this.authService.clearStaleState();
  }
  getUser() {
    this._user = this.authService.oidcUser;
  }
  removeUser() {
    this.authService.expireToken();
    this.router.navigateByUrl("/home");
  }
  startSigninMainWindow() {
    this.authService.externalLogin('');
  }
  endSigninMainWindow() {
    this.authService.externalLoginCallback('');
  }
  startSignoutMainWindow() {
    this.authService.logout();
  }
  endSignoutMainWindow() {
    // this.authService.finalizeLogout();
  }

}