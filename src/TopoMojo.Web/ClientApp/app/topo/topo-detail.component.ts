import { Component, OnInit, Inject } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopoService } from './topo.service';
import 'rxjs/add/operator/switchMap';
import { DOCUMENT } from "@angular/platform-browser";

@Component({
    selector: 'topo-detail',
    templateUrl: './topo-detail.component.html',
    styleUrls: [ './topo-detail.component.css' ]
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
    showing: string = "topo";
    host: string;

    constructor(
        private service: TopoService,
        private route: ActivatedRoute,
        private router: Router,
        @Inject(DOCUMENT) private dom : Document
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

        this.service.ipCheck().subscribe(data => {
            console.log(data);
            this.host = data.host;
        });
    }

    copyToClipboard(text : string) {
        let el = this.dom.getElementById("clipboardText") as HTMLTextAreaElement;
        el.value = text;
        el.select();
        this.dom.execCommand("copy");
    }

    clipShareUrl() {
        this.copyToClipboard(this.host + "/enlist/" + this.topo.shareCode);
    }

    clipPublishUrl() {
        this.copyToClipboard(this.host + "/mojo/" + this.topo.id);
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

    publish() {
        this.service.publish(this.topo.id)
        .subscribe(data => {
            this.topo.isPublished = true;
        });
    }

    unpublish() {
        this.service.unpublish(this.topo.id)
        .subscribe(data => {
            this.topo.isPublished = false;
        });
    }

    share() {
        this.service.share(this.topo.id)
        .subscribe(data => {
            this.topo.shareCode = data.url;
        });
    }

    unshare() {
        this.service.unshare(this.topo.id)
        .subscribe(data => {
            this.topo.shareCode = "";
        });
    }

}


