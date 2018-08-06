import { Component, OnInit } from '@angular/core';
import { ToolbarService } from '../../svc/toolbar.service';

@Component({
  templateUrl: './welcome.component.html',
  styleUrls: ['./welcome.component.scss']
})
export class WelcomeComponent implements OnInit {

  constructor(
    private toolbarSvc: ToolbarService
  ) { }

  ngOnInit() {

  }

}
