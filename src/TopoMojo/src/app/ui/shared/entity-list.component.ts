import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-entity-list',
    templateUrl: 'entity-list.component.html',
    styleUrls: [ 'entity-list.component.css' ]
})
export class EntityListComponent implements OnInit {
    @Input() entities: any[] = [];
    private entity: any;
    @Input() icon: string;
    @Output() selected: EventEmitter<any> = new EventEmitter<any>();
    constructor() { }

    ngOnInit() {

    }

    itemClicked(e) {
        this.entity = e;
        this.selected.emit(e);
    }
}
