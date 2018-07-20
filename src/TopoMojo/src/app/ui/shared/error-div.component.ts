import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-error-div',
    templateUrl: 'error-div.component.html'
})
export class ErrorDivComponent {
    @Input() errors: any[];
    @Output() errorCleared: EventEmitter<any> = new EventEmitter<any>();

    constructor() { }

    clearError(e: any): void {
        this.errors.splice(this.errors.indexOf(e), 1);
        this.errorCleared.emit(e);
    }

}
