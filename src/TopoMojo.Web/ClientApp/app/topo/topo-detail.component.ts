import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopoService } from './topo.service';
import { NotificationService } from '../shared/notification.service';
import 'rxjs/add/operator/switchMap';
import { DOCUMENT } from "@angular/platform-browser";
import {Observable, Subscription, Subject} from 'rxjs/Rx';
//import {TranslateService} from '@ngx-translate/core';


import { ORIGIN_URL } from '../shared/constants/baseurl.constants';

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
    //private connection: ISignalRConnection;
    private subs: Subscription[] = [];
    private id: number;

    constructor(
        private service: TopoService,
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
        this.service.loadTopo(this.id).subscribe(
            (result) => {
                this.topo = result as any;
                //console.log(this.topo);

                this.subs.push(
                    this.notifier.topoEvents.subscribe(
                        (event) => {
                            this.topo = event.model;
                        }
                    )
                );
                this.notifier.start(this.topo.globalId);

                this.service.listTopoTemplates(result.id).subscribe(
                    (result) => {
                        this.trefs = result as any[];
                    },
                    (err) => {
                        this.service.onError(err);
                    }
                );
            },
            (err) => {
                this.service.onError(err);
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


