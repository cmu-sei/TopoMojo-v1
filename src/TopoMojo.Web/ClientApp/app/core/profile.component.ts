import { Component, OnInit } from '@angular/core';
import { Router , ActivatedRoute, Params} from '@angular/router';
import { AuthService } from './auth.service';
import 'rxjs/add/operator/switchMap';

@Component({
    //moduleId: module.id,
    selector: 'profile-editor',
    templateUrl: 'profile.component.html'
})
export class ProfileEditorComponent implements OnInit {
    name: string;
    current: string;
    pass1: string;
    pass2: string;
    errorMessage : string;

    constructor(
        private service : AuthService,
        private router: Router,
        private route: ActivatedRoute
        ) { }

    ngOnInit() {
        this.service.profile$.subscribe(p => {
            this.name = p.userName;
        });
        this.service.init();
    }

    save() {

        if (this.name) {
            this.service.updateProfile({ name: this.name});
        }
    }

    change() {
        if (!this.current) {
            this.errorMessage = "Please specify your current password.";
            return;
        }

        if (this.pass1 == null || this.pass2==null || this.pass1 != this.pass2) {
            this.errorMessage = "Passwords need to match.";
            return;
        }

        this.service.changePassword({
            current : this.current,
            password : this.pass1,
        })
        .then(data => {
            this.current = '';
            this.pass1 = '';
            this.pass2 = '';
        }, (err) => { console.log(err); this.errorMessage = JSON.parse(err.text()).message; });
    }
}