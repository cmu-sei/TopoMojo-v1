import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { GamespaceService } from '../api/gamespace.service';
import { GameState, VmState } from "../api/gen/models";
import { NotificationService } from '../shared/notification.service';
import { Converter } from 'showdown/dist/showdown';
import { SHOWDOWN_OPTS } from '../shared/constants/ui-params';
import { VmService } from '../api/vm.service';
import { SettingsService } from '../auth/settings.service';
import {Observable, Subscription, Subject} from 'rxjs/Rx';

@Component({
    selector: 'player',
    templateUrl: 'player.component.html',
    styleUrls: [ 'player.component.css']
})
export class PlayerComponent implements OnInit {
    game: GameState;
    errors: any[] = [];
    markdownDoc: string;
    renderedDoc: string;
    docMissing: boolean;
    launching: boolean;
    loading: boolean = true;
    destroyMsgVisible: boolean;
    messageCount: number = 0;
    collaborationVisible: boolean = false;
    private converter : Converter;
    private id: number;
    private subs: Subscription[] = [];
    private showOverlay: boolean;
    private appName: string = "";

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service : GamespaceService,
        private vmService : VmService,
        private notifier: NotificationService,
        private settings: SettingsService
    ) {
        this.converter = new Converter(SHOWDOWN_OPTS);
        this.settings.changeLayout({ embedded : true });
        this.appName = this.settings.branding.applicationName;
    }

    ngOnInit() {
        this.loading = true;
        this.id = +this.route.snapshot.paramMap.get('id');
        this.service.getGamespace(this.id).subscribe(
            (result : GameState) => {
                this.service.getText(result.topologyDocument)
                    .finally(() => this.render())
                    .subscribe(
                        (text) => {
                            //todo: update console urls
                            //todo: enforce image size?
                            //let newHtml = this.converter.makeHtml(text);
                            //newHtml = newHtml.replace(/href="console"/g, 'href="console" target="console"');
                            //this.renderedDocument = newHtml;
                            //this.status = '';
                            this.markdownDoc = text;
                        }
                );

                this.initGame(result);
            },
            (err) => {
                this.onError(err);
            },
            () => {
                this.loading = false;
            }
        );

    }

    private initGame(game: GameState) {

        this.game = game;

        if (!this.game.globalId)
            return;

        this.game.shareCode = this.settings.hostUrl + "/mojo/enlist/" + this.game.shareCode;
        this.subs.push(
            this.notifier.topoEvents.subscribe(
                (event) => {
                    switch (event.action) {
                        case "GAME.OVER":
                        this.showOverlay = true;
                        break;
                    }
                }
            ),
            this.notifier.vmEvents.subscribe(
                (event) => {
                    this.onVmUpdated(event.model as VmState);
                }
            ),
            this.notifier.chatEvents.subscribe(
                (event) => {
                    switch (event.action) {
                        case "CHAT.MESSAGE":
                        if (!this.collaborationVisible)
                            this.messageCount += 1;
                        break;
                    }
                }
            )
        );
        this.notifier.start(this.game.globalId);
        this.loading = false;
    }

    private render() {
        this.docMissing = (!this.markdownDoc);
        this.renderedDoc = this.converter.makeHtml(this.markdownDoc);
    }

    onVmUpdated(vm: VmState) {
        this.game.vms.forEach(
            (v, i) => {
                //console.log(v);
                if (v.id == vm.id) {
                    v.isRunning = vm.isRunning;
                    this.game.vms.splice(i, 1, v);
                }
            }
        )
    }

    ngOnDestroy() {
        this.notifier.stop();

        this.subs.forEach(
            (sub) => {
                sub.unsubscribe();
            }
        );

        this.settings.changeLayout({ embedded : false });
    }

    launch() {
        this.loading = true;

        this.service.launchGamespace(this.id)
        .finally(() => this.loading = false)
        .subscribe(
            (result) => {
                this.initGame(result);
            },
            (err) => {
                this.onError(err);
            },
            () => {
                this.loading = false;
            }
        );
    }

    //wmks : any;
    launchConsole(vm) {
        //console.log('launch console ' + vm.id);
        this.service.openConsole(vm.id, vm.name);
        //this.vmService.launchPage("/vm/display/" + id);
        // this.vmService.ticket(id).subscribe(
        //     (result) => {
        //         console.log(result);
        //         //let test = new WMKS();
        //         this.wmks = WMKS.createWMKS('console-canvas-div',{
        //             rescale: false,
        //             position: 0,
        //             changeResolution: false,
        //             useVNCHandshake: false
        //         });
        //         console.log(this.wmks);
        //     }
        // );
    }

    destroy() {
        this.loading = true;
        this.service.deleteGamespace(this.game.id)
        .finally(() => this.loading = false)
        .subscribe(
            (result) => {
            },
            (err) => {
                this.onError(err);
            },
            () => {
                this.loading = false;
            }
        );
    }

    collaborate() {
        this.collaborationVisible = !this.collaborationVisible;
        if (this.collaborationVisible)
            this.messageCount = 0;
    }

    redirect() {
        this.router.navigate(['/home']);
    }
    // parseConsoleAnchors(html : string) {
    //     let result = '';
    //     let pattern = /<a href='console'>(.+)<\/a>/;
    //     html.replace(pattern, function(m, t) {
    //         //lookup t in this.summary.vms
    //         let id = '';
    //         for (let i = 0; i < this.summary.vms; i++) {
    //             if (this.summary.vms[i].name == t) {
    //                 id = this.summary.vms[i].id;
    //                 break;
    //             }
    //         }

    //         return result;
    //     });
    // }

    onError(err) {
        let text = JSON.parse(err.text());
        this.errors.push(text);
        console.debug(text);
    }

}