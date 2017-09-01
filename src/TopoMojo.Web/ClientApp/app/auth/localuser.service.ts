import { Injectable } from '@angular/core';

//@Injectable()
export class LocalUserService {

    constructor() {
    }

    storageKey: string = "sketch.auth.jwt.";
    timer: any;
    token: any;
    events: LocalUserEvents = new LocalUserEvents();
    headsup: number = 60;

    init() : void {
        this.storageKey += window.navigator.userAgent.substring(window.navigator.userAgent.lastIndexOf(' '));
        console.log("localtoken: loading token from storage");
        let token = localStorage.getItem(this.storageKey);
        if (!!token) {
            this.token = JSON.parse(token);
            this.startTimer(this.ttn());
            this.events.onTokenLoaded(this.token);
        }
    }

    addUser(token) : void {
        if (token) {
            console.log("localtoken: added local token");
            token.expires_at = (Date.now() / 1000) + token.expires_in;
            localStorage.setItem(this.storageKey, JSON.stringify(token));
            this.token = token;
            this.startTimer(this.ttn());
            this.events.onTokenLoaded(this.token);
        }
    }

    removeUser() : void {
        console.log("localtoken: removed local token");
        localStorage.removeItem(this.storageKey);
        this.killTimer();
        this.token = null;
        this.events.onTokenUnloaded(this.token);
    }

    getToken() : any {
        return this.token;
    }

    private startTimer(duration) {
        this.killTimer();
        console.log("localtoken: starting timer for " + duration + " seconds");
        this.timer = window.setTimeout(() => { this.onTimer(); }, duration * 1000);
        window["localUserManagerTimer"] = this.timer;
        console.log('storing timer ' + window["localUserManagerTimer"]);

    }

    private killTimer() {
        if (window["localUserManagerTimer"]) {
            console.log('killing timer ' + window["localUserManagerTimer"]);
            window.clearTimeout(window["localUserManagerTimer"]);
            window["localUserManagerTimer"] = null;
        }
        if (this.timer)
            window.clearTimeout(this.timer);
    }

    private ttn() {
        return Math.max(this.ttl() - this.headsup, 0);
    }

    private ttl() {
        return Math.max(this.token.expires_at - (Date.now() / 1000), 0);
    }

    private onTimer() {
        console.log("localtoken: timeout reached");
        if (this.ttl()) {
            this.events.onTokenExpiring();
            this.startTimer(this.ttl());
        } else {
            this.events.onTokenExpired();
        }

    }

}

export class LocalUserEvents {
    expiringHandler : any;
    expiredHandler :any;
    loadedHandler: any;
    unloadedHandler: any;

    onTokenLoaded(token) {
        if (this.loadedHandler) this.loadedHandler(token);
    }
    onTokenUnloaded(token) {
        if (this.unloadedHandler) this.unloadedHandler(token);
    }
    onTokenExpiring() {
        if (this.expiringHandler) this.expiringHandler();
    }
    onTokenExpired() {
        if (this.expiredHandler) this.expiredHandler();
    }

    addTokenExpiring(handler) {
        this.expiringHandler = handler;
    }
    addTokenExpired(handler) {
        this.expiredHandler = handler;
    }
    addTokenLoaded(handler) {
        this.loadedHandler = handler;
    }
    addTokenUnloaded(handler) {
        this.unloadedHandler = handler;
    }
}