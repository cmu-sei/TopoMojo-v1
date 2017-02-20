import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TopoService } from './topo.service';

@Component({
    selector: 'topo-browser',
    templateUrl: './topo-browser.component.html',
    styleUrls: ['./topo-browser.component.css']
})
export class TopoBrowserComponent {
    topos: any[];
    term: string;
    editorVisible: boolean;

    constructor(private service: TopoService, private router: Router) {
        this.service = service;
    }

    ngOnInit(): void {
        this.search('');
    };

    search(term) {
        this.service.listtopo({
            term: term,
            take: 20,
            filters: []
        }).subscribe(data => {
            this.topos = data.results as any[];
        }, (err) => { this.service.onError(err); });
    }

    showEditor() {
        this.editorVisible = !this.editorVisible;
    }
}


