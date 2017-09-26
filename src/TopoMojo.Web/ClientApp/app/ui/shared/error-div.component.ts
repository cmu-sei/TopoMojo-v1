import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'error-div',
    templateUrl: 'error-div.component.html'
})
export class ErrorDivComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

    @Input() errors: any[];
    @Output() onErrorCleared : EventEmitter<any> = new EventEmitter<any>();

    errorCleared(e : any) : void {
        this.errors.splice(this.errors.indexOf(e), 1);
        this.onErrorCleared.emit(e);
    }

}