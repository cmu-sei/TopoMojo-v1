import { SignalR, ISignalRConnection, IConnectionOptions, BroadcastEventListener } from 'ng2-signalr';
import { Injectable } from '@angular/core';
import { AuthService, AuthTokenState } from '../auth/auth.service';
import {Observable, Subscription, Subject} from 'rxjs/Rx';

@Injectable()
export class NotificationService {

    constructor(
        private _signalR: SignalR,
        private auth: AuthService
    ) {
        this.initTokenRefresh();
    }

    private isConnected: boolean = false;
    private connection: ISignalRConnection;
    private subs: Subscription[] = [];
    private key: string;

    actors : any[] = [];

    private presenceSource : Subject<any> = new Subject<any>();
    presenceEvents : Observable<any> = this.presenceSource.asObservable();

    private topoSource : Subject<any> = new Subject<any>();
    topoEvents : Observable<any> = this.topoSource.asObservable();

    private vmSource : Subject<any> = new Subject<any>();
    vmEvents : Observable<any> = this.vmSource.asObservable();

    private chatSource : Subject<any> = new Subject<any>();
    chatEvents : Observable<any> = this.chatSource.asObservable();

    private templateSource : Subject<any> = new Subject<any>();
    templateEvents : Observable<any> = this.templateSource.asObservable();

    private initTokenRefresh() : void {
        this.auth.tokenStatus$.subscribe(
            (state : AuthTokenState) => {
                switch (state) {
                    case AuthTokenState.valid:
                    this.restart();
                    break;

                    case AuthTokenState.invalid:
                    case AuthTokenState.expired:
                    this.stop();
                    break;
                }
            }
        );
    }

    private restart() : void {
        if (this.isConnected) {
            this.stop().then(
                () => {
                    this.log("sigr: leave/stop complete. starting");
                    this.start(this.key);
                }
            );
        }
    }

    start(key: string) : Promise<boolean> {
        this.key = key;
        this.connection = this._signalR.createConnection({ qs: "bearer=" + this.auth.currentUser.access_token });
        this.subs.push(
            this.connection.status.subscribe(
                (status) => {
                    this.isConnected = (status.name === "connected");
                }
            )
        );
        this.log("starting sigr");
        return this.connection.start().then(
            (conn: ISignalRConnection) => {
                this.log("started sigr");
                this.subs.push(
                    this.connection.listenFor("presenceEvent").subscribe(
                        (event : any) => {
                            if (event.action == "PRESENCE.ARRIVED") {
                                this.connection.invoke("Greet", this.key);
                            }
                            this.setActor(event);
                            this.presenceSource.next(event);
                        }
                    ),
                    this.connection.listenFor("topoEvent").subscribe(
                        (msg) => { this.topoSource.next(msg); }
                    ),
                    this.connection.listenFor("vmEvent").subscribe(
                        (msg) => { this.vmSource.next(msg); }
                    ),
                    this.connection.listenFor("chatEvent").subscribe(
                        (msg) => { this.chatSource.next(msg); }
                    ),
                    this.connection.listenFor("templateEvent").subscribe(
                        (msg) => { this.templateSource.next(msg); }
                    )
                );
                this.log("sigr: invoking Listen");
                this.connection.invoke("Listen", this.key).then(
                    (result) => this.log("sigr: invoked Listen"));
                return true;
            }
        );
    }

    stop() : Promise<boolean> {
        if (!this.isConnected)
            return Promise.resolve<boolean>(true);

        this.log("sigr: invoking Leave");
        return this.connection.invoke('Leave', this.key).then(
            (result) => {
                    this.log("sigr: invoked Leave, stopping");
                    this.connection.stop();
                    for (let i = 0; i < this.subs.length; i++) {
                        this.subs[i].unsubscribe();
                    }
                    this.subs = [];
                    this.actors = [];
                    return true;
            }
        ).catch(
            (reason) => {
                this.log("sigr: failed to Leave");
                this.log(reason);
                this.connection.stop();
                for (let i = 0; i < this.subs.length; i++) {
                    this.subs[i].unsubscribe();
                }
                this.subs = [];
                this.actors = [];
                return true;
            }
        );
    }

    sendTemplateEvent(e: string, model: any) : void {
        this.connection.invoke("TemplateMessage", e, model);
    }

    typing() : void {
        this.connection.invoke("Typing", this.key);
    }

    sendChat(text : string) : void {
        this.connection.invoke("Post", this.key, text);
    }

    private setActor(event: any) : void {
        event.actor.isConnected = (event.action == "PRESENCE.ARRIVED" || event.action == "PRESENCE.GREETED");
        let found: boolean = false;
        for (let i = 0; i < this.actors.length; i++) {
            if (this.actors[i].id == event.actor.id) {
                this.actors[i].isConnected = event.actor.isConnected;
                found = true;
                break;
            }
        }

        if (!found) {
            this.actors.push(event.actor);
        }
    }

    log(msg) : void {
        //console.log(msg);
    }

}