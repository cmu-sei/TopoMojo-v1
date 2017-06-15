import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopoService } from './topo.service';
import 'rxjs/add/operator/switchMap';

@Component({
    selector: 'topo-detail',
    templateUrl: './topo-detail.component.html'
})
export class TopoDetailComponent {
    topo: any;
    trefs: any[];
    publishedTemplates: any[];
    selectorVisible: boolean;
    documentVisible: boolean;
    documentText: string = "# Title";
    deleteMsgVisible: boolean;
    ttIcon: string = 'fa fa-clipboard';
    addIcon: string = 'fa fa-plus-circle';
    showing: string = "templates";

    constructor(
        private service: TopoService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.route.params
            .switchMap((params: Params) => this.service.loadTopo(params['id']))
            .subscribe(result => {
                this.topo = result as any;
            }, (err) => { this.service.onError(err); });

        this.route.params
            .switchMap((params: Params) => this.service.listTopoTemplates(params['id']))
            .subscribe(result => {
                this.trefs = result as any[];
            }, (err) => { this.service.onError(err); });
    }

    toggleSelector() {
        this.selectorVisible = !this.selectorVisible;
        if (this.selectorVisible) {
            this.search('');
        }
    }

    toggleDocument() {
        this.documentVisible = !this.documentVisible;
    }
    search(term) {
        this.service.listTemplates({
            term: term,
            take: 100,
            filters: [
                {name: 'published', id: 0 }
            ]
        })
        .subscribe(data => {
            this.publishedTemplates = data.results as any[];
        }, (err) => { this.service.onError(err); });
    }

    onAdded(template) {
        //this.trefs.push(template);
        this.service.addTemplate({
            topologyId: this.topo.id,
            templateId: template.id})
        .subscribe(result => {
            this.trefs.push(result);
        }, (err) => { this.service.onError(err); });
    }

    //remove tref from list
    remove(tref) {
        let i = 0;
        for (i=0; i<this.trefs.length; i++) {
            if (this.trefs[i].id == tref.id) {
                this.trefs.splice(i, 1);
                break;
            }
        }
    }

    confirmDelete() {
        this.deleteMsgVisible = true;
    }

    cancelDelete() {
        this.deleteMsgVisible = false;
    }

    delete() {
        this.service.deleteTopo(this.topo)
        .subscribe(data => {
            this.router.navigate(['/topo']);
        }, (err) => { this.service.onError(err)});
    }

    update() {
        this.service.updateTopo(this.topo)
        .subscribe(data => {

        }, (err) => { this.service.onError(err)});
    }

    show(section: string) : void {
        this.showing = section;
    }
}


