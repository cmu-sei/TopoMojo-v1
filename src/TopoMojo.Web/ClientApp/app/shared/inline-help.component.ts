import { Component, OnInit } from '@angular/core';

@Component({
    //moduleId: module.id,
    selector: 'inline-help',
    templateUrl: 'inline-help.component.html',
    styles: [ `
        div {
            display: inline-block;
        }
    `]
})
export class InlineHelpComponent implements OnInit {
    visible: boolean;

    constructor() { }

    ngOnInit() {

    }

    toggle() {
        this.visible = !this.visible;
    }

}