import { Component, Inject, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TopologyService } from '../../api/topology.service';
import { TemplateService } from '../../api/template.service';
import { Topology, Template, TemplateSummary, TopologyStateActionTypeEnum } from '../../api/gen/models';
import { NotificationService } from '../../svc/notification.service';
import { Subscription } from 'rxjs';
import { ORIGIN_URL } from '../../svc/settings.service';
import { ClipboardService } from '../../svc/clipboard.service';
import { trigger, state, style } from '@angular/animations';

export class EventState {
    type: string;
    state: string;
}

@Component({
    selector: 'app-workspace',
    templateUrl: './workspace.component.html',
    styleUrls: [ './workspace.component.css' ],
    animations:  [
        trigger('eventState', [
            state('normal', style({
                color: 'inherit'
            })),
            state('success', style({
                color: 'green'
            })),
            state('failure', style({
                color: 'crimson'
            }))
        ])
    ]
})
export class WorkspaceComponent implements OnInit, OnDestroy {
    topo: Topology;
    publishedTemplates: TemplateSummary[];
    selectorVisible: boolean;
    // documentVisible: boolean;
    // documentText: string = "# Title";
    deleteMsgVisible: boolean;
    // ttIcon: string = 'fa fa-clipboard';
    addIcon = 'fa fa-plus-circle';
    showing = 'topo';
    errors: any[] = [];
    showOverlay: boolean;
    messageCount: number;
    private subs: Subscription[] = [];
    private id: number;
    private collaborationVisible: boolean;
    private state: Array<EventState> = new Array<EventState>();

    constructor(
        private service: TopologyService,
        private templateSvc: TemplateService,
        private notifier: NotificationService,
        private route: ActivatedRoute,
        private router: Router,
        private clipboard: ClipboardService,
        @Inject(ORIGIN_URL) private origin
    ) {
    }

    ngOnInit(): void {

        this.id = +this.route.snapshot.paramMap.get('id');
        this.service.getTopology(this.id).subscribe(
            (result: Topology) => {
                this.topo = result;
                this.topo.shareCode = this.origin + '/topo/enlist/' + this.topo.shareCode;

                // console.log(this.topo);
                this.subs.push(
                    this.notifier.topoEvents.subscribe(
                        (event) => {
                            switch (event.action) {
                                case 'TOPO.UPDATED':
                                this.topo = event.model;
                                break;

                                case 'TOPO.DELETED':
                                this.showOverlay = true;
                                break;
                            }
                        }
                    ),
                    this.notifier.templateEvents.subscribe(
                        (event) => {
                            switch (event.action) {
                                case 'TEMPLATE.ADDED':
                                console.log(event.model);
                                this.topo.templates.push(event.model);
                                break;

                                case 'TEMPLATE.UPDATED':
                                this.merge(event.model);
                                break;

                                case 'TEMPLATE.REMOVED':
                                this.remove(event.model);
                                break;
                            }
                        }
                    ),
                    this.notifier.chatEvents.subscribe(
                        (event) => {
                            if (event.action === 'CHAT.MESSAGE' && !this.collaborationVisible) {
                                this.messageCount += 1;
                            }
                        }
                    )
                );

                const loadWorkers = new Promise((resolve) => {
                    this.notifier.actors = this.topo.workers.map(
                        (worker) => {
                            return {
                                id: worker.personGlobalId,
                                name: worker.personName,
                                online: false
                            };
                        }
                    );
                    resolve(true);
                });

                loadWorkers.then(() => {
                    this.notifier.start(this.topo.globalId);
                });

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

    clipShareUrl() {
        this.clipboard.copyToClipboard(this.topo.shareCode);
        this.animateSuccess('clipShareUrl');
    }

    clipPublishUrl() {
        this.clipboard.copyToClipboard(this.origin + '/mojo/' + this.topo.id);
        this.animateSuccess('clipPublishUrl');
    }

    toggleSelector() {
        this.selectorVisible = !this.selectorVisible;
        if (this.selectorVisible) {
            this.search('');
        }
    }

    // toggleDocument() {
    //     this.documentVisible = !this.documentVisible;
    // }

    search(term) {
        this.templateSvc.getTemplates({
            term: term,
            take: 100,
            filters: ['published']
        })
        .subscribe(
            (data) => {
                this.publishedTemplates = data.results;
            },
            (err) => { this.onError(err);  }
        );
    }

    onAdded(template: TemplateSummary) {
        this.templateSvc.postTemplateLink({
            templateId: template.id,
            topologyId: this.topo.id
        })
        .subscribe(
            // (result: Template) => {
            //     this.notifier.sendTemplateEvent("TEMPLATE.ADDED", result);
            //     let existing = this.topo.templates.filter(
            //         (v) => {
            //             return v.name == result.name;
            //         }
            //     );
            //     if (existing && existing.length > 0)
            //         this.onError({error: { message: 'EXCEPTION.DUPLICATENAME'}});

            //     this.topo.templates.push(result);
            // }
            () => { },
            (err) => {  this.onError(err); }
        );
    }

    merge(tref) {
        this.topo.templates.forEach(
            (v, i, a) => {
                if (v.id === tref.id) {
                    a[i] = tref;
                }
            }
        );
    }

    // remove tref from list
    remove(template: Template) {
        const target = this.topo.templates.find((t) => t.id === template.id);
        if (target) {
            this.topo.templates.splice(
                this.topo.templates.indexOf(target),
                1
            );
        }
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

    show(section: string): void {
        this.showing = section;
    }

    publish() {
        this.service.postTopologyAction(this.topo.id, {
            id: this.topo.id,
            type: TopologyStateActionTypeEnum.publish
        })
        .subscribe(
            () => {
                this.topo.isPublished = true;
                this.animateSuccess('publish');
            },
            (err) => {
                this.animateFailure('publish');
                this.onError(err);
            }
        );
    }

    unpublish() {
        this.service.postTopologyAction(this.topo.id, {
            id: this.topo.id,
            type: TopologyStateActionTypeEnum.unpublish
        })
        .subscribe(
            () => {
                this.topo.isPublished = false;
                this.animateSuccess('publish');
            },
            (err) => {
                this.animateFailure('publish');
                this.onError(err);
            }
        );
    }

    share() {
        this.service.postTopologyAction(this.topo.id, {
            id: this.topo.id,
            type: TopologyStateActionTypeEnum.share
        })
        .subscribe(
            (data) => {
                this.topo.shareCode = this.origin + '/topo/enlist/' + data.shareCode;
                this.animateSuccess('share');
            },
            (err) => {
                this.animateFailure('share');
                this.onError(err);
            }
        );
    }

    // unshare() {
    //     this.service.unshareTopology(this.topo.id)
    //     .subscribe(
    //         (data) => {
    //             this.topo.shareCode = data.shareCode;
    //         },
    //         (err) => { this.onError(err); }
    //     );
    // }

    onError(err) {
        this.errors.push(err.error);
        // console.debug(err.error.message);
    }

    collaborate() {
        this.collaborationVisible = !this.collaborationVisible;
        this.messageCount = 0;
    }

    showCollaboration(): boolean {
        return this.collaborationVisible;
    }

    redirect() {
        this.router.navigate(['/topo']);
    }

    getEvent(type: string) {
        let ev = this.state.find((item) => item.type === type);
        if (!ev) {
            ev = { type: type, state: 'normal'};
            this.state.push(ev);
        }
        return ev;
    }

    animateSuccess(type: string): void {
        const ev = this.getEvent(type);
        ev.state = 'success';
        setTimeout(() => { ev.state = 'normal'; }, 2000);
    }

    animateFailure(type: string): void {
        const ev = this.getEvent(type);
        ev.state = 'failure';
        setTimeout(() => { ev.state = 'normal'; }, 2000);
    }

    killGames(): void {
        this.service.deleteTopologyGames(this.topo.id).subscribe(
            (result) => {
                if (result) {
                    this.animateSuccess('gameCount');
                    this.topo.gamespaceCount = 0;
                }
            },
            (err) => {
                this.animateFailure('gameCount');
                this.onError(err);
            }
        );
    }
}
