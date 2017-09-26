import { Component, OnInit } from '@angular/core';

@Component({
    //moduleId: module.id,
    template: `
    <p class="alert alert-danger">You do not have permission for the requested resource</p>
    `
})
export class NotAllowedComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}