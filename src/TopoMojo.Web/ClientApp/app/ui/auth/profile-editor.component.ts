import { Component, OnInit } from '@angular/core';
import { AccountService } from '../../api/account.service';
import { AuthService } from '../../svc/auth.service';
import { ProfileService } from '../../api/profile.service';

@Component({
    selector: 'profile-editor',
    templateUrl: 'profile-editor.component.html',
    styleUrls: [ 'profile-editor.component.css' ]
})
export class ProfileEditorComponent implements OnInit {

    constructor(
        private authSvc : AuthService,
        private profileSvc : ProfileService
    ) { }
    profileName: string = "";

    ngOnInit() {
        this.profileName = this.authSvc.currentUser.profile.name;
    }

    update() : void {
        this.profileSvc.postProfile({
            globalId : this.authSvc.currentUser.profile.sub,
            name : this.profileName
        }).subscribe(
            (result) => {
                this.authSvc.currentUser.profile.name = result.name;
            }
        );
    }
}