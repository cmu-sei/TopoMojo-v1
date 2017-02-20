import { Component, Input } from '@angular/core';

@Component({
    selector: 'collapser',
    template: `
        <div>
            <button class="btn btn-link" (click)="toggle()">
                <h4>
                    <span class="fa fa-plus-circle"></span>
                    <span>New Simulation</span>
                </h4>
            </button>
            <div *ngIf="show">
                <ng-content></ng-content>
            </div>
        </div>
    `
})
export class Collapser {
    @Input() title: string;
    show: boolean;

    toggle() {
        this.show = !this.show;
    }
}