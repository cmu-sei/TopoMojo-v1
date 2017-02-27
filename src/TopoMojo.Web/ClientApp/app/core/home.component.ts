import { Component } from '@angular/core';

@Component({
    selector: 'home',
    templateUrl: './home.component.html',
    styles: [`
        h1 {
            font-size: 15vmin;
            position: absolute;
            left: 50%;
            top: 50%;
            -webkit-transform: translateX(-50%) translatey(-50%);
            -moz-transform: translateX(-50%) translatey(-50%);
            transform: translateX(-50%) translatey(-50%);
        }
    `]
})
export class HomeComponent {
}
