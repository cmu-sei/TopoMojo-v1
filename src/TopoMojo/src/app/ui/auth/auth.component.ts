import { Component, OnInit } from '@angular/core';

@Component({
    template: `
        <div class="container">
            <router-outlet></router-outlet>
        </div>
    `,
    styles: [ `
        .container {
            padding: 16px 0px 16px 0px;
            max-width: 600px;
        }
    `
    ]
})
export class AuthComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}
