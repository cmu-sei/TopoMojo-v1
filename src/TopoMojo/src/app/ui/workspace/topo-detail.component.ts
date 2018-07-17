import { Component, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TopologyService } from '../../api/topology.service';
import { TemplateService } from '../../api/template.service';
import { Topology, Template, TemplateSummary } from '../../api/gen/models';
import { NotificationService } from '../../svc/notification.service';
import { DOCUMENT } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { ORIGIN_URL } from '../../svc/settings.service';

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
    private collaborationVisible: boolean;

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

            },
            (err) => {
                this.onError(err);
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
        this.copyToClipboard(this.origin + "/topo/enlist/" + this.topo.shareCode);
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
                let existing = this.topo.templates.filter(
                    (v) => {
                        return v.name == result.name;
                    }
                );
                if (existing && existing.length > 0)
                    this.onError({error: { message: 'EXCEPTION.DUPLICATENAME'}});

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
        .subscribe(() => {
                this.router.navigate(['/topo']);
            }, (err) => {  this.onError(err); });
    }

    update() {
        this.service.putTopology(this.topo)
        .subscribe(() => {
            }, (err) => {  this.onError(err); });
    }

    show(section: string) : void {
        this.showing = section;
    }

    publish() {
        this.service.publishTopology(this.topo.id)
        .subscribe(
            () => {
                this.topo.isPublished = true;
            },
            (err) => { this.onError(err); }
        );
    }

    unpublish() {
        this.service.unpublishTopology(this.topo.id)
        .subscribe(
            () => {
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
        this.errors.push(err.error);
        console.debug(err.error.message);
    }

    collaborate() {
        this.collaborationVisible = !this.collaborationVisible;
    }

    redirect() {
        this.router.navigate(['/topo']);
    }
}


