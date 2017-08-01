import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { UserManager, UserManagerSettings, WebStorageStateStore, Log, MetadataService, User } from 'oidc-client';
import { Observable, Subject } from 'rxjs/Rx';
import { LocalUserService } from './localuser.service';
import { SettingsService } from './settings.service';

@Injectable()
export class AuthService {
    mgr: UserManager;
    localmgr: LocalUserService;
    currentUser: User;
    loggedIn: boolean = false;
    apiUrl : string;
    allowExternalLogin: boolean;
    redirectUrl: string;
    private userSource: Subject<User> = new Subject<User>();
    public user$: Observable<User> = this.userSource.asObservable();
    private tokenStatus: Subject<string> = new Subject<string>();
    tokenStatus$: Observable<string> = this.tokenStatus.asObservable();
    lastCall : number;

    constructor(
        private http: Http,
        private settings: SettingsService
    ) {
        // Log.level = Log.DEBUG;
        // Log.logger = console;
        this.apiUrl = this.settings.urls.apiUrl;
        this.allowExternalLogin = this.settings.oidc.authority != '';

        this.mgr = new UserManager(this.settings.oidc);
        this.mgr.events.addUserLoaded(user => { this.onTokenLoaded(user); });
        this.mgr.events.addUserUnloaded(user => { this.onTokenUnloaded(user); });
        this.mgr.events.addAccessTokenExpiring(e => { this.onTokenExpiring(e); });
        this.mgr.events.addAccessTokenExpired(e => { this.onTokenExpired(e); });

        this.localmgr = new LocalUserService();
        this.localmgr.events.addTokenLoaded(user => { this.onTokenLoaded(user); });
        this.localmgr.events.addTokenUnloaded(user => { this.onTokenUnloaded(user); });
        this.localmgr.events.addTokenExpiring(e => { this.onTokenExpiring(e); });
        this.localmgr.events.addTokenExpired(e => { this.onTokenExpired(e); });

        this.init();
    }

    init() {
        this.localmgr.init();
        this.mgr.getUser().then(user => {
            if (user) this.onTokenLoaded(user);
        })
    }

    isAuthenticated() : Promise<boolean> {
        return Promise.resolve(!!this.currentUser);
    }

    markAction() {
        this.lastCall = Date.now();
    }

    private onTokenLoaded(user) {
        this.currentUser = user;
        this.loggedIn = (user !== null);
        this.userSource.next(user);
        this.tokenStatus.next("valid");
    }

    private onTokenUnloaded(user) {
        this.currentUser = user;
        this.userSource.next(user);
        this.tokenStatus.next("invalid");
    }

    private onTokenExpiring(e) {
        if (Date.now() - this.lastCall < 15000)
            this.refreshToken();
        else
            this.tokenStatus.next("expiring");
    }

    private onTokenExpired(e) {
        this.tokenStatus.next("expired");
        if (this.localmgr.getToken())
            this.localmgr.removeUser();
        else
            this.mgr.removeUser();
    }

    localLogin(type, creds) {
        return this.http.post('/api/account/' + type, creds)
        .toPromise().then(response => {
            let user = response.json();
            this.localmgr.addUser(user);
            return response;
        });
    }

    externalLogin(url) {
        this.mgr.signinRedirect({ state: url }).then(function () {
            //console.log("signinRedirect done");
        }).catch(function (err) {
            console.log(err);
        });
    }

    externalLoginCallback(url) : Promise<User> {
        return this.mgr.signinRedirectCallback(url);
    }

    logout() {
        if (this.currentUser) {

            if (this.localmgr.getToken()) {
                this.localmgr.removeUser();
            }
            else {
                this.mgr.signoutRedirect().then(resp => {
                    console.log("initiated external logout");
                }).catch(err => {
                    console.log(err.text());
                });
            }
        }
    }

    sendAuthCode(u : string) {
        return this.http.post('/api/account/confirm', { username: u });
    }

    refreshToken() {
        if (this.localmgr.getToken()) {
            let opts : RequestOptions = new RequestOptions();
            opts.headers = new Headers();
            opts.headers.set("Authorization", "Bearer " + this.currentUser.access_token);
            this.http.get('/api/account/refresh', opts).subscribe(result => {
                this.localmgr.addUser(result.json());
            });
        } else {
            this.mgr.signinSilent().then(() => {

            });
        }

    }

    isAdmin() {
        return (this.currentUser && this.currentUser.profile.isAdmin);
    }
}
