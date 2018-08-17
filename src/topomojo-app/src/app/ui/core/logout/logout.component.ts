import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../../svc/auth.service';

@Component({
  selector: 'topomojo-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.scss']
})
export class LogoutComponent implements OnInit {

  constructor(
    private authSvc: AuthService
  ) { }

  ngOnInit() {
    this.authSvc.logout();
  }

}
