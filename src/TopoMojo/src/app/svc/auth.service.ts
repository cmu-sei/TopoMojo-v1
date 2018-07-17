import { Injectable } from '@angular/core';
import { UserManager, User, WebStorageStateStore } from 'oidc-client';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { SettingsService } from './settings.service';
import { AccountService } from '../api/account.service';

@Injectable()
export class AuthService {
    mgr: UserManager;
    currentUser: User;
    authority: string;
    loginSettings: any;
    redirectUrl: string;
    private userSource: Subject<User> = new Subject<User>();
    public user$: Observable<User> = this.userSource.asObservable();
    private tokenStatus: Subject<AuthTokenState> = new Subject<AuthTokenState>();
    tokenStatus$: Observable<AuthTokenState> = this.tokenStatus.asObservable();
    public profile$ : BehaviorSubject<UserProfile>;
    private profile: UserProfile;
    lastCall : number;

    constructor(
        private accountSvc : AccountService,
        private settingsSvc: SettingsService
    ) {
        // Log.level = Log.DEBUG;
        // Log.logger = console;
        this.profile = { state: AuthTokenState.invalid };
        this.profile$ = new BehaviorSubject<UserProfile>(this.profile);
        this.authority = this.settingsSvc.settings.oidc.authority.replace(/https?:\/\//,"") || "External";
        this.mgr = new UserManager(this.settingsSvc.settings.oidc);
        this.mgr.events.addUserLoaded(user => { this.onTokenLoaded(user); });
        this.mgr.events.addUserUnloaded(user => { this.onTokenUnloaded(user); });
        this.mgr.events.addAccessTokenExpiring(e => { this.onTokenExpiring(e); });
        this.mgr.events.addAccessTokenExpired(e => { this.onTokenExpired(e); });
        this.mgr.getUser().then((user) => {
            if (user) {
                this.onTokenLoaded(user);
            }
        });
    }

    init() {
        //this.localmgr.init();
        this.mgr.getUser().then(user => {
            if (user) this.onTokenLoaded(user);
        })
    }

    // isAuthenticated() : Promise<boolean> {
    //     return Promise.resolve(!!this.currentUser);
    // }

    // init() {
    //     this.mgr.getUser().then((user) => {
    //         if (user) {
    //             this.onTokenLoaded(user);
    //         }
    //     });
    // }

    isAuthenticated() : Promise<boolean> {
        if (!!this.currentUser)
            return Promise.resolve(true);

        return this.mgr.getUser().then(
            (user) => {
                if (!!user) {
                    return Promise.resolve(true);
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
        if (user && user.sub) {
            this.profile.id = user.sub;
            this.profile.name = user.profile.name;
            this.profile.state = AuthTokenState.valid;
        }
        this.profile$.next(this.profile);

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

    private onTokenExpiring(ev) {
        console.log(ev);
        if (Date.now() - this.lastCall < 30000)
            this.refreshToken();
        else {
            this.profile.state = AuthTokenState.expiring;
            this.profile$.next(this.profile);
            this.tokenStatus.next(AuthTokenState.expiring);
        }
    }

    private onTokenExpired(ev) {
        console.log(ev);
        this.profile.state = AuthTokenState.expired;
        this.profile$.next(this.profile);

        this.tokenStatus.next(AuthTokenState.expired);

        //give any clean process 10 seconds or so.
        setTimeout(() => {
            this.mgr.removeUser();
        }, 10000);
    }

    localLogin(method: string, creds: any) {
        return this.accountSvc.submit(method, creds)
        // .map(
        //     (token) => {
        //         this.localmgr.addUser(token);
        //         return token;
        //     }
        // )
        .toPromise();
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
            this.mgr.signoutRedirect().then(() => {
                console.log("initiated external logout");
            }).catch(err => {
                console.log(err.text());
            });
        }
    }

    sendAuthCode(u : string) {
        return this.accountSvc.confirmAccount({ username: u });
    }

    refreshToken() {
        this.mgr.signinSilent().then(() => { });
    }

    isAdmin() {
        return this.currentUser &&
            (
                this.currentUser.profile.isAdmin ||
                this.currentUser.profile.role == "administrator"
            )
        ;
    }

    cleanUrl(url) {
        return url
            .replace(/[?&]auth-hint=[^&]*/, '')
            .replace(/[?&]contentId=[^&]*/, '')
            .replace(/[?&]profileId=[^&]*/, '');
    }
}

export interface UserProfile {
    id?: string,
    name?: string,
    isAdmin?: boolean,
    state?: AuthTokenState
}

export enum AuthTokenState {
    valid = <any>'valid',
    invalid = <any>'invalid',
    expiring = <any>'expiring',
    expired = <any>'expired'
}
