import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { AuthService, AuthTokenState, UserProfile } from './auth.service';
import { Observable, Subject } from "rxjs";
import { SettingsService } from './settings.service';

@Injectable()
export class NotificationService {

    constructor(
        private auth: AuthService,
        private settingSvc: SettingsService
    ) {
        this.initTokenRefresh();
    }
    private key: string;
    private debug: boolean = false;
    private online: boolean = false;
    private connection: HubConnection;

    actors : Array<Actor> = new Array<Actor>();

    private globalSource : Subject<any> = new Subject<any>();
    globalEvents : Observable<any> = this.globalSource.asObservable();

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

    private gameSource : Subject<any> = new Subject<any>();
    gameEvents : Observable<any> = this.gameSource.asObservable();

    private initTokenRefresh() : void {
        this.auth.profile$.subscribe(
            (profile : UserProfile) => {
                switch (profile.state) {
                    case AuthTokenState.valid:
                        this.restart();
                        break;

                    case AuthTokenState.invalid:
                    case AuthTokenState.expired:
                        this.stop();
                        this.key = null;
                        break;
                }
            }
        );
    }

    private restart() : void {
        if (this.online) {
            this.stop().then(
                () => {
                    this.log("sigr: leave/stop complete. starting");
                    if (this.key)
                        this.start(this.key);
                }
            );
        }
    }

    start(key: string) : Promise<boolean> {
        this.key = key;
        this.connection = new HubConnectionBuilder()
            .withUrl(`${this.settingSvc.settings.urls.apiUrl}/hub?bearer=${this.auth.currentUser.access_token}`).build();
        this.log("starting sigr");
        return this.connection.start()
            .catch(err => { console.error(err)})
            .then(() => {
                this.log("started sigr");
                this.online = true;
                this.log(this.auth.currentUser.profile);
                this.setActor({
                    action: "PRESENCE.ARRIVED",
                    actor: {
                        id: this.auth.currentUser.profile.id,
                        name: this.auth.currentUser.profile.name,
                        online: true
                    }
                });

                this.connection.on("presenceEvent",
                    (event : TopoEvent) => {
                        if (event.actor.id == this.getMyId())
                            return;

                        if (event.action == "PRESENCE.ARRIVED") {
                            this.connection.invoke("Greet", this.key);
                        }

                        this.setActor(event);
                        this.presenceSource.next(event);
                    }
                );
                this.connection.on("topoEvent",
                    (msg) => { this.topoSource.next(msg); }
                );
                this.connection.on("vmEvent",
                    (msg) => { this.vmSource.next(msg); }
                );
                this.connection.on("gameEvent",
                    (msg) => { this.gameSource.next(msg); }
                );
                this.connection.on("chatEvent",
                    (msg) => {
                        this.setChatActor(msg);
                        this.chatSource.next(msg);
                    }
                );
                this.connection.on("templateEvent",
                    (msg) => { this.templateSource.next(msg); }
                );
                this.connection.on("globalEvent",
                    (msg) => { this.globalSource.next(msg); }
                );
                this.log("sigr: invoking Listen");
                this.connection.invoke("Listen", this.key).then(
                    () => this.log("sigr: invoked Listen"));
                return true;
            });
    }

    getMyId() : string {
        return this.auth.currentUser.profile.id;
    }

    stop() : Promise<boolean> {
        if (!this.online || !this.key)
            return Promise.resolve<boolean>(true);

        this.log("sigr: invoking Leave");
        return this.connection.invoke('Leave', this.key).then(
            () => {
                this.log("sigr: invoked Leave, stopping");
                this.connection.stop();
                this.actors = [];
                this.online = false;
                return true;
            }
        ).catch(
            (reason) => {
                this.log("sigr: failed to Leave");
                this.log(reason);
                this.connection.stop();
                this.actors = [];
                this.online = false;
                return true;
            }
        );
    }

    sendTemplateEvent(e: string, model: any) : void {
        this.connection.invoke("TemplateMessage", e, model);
    }

    typing(v: boolean) : void {
        this.connection.invoke("Typing", this.key, v);
    }

    sendChat(text : string) : void {
        this.connection.invoke("Post", this.key, text);
    }

    private setActor(event: TopoEvent) : void {
        this.log(this.actors);
        event.actor.online = (event.action == "PRESENCE.ARRIVED" || event.action == "PRESENCE.GREETED");
        let actor = this.actors.find(a => { return a.id == event.actor.id });
        if (actor) {
            actor.online = event.actor.online;
        } else {
            this.actors.push(event.actor);
        }
    }

    private setChatActor(event: TopoEvent): void {

        if (event.actor.id == this.auth.currentUser.profile.id)
            return;

        let actor = this.actors.find(a => { return a.id == event.actor.id });
        if (actor) {
            actor.typing = event.action == "CHAT.TYPING" && !!event.model;
            //console.log(actor);
        }
    }

    log(msg) : void {
        if (this.debug)
            console.log(msg);
    }

}

export interface TopoEvent {
    action: string;
    actor: Actor;
    model?: any;
}

export interface Actor {
    id: string;
    name: string;
    online?: boolean;
    typing? : boolean;
}

export interface ChatMessage {
    id: number;
    actor: string;
    text: string;
    time?: string;
}
