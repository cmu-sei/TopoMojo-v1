import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ProfileService } from '../../../api/profile.service';

@Component({
  selector: 'topomojo-profile-settings',
  templateUrl: './profile-settings.component.html',
  styleUrls: ['./profile-settings.component.scss']
})
export class ProfileSettingsComponent implements OnInit {
  @Input() profile;
  @ViewChild(NgForm) form;

  constructor(
    private profileSvc: ProfileService
  ) { }

  ngOnInit() {
  }

  update() {
    this.profileSvc.putProfilePriv(this.profile).subscribe(
      () => {
        this.form.reset(this.form.value);
      }
    );
  }

}
