import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@aspnet/signalr';
import { AuthService, AuthTokenState } from './auth.service';
import { Observable, Subject } from "rxjs";
import { SettingsService } from './settings.service';
import { UserService } from './user.service';
import { Profile } from '../api/gen/models';

@Injectable()
export class NotificationService {

    constructor(
        private authSvc: AuthService,
        private userSvc: UserService,
        private settingSvc: SettingsService
    ) {
        this.initTokenRefresh();
        this.userSvc.profile$.subscribe(
            (p: Profile) => {
                this.profile = p;
            }
        );
    }
    private profile: Profile;
    private key: string;
    private debug: boolean = false;
    private connected: boolean = false;
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
        this.authSvc.tokenState$.subscribe(
            (state : AuthTokenState) => {
                switch (state) {
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
        if (this.connected) {
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
            .withUrl(`${this.settingSvc.settings.urls.apiUrl}/hub?bearer=${this.authSvc.access_token()}`).build();
        this.log("starting sigr");
        return this.connection.start()
            .catch(err => { console.error(err)})
            .then(() => {
                this.log("started sigr");
                this.connected = true;

                if (this.profile.globalId) {
                    this.setActor({
                        action: "PRESENCE.ARRIVED",
                        actor: {
                            id: this.profile.globalId,
                            name: this.profile.name,
                            online: true
                        }
                    });
                }

                this.connection.on("presenceEvent",
                    (event : TopoEvent) => {
                        if (event.actor.id == this.profile.globalId)
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

    stop() : Promise<boolean> {
        if (!this.connected || !this.key)
            return Promise.resolve<boolean>(true);

        this.log("sigr: invoking Leave");
        return this.connection.invoke('Leave', this.key).then(
            () => {
                this.log("sigr: invoked Leave, stopping");
                this.connection.stop();
                this.actors = [];
                this.connected = false;
                return true;
            }
        ).catch(
            (reason) => {
                this.log("sigr: failed to Leave");
                this.log(reason);
                this.connection.stop();
                this.actors = [];
                this.connected = false;
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

        if (event.actor.id == this.profile.globalId)
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
