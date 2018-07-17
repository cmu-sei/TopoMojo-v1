import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../svc/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: 'auth-test.component.html',
  styleUrls: ['auth-test.component.css']
})
export class AuthTestComponent implements OnInit {
  _user: any;
  loadedUserSub: any;

    constructor(
      private authService: AuthService
    ) { }

  ngOnInit() {
    this.loadedUserSub = this.authService.user$
      .subscribe(user => {
        this._user = user;
      });
  }
  clearState() {
    //this.authService.clearState();
  }
  getUser() {
    this.authService.init();
  }
  removeUser() {
    //this.authService.removeUser();
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

  ngOnDestroy(){
    if(this.loadedUserSub !== null){
      this.loadedUserSub.unsubscribe();
    }
  }
}