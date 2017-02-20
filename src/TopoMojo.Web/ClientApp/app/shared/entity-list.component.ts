import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    //moduleId: module.id,
    selector: 'entity-list',
    templateUrl: 'entity-list.component.html',
    styleUrls: [ 'entity-list.component.css' ]
})
export class EntityListComponent implements OnInit {
    @Input() entities : any[];
    private entity: any;
    @Input() icon: string;
    @Output() onSelected: EventEmitter<any> = new EventEmitter<any>();
    constructor() { }

    ngOnInit() {

    }

    itemClicked(e) {
        this.entity = e;
        this.onSelected.emit(e);
        //setTimeout(() => { this.onSelected.emit(e) }, 5);
    }
}