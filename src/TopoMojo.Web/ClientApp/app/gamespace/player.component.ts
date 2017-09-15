import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { GamespaceService } from '../api/gamespace.service';
import { GameState } from "../api/api-models";
import { Converter } from 'showdown/dist/showdown';
import { SHOWDOWN_OPTS } from '../shared/constants/ui-params';
import { VmService } from '../vm/vm.service';
import { SettingsService } from '../auth/settings.service';
import { SignalR, BroadcastEventListener, ISignalRConnection } from 'ng2-signalr';
import {Observable, Subscription, Subject} from 'rxjs/Rx';

@Component({
    selector: 'player',
    templateUrl: 'player.component.html',
    styleUrls: [ 'player.component.css']
})
export class PlayerComponent implements OnInit {
    game: GameState;
    errorMessage: string;
    renderedDocument: string;
    launching: boolean;
    loading: boolean = true;
    destroyMsgVisible: boolean;
    destroyPrompt : string = "Confirm deletion of this instance.";
    private converter : Converter;
    private id: number;
    private collaborationVisible: boolean;
    private connection : ISignalRConnection;
    private subs: Subscription[] = [];
    private showOverlay: boolean;
    private appName: string = "";

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service : GamespaceService,
        private vmService : VmService,
        private settings: SettingsService
    ) {
        this.connection = this.route.snapshot.data['connection'];
        this.converter = new Converter(SHOWDOWN_OPTS);
        this.settings.changeLayout({ embedded : true });
        this.appName = this.settings.branding.applicationName;
    }

    ngOnInit() {
        this.loading = true;
        this.id = +this.route.snapshot.paramMap.get('id');
        this.service.getGamespace(this.id)
        .subscribe(
            (result) => {

                // this.service.loadUrl(result.document).subscribe(
                //     (text) => {
                //         //todo: update console urls
                //         //todo: enforce image size?
                //         let newHtml = this.converter.makeHtml(text);
                //         //newHtml = newHtml.replace(/href="console"/g, 'href="console" target="console"');
                //         this.renderedDocument = newHtml;
                //         //this.status = '';
                //     },
                //     (err) => {
                //         console.log(err);

                //         //this.status = '';
                //         //this.errorMessage = "No document specified";
                //     },
                //     () => {
                //         //console.log("done.");
                //         //this.status = '';
                //     }
                // );

                this.initGame(result);
            },
            (err) => {
                this.errorMessage = err.json().message;
                //console.log(err);
                //this.service.onError(err);
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

        this.connection.start().then(
            (conn: ISignalRConnection) => {
                //this.game = game;
                this.connection = conn;
                this.game.shareCode = this.settings.hostUrl + "/mojo/enlist/" + this.game.shareCode;
                this.subs.push(
                    this.connection.listenFor("destroying").subscribe(
                        (actor : any) => {
                            this.showOverlay = true;
                        }
                    )
                );
                this.subs.push(
                    this.connection.listenFor("vmUpdated").subscribe(
                        (vm: any) => {
                            console.log(vm);
                            this.onVmUpdated(vm);
                        }
                    )
                );
                this.connection.invoke("Listen", this.game.globalId);
                this.loading = false;
            }
        );
    }

    onVmUpdated(vm: any) {
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
        this.settings.changeLayout({ embedded : false });
        this.subs.forEach(
            (sub) => {
                sub.unsubscribe();
            }
        );
        try {
            this.connection.stop();
        }
        catch (ex) { }
    }

    launch() {
        this.loading = true;
        //this.status = "Launching topology instance...";
        this.service.launchGamespace(this.id).subscribe(
            (result) => {
                //this.game = result;
                this.initGame(result);
                //this.status = '';
                //this.launching = false;
            },
            (err) => {
                this.errorMessage = err.json().message;
                //this.service.onError(err);
            },
            () => {
                this.loading = false;
            }
        );
    }

    // //wmks : any;
    // launchConsole(vm) {
    //     //console.log('launch console ' + vm.id);
    //     this.vmService.display(vm.id, vm.name);
    //     //this.vmService.launchPage("/vm/display/" + id);
    //     // this.vmService.ticket(id).subscribe(
    //     //     (result) => {
    //     //         console.log(result);
    //     //         //let test = new WMKS();
    //     //         this.wmks = WMKS.createWMKS('console-canvas-div',{
    //     //             rescale: false,
    //     //             position: 0,
    //     //             changeResolution: false,
    //     //             useVNCHandshake: false
    //     //         });
    //     //         console.log(this.wmks);
    //     //     }
    //     // );
    // }

    destroy() {
        this.loading = true;
        this.service.deleteGamespace(this.game.id).subscribe(
            (result) => {
                this.connection.invoke("Destroying", this.game.globalId);
                this.game = null;
                this.router.navigate(['/']);
            },
            (err) => {
                this.errorMessage = err.json().message;
            },
            () => {
                this.loading = false;
            }
        );
    }

    collaborate() {
        this.collaborationVisible = !this.collaborationVisible;
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
}