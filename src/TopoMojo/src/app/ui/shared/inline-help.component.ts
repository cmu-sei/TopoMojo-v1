import { Component, OnInit, Input } from '@angular/core';

@Component({
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
    @Input() message: string;

    constructor() { }

    ngOnInit() {

    }

    toggle() {
        this.visible = !this.visible;
    }

}
