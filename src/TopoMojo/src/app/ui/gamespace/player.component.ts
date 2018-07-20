import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { GamespaceService } from '../../api/gamespace.service';
import { GameState, VmState } from '../../api/gen/models';
import { NotificationService } from '../../svc/notification.service';
import { Converter } from 'showdown/dist/showdown';
import { SettingsService, SHOWDOWN_OPTS } from '../../svc/settings.service';
import { Subscription } from 'rxjs';
import { LayoutService } from '../../svc/layout.service';

@Component({
    selector: 'app-player',
    templateUrl: 'player.component.html',
    styleUrls: [ 'player.component.css']
})
export class PlayerComponent implements OnInit, OnDestroy {
    game: GameState;
    errors: any[] = [];
    markdownDoc: string;
    renderedDoc: string;
    docMissing: boolean;
    launching: boolean;
    loading = true;
    destroyMsgVisible: boolean;
    messageCount = 0;
    collaborationVisible = false;
    appName: string;
    private converter: Converter;
    private id: number;
    private subs: Subscription[] = [];
    showOverlay: boolean;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service: GamespaceService,
        private notifier: NotificationService,
        private settingsSvc: SettingsService,
        private layoutSvc: LayoutService
        ) {
        this.converter = new Converter(SHOWDOWN_OPTS);
        this.settingsSvc.changeLayout({ embedded : true });
        this.appName = this.settingsSvc.settings.branding.applicationName || 'TopoMojo';
    }

    ngOnInit() {
        this.loading = true;
        this.id = +this.route.snapshot.paramMap.get('id');
        this.service.getGamespace(this.id).subscribe(
            (result: GameState) => {
                this.service.getText(result.topologyDocument + '?ts=' + Date.now())
                    .subscribe(
                        (text) => {
                            // todo: update console urls
                            // todo: enforce image size?
                            // let newHtml = this.converter.makeHtml(text);
                            // newHtml = newHtml.replace(/href="console"/g, 'href="console" target="console"');
                            // this.renderedDocument = newHtml;
                            // this.status = '';
                            this.markdownDoc = text;
                        },
                        () => this.render()
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

        if (!this.game.globalId) {
            return;
        }

        this.game.shareCode = this.settingsSvc.hostUrl + '/mojo/enlist/' + this.game.shareCode;
        this.subs.push(
            this.notifier.gameEvents.subscribe(
                (event) => {
                    switch (event.action) {
                        case 'GAME.OVER':
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
                        case 'CHAT.MESSAGE':
                        if (!this.collaborationVisible) {
                            this.messageCount += 1;
                        }
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
        const html = this.converter.makeHtml(this.markdownDoc);

        this.renderedDoc = html;
    }

    onVmUpdated(vm: VmState) {
        this.game.vms.forEach(
            (v, i) => {
                // console.log(v);
                if (v.id === vm.id) {
                    v.isRunning = vm.isRunning;
                    this.game.vms.splice(i, 1, v);
                }
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

        this.layoutSvc.changeLayout({ embedded : false });
    }

    launch() {
        this.loading = true;

        this.service.postGamespaceLaunch(this.id)
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

    // wmks : any;
    launchConsole(vm) {
        // console.log('launch console ' + vm.id);
        this.service.openConsole(vm.id, vm.name);
        // this.vmService.launchPage("/vm/display/" + id);
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
        .subscribe(
            () => {
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
        if (this.collaborationVisible) {
            this.messageCount = 0;
        }
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
        this.errors.push(err.error);
    }

}
