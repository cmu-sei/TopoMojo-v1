import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../../svc/settings.service';
import { AuthService } from '../../../svc/auth.service';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'topomojo-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  authority = '';
  authfrag = '';
  authmsg = '';

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private authSvc: AuthService
  ) { }

  ngOnInit() {
    this.authfrag = this.route.snapshot.fragment;
    if (this.authfrag) {
      this.authSvc.externalLoginCallback(this.authfrag)
      .then(
        (user) => {
          this.router.navigateByUrl(this.authSvc.cleanUrl(user.state) || '/topo');
        },
        (err) => {
          console.log(err);
          this.authmsg = (err.error || err).message;
        }
      );
    } else {
      this.authority = this.authSvc.authority;
      if (this.authority) { this.login(); }
    }
  }

  login(): void {
    this.authSvc.externalLogin(
      this.authSvc.redirectUrl || this.route.snapshot.params['url'] );
  }

}
