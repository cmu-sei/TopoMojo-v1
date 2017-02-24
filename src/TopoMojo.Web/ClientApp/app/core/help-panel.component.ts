import { Component, OnInit } from '@angular/core';

@Component({
    //moduleId: module.id,
    selector: 'help-panel',
    templateUrl: 'help-panel.component.html',
    styles: [ `
        .avatar {
            font-size: 4em;
        }
        .div {
            width: auto;
        }
        .instruction > div {
            display: inline;
            padding: 16px;
        }
    `]
})
export class HelpPanelComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}