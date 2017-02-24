import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TopoService } from './topo.service';

@Component({
    selector: 'topo-browser',
    templateUrl: './topo-browser.component.html',
    styleUrls: ['./topo-browser.component.css']
})
export class TopoBrowserComponent {
    topos: any[] = [];
    term: string;
    model: any = {
        term: '',
        skip: 0,
        take: 20,
        filters: []
    };
    hasMore: number;
    editorVisible: boolean;

    constructor(private service: TopoService, private router: Router) {
        this.service = service;
    }

    ngOnInit(): void {
        this.search();
    };

    more() {
        this.model.skip += this.model.take;
        this.search();
    }

    termChanged(term) {
        this.hasMore = 0;
        this.model.term = term;
        this.model.skip = 0;
        this.topos = [];
        this.search();
    }

    search() {
        this.service.listtopo(this.model)
        .subscribe(data => {
            this.topos = this.topos.concat(data.results);
            this.hasMore = data.total - (data.skip+data.take);
        }, (err) => { this.service.onError(err); });
    }

    showEditor() {
        this.editorVisible = !this.editorVisible;
    }
}


