import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ProfileService } from '../../../api/profile.service';
import { Profile } from '../../../api/gen/models';

@Component({
  selector: 'topomojo-profile-settings',
  templateUrl: './profile-settings.component.html',
  styleUrls: ['./profile-settings.component.scss']
})
export class ProfileSettingsComponent implements OnInit {
  @Input() profile: Profile;
  @Output() deleted = new EventEmitter<Profile>();
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

  delete() {
    this.profileSvc.deleteProfile(this.profile.id).subscribe(
      () => {
        this.deleted.emit(this.profile);
      }
    );
  }
}
