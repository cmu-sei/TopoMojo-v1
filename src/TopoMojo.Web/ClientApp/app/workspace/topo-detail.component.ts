import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopologyService } from '../api/topology.service';
import { TemplateService } from '../api/template.service';
import { Topology, Template, TemplateSummary, TemplateSummarySearchResult } from "../api/gen/models";
import { NotificationService } from '../shared/notification.service';
import 'rxjs/add/operator/switchMap';
import { DOCUMENT } from '@angular/platform-browser';
import {Observable, Subscription, Subject} from 'rxjs/Rx';
import { ORIGIN_URL } from '../shared/constants/baseurl.constants';

@Component({
    selector: 'topo-detail',
    templateUrl: './topo-detail.component.html',
    styleUrls: [ './topo-detail.component.css' ]
})
export class TopoDetailComponent {
    topo: Topology;
    publishedTemplates: TemplateSummary[];
    selectorVisible: boolean;
    documentVisible: boolean;
    documentText: string = "# Title";
    deleteMsgVisible: boolean;
    ttIcon: string = 'fa fa-clipboard';
    addIcon: string = 'fa fa-plus-circle';
    showing: string = "topo";
    errors: any[] = [];
    showOverlay: boolean;
    private subs: Subscription[] = [];
    private id: number;

    constructor(
        private service: TopologyService,
        private templateSvc: TemplateService,
        private notifier: NotificationService,
        private route: ActivatedRoute,
        private router: Router,
        @Inject(ORIGIN_URL) private origin,
        @Inject(DOCUMENT) private dom : Document
    ) {
    }

    ngOnInit(): void {

        //this.loading = true;
        this.id = +this.route.snapshot.paramMap.get('id');
        this.service.getTopology(this.id).subscribe(
            (result: Topology) => {
                this.topo = result;
                //console.log(this.topo);
                this.subs.push(
                    this.notifier.topoEvents.subscribe(
                        (event) => {
                            switch (event.action) {
                                case "TOPO.UPDATED":
                                this.topo = event.model;
                                break;

                                case "TOPO.DELETED":
                                this.showOverlay = true;
                                break;
                            }
                        }
                    ),
                    this.notifier.templateEvents.subscribe(
                        (event) => {
                            switch (event.action) {
                                case "TEMPLATE.ADDED":
                                this.topo.templates.push(event.model);
                                break;

                                case "TEMPLATE.UPDATED":
                                this.merge(event.model);
                                break;

                                case "TEMPLATE.REMOVED":
                                this.remove(event.model);
                                break;
                            }
                        }
                    )
                );
                this.notifier.start(this.topo.globalId);

                // this.service.listTopoTemplates(result.id).subscribe(
                //     (result) => {
                //         this.trefs = result as any[];
                //     },
                //     (err) => {
                //         this.service.onError(err);
                //     }
                // );
            },
            (err) => {
                //this.service.onError(err);
            }
        );

    }

    ngOnDestroy() {
        this.notifier.stop();

        this.subs.forEach(
            (sub) => {
                sub.unsubscribe();
            }
        );
    }

    copyToClipboard(text : string) {
        let el = this.dom.getElementById("clipboardText") as HTMLTextAreaElement;
        el.value = text;
        el.select();
        this.dom.execCommand("copy");
    }

    clipShareUrl() {
        this.copyToClipboard(this.origin + "/enlist/" + this.topo.shareCode);
    }

    clipPublishUrl() {
        this.copyToClipboard(this.origin + "/mojo/" + this.topo.id);
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
        this.templateSvc.getTemplates({
            term: term,
            take: 100,
            filters: ["published"]
        })
        .subscribe(
            (data) => {
                this.publishedTemplates = data.results;
            },
            (err) => { this.onError(err);  }
        );
    }

    onAdded(template : TemplateSummary) {
        this.templateSvc.linkTemplate(template.id, this.topo.id)
        .subscribe(
            (result: Template) => {
                this.notifier.sendTemplateEvent("TEMPLATE.ADDED", result);
                this.topo.templates.push(result);
            },
            (err) => {  this.onError(err); }
        );
    }

    merge(tref) {
        this.topo.templates.forEach(
            (v, i, a) => {
                if (v.id == tref.id)
                    a[i] = tref;
            }
        )
    }

    //remove tref from list
    remove(template: Template) {
        this.topo.templates.splice(
            this.topo.templates.indexOf(template),
            1
        );
        // for (let i=0; i<this.trefs.length; i++) {
        //     if (this.trefs[i].id == tref.id) {
        //         //this.notifier.sendTemplateEvent("TEMPLATE.REMOVED", tref);
        //         this.trefs.splice(i, 1);
        //         break;
        //     }
        // }
    }

    confirmDelete() {
        this.deleteMsgVisible = true;
    }

    cancelDelete() {
        this.deleteMsgVisible = false;
    }

    delete() {
        this.service.deleteTopology(this.topo.id)
        .subscribe(data => {
            this.router.navigate(['/topo']);
        }, (err) => {  this.onError(err); });
    }

    update() {
        this.service.putTopology(this.topo)
        .subscribe(data => {

        }, (err) => {  this.onError(err); });
    }

    show(section: string) : void {
        this.showing = section;
    }

    publish() {
        this.service.publishTopology(this.topo.id)
        .subscribe(
            (data) => {
                this.topo.isPublished = true;
            },
            (err) => { this.onError(err); }
        );
    }

    unpublish() {
        this.service.unpublishTopology(this.topo.id)
        .subscribe(
            (data) => {
                this.topo.isPublished = false;
            },
            (err) => { this.onError(err); }
        );
    }

    share() {
        this.service.shareTopology(this.topo.id)
        .subscribe(
            (data) => {
                this.topo.shareCode = data.shareCode;
            },
            (err) => { this.onError(err); }
        );
    }

    unshare() {
        this.service.unshareTopology(this.topo.id)
        .subscribe(
            (data) => {
                this.topo.shareCode = data.shareCode;
            },
            (err) => { this.onError(err); }
        );
    }

    onError(err) {
        //let text = JSON.parse(err.text());
        this.errors.push(err.error);
        console.debug(err.error.message);
    }

    redirect() {
        this.router.navigate(['/topo']);
    }
}


