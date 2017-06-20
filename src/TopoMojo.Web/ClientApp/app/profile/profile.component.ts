import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'profile',
    templateUrl: './profile.component.html'
})
export class ProfileComponent {

    constructor(
        private router : Router
    ){

    }

}