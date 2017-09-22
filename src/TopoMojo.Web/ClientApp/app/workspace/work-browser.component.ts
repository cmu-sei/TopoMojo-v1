import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TopologyService } from '../api/topology.service';
import { Search, TopologySummarySearchResult, Topology } from '../api/gen/models';

@Component({
    selector: 'workspace-browser',
    templateUrl: './work-browser.component.html',
    styleUrls: ['./work-browser.component.css']
})
export class WorkBrowserComponent {
    topos: Topology[] = [];
    term: string;
    model: Search = {
        term: '',
        skip: 0,
        take: 20,
        filters: []
    };
    hasMore: number;
    editorVisible: boolean;
    loading: boolean;

    constructor(
        private service: TopologyService,
        private router: Router
    ) {
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
        this.loading = true;
        this.model.filters = ["mine"];
        this.service.getTopologies(this.model)
        .subscribe(
            (data) => {
                this.topos = this.topos.concat(data.results);
                this.hasMore = data.total - (data.search.skip+data.search.take);
            },
            (err) => { },
            () => {
                this.loading = false;
            }
        );
    }

    showEditor() {
        this.editorVisible = !this.editorVisible;
    }
}


