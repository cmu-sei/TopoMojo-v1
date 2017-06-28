import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TopoService } from './topo.service';

@Component({
    selector: 'gamespace',
    templateUrl: './gamespace.component.html',
    styleUrls: ['./gamespace.component.css']
})
export class GamespaceComponent {
    // topos: any[] = [];
    // term: string;
    // model: any = {
    //     term: '',
    //     skip: 0,
    //     take: 20,
    //     filters: []
    // };
    // hasMore: number;
    // editorVisible: boolean;
    instances: any[];
    loading: boolean = true;

    constructor(
        private service: TopoService,
        private router: Router
    ) {
        this.service = service;
    }

    ngOnInit(): void {
        this.loadActive();
        //this.search();
    };

    // more() {
    //     this.model.skip += this.model.take;
    //     this.search();
    // }

    // termChanged(term) {
    //     this.hasMore = 0;
    //     this.model.term = term;
    //     this.model.skip = 0;
    //     this.topos = [];
    //     this.search();
    // }

    // search() {
    //     this.service.listtopo(this.model)
    //     .subscribe(data => {
    //         this.topos = this.topos.concat(data.results);
    //         this.hasMore = data.total - (data.skip+data.take);
    //     }, (err) => { this.service.onError(err); });
    // }

    // showEditor() {
    //     this.editorVisible = !this.editorVisible;
    // }

    loadActive() {
        this.loading = true;
        this.service.activeInstances().subscribe(result => {
            this.instances = result;
        },
        (err) => {},
        () => {
            this.loading = false;
        });
    }

    destroyInstance(id: number) {
        this.loading = true;
        this.service.destroyInstance(id).subscribe(result => {
            this.loadActive();
        })
    }
}


