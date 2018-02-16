import { Injectable } from '@angular/core';
import { UserManager, UserManagerSettings, WebStorageStateStore, Log, MetadataService, User } from 'oidc-client';
import { Observable, Subject } from 'rxjs/Rx';
import { LocalUserService } from './localuser.service';
import { SettingsService } from './settings.service';
import { AccountService } from '../api/account.service';

@Injectable()
export class AuthService {
    mgr: UserManager;
    localmgr: LocalUserService;
    currentUser: User;
    //loggedIn: boolean = false;
    externalLoginName: string;
    loginSettings: any;
    redirectUrl: string;
    private userSource: Subject<User> = new Subject<User>();
    public user$: Observable<User> = this.userSource.asObservable();
    private tokenStatus: Subject<AuthTokenState> = new Subject<AuthTokenState>();
    tokenStatus$: Observable<AuthTokenState> = this.tokenStatus.asObservable();
    lastCall : number;

    constructor(
        private accountSvc : AccountService,
        private settings: SettingsService
    ) {
        // Log.level = Log.DEBUG;
        // Log.logger = console;
        this.loginSettings = this.settings.login;
        this.externalLoginName = this.settings.oidc.name || "External";

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

    // init() {
    //     this.localmgr.init();
    //     this.mgr.getUser().then(user => {
    //         if (user) this.onTokenLoaded(user);
    //     })
    // }

    // isAuthenticated() : Promise<boolean> {
    //     return Promise.resolve(!!this.currentUser);
    // }

    init() {
        this.mgr.getUser().then((user) => {
            if (user) {
                this.onTokenLoaded(user);
            } else {
                this.localmgr.getUser().then((localuser) => {
                    //console.log(localuser);
                    if (localuser) this.onTokenLoaded(localuser);
                });
            }
        });
    }

    isAuthenticated() : Promise<boolean> {
        if (!!this.currentUser)
            return Promise.resolve(true);

        return this.mgr.getUser().then(
            (user) => {
                if (!!user) {
                    return Promise.resolve(true);
                } else {
                    return this.localmgr.getUser().then(
                        (user) => {
                            return Promise.resolve(!!user);
                        }
                    )
                }
            }
        );
    }

    getAuthorizationHeader() : string {
        this.markAction();
        return ((this.currentUser)
            ? this.currentUser.token_type + " " + this.currentUser.access_token
            : "no_token");
    }

    markAction() {
        this.lastCall = Date.now();
    }

    private onTokenLoaded(user) {
        this.currentUser = user;
        //this.loggedIn = (user !== null);
        this.userSource.next(user);
        this.tokenStatus.next(AuthTokenState.valid);
    }

    private onTokenUnloaded(user) {
        this.currentUser = user;
        this.userSource.next(user);
        this.tokenStatus.next(AuthTokenState.invalid);
    }

    private onTokenExpiring(e) {
        if (Date.now() - this.lastCall < 15000)
            this.refreshToken();
        else
            this.tokenStatus.next(AuthTokenState.expiring);
    }

    private onTokenExpired(e) {
        this.tokenStatus.next(AuthTokenState.expired);

        //give any clean process 10 seconds or so.
        setTimeout(() => {
            if (this.localmgr.getToken())
                this.localmgr.removeUser();
            else
                this.mgr.removeUser();
        }, 10000);
    }

    localLogin(method: string, creds: any) {
        return this.accountSvc.submit(method, creds)
        .map(
            (token) => {
                this.localmgr.addUser(token);
                return token;
            }
        ).toPromise();
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
        return this.accountSvc.confirmAccount({ username: u });
    }

    refreshToken() {
        if (this.localmgr.getToken()) {
            this.accountSvc.refreshAccount()
                .subscribe(token => {
                    this.localmgr.addUser(token);
                });
        } else {
            this.mgr.signinSilent().then(() => {

            });
        }

    }

    isAdmin() {
        return (this.currentUser && this.currentUser.profile.isAdmin);
    }

    cleanUrl(url) {
        return url
            .replace(/[?&]auth-hint=[^&]*/, '')
            .replace(/[?&]contentId=[^&]*/, '')
            .replace(/[?&]profileId=[^&]*/, '');
    }
}

export enum AuthTokenState {
    valid = <any>'valid',
    invalid = <any>'invalid',
    expiring = <any>'expiring',
    expired = <any>'expired'
}
