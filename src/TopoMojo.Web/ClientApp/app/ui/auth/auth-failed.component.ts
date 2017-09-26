import { Component, OnInit } from '@angular/core';

@Component({
    //moduleId: module.id,
    selector: 'auth-failed',
    template: `
        <h4>Not allowed</h4>
    `
})
export class AuthFailedComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}