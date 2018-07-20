import { Component, OnInit } from '@angular/core';

@Component({
    template: `
    <p class="alert alert-danger">You do not have permission for the requested resource</p>
    `
})
export class NotAllowedComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}
