import { Component, OnInit } from '@angular/core';

@Component({
    //moduleId: module.id,
    //selector: 'topo-panel',
    template: `
        <router-outlet></router-outlet>
    `
})
export class TopoComponent implements OnInit {

    constructor() { }

    ngOnInit() {

    }

}