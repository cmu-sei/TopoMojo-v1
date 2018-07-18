import { Injectable } from '@angular/core';
import { UserManager, User, WebStorageStateStore, Log } from 'oidc-client';
import { BehaviorSubject } from 'rxjs';
import { SettingsService } from './settings.service';

@Injectable()
export class AuthService {
    mgr: UserManager;
    authority: string;
    redirectUrl: string;
    lastCall : number;
    profile: UserProfile;
    public profile$ : BehaviorSubject<UserProfile>;
    currentUser: User;
    public oidcUser$ : BehaviorSubject<User>;

    constructor(
        private settingsSvc: SettingsService
    ) {
        // Log.level = Log.DEBUG;
        // Log.logger = console;
        this.profile = { state: AuthTokenState.invalid };
        this.profile$ = new BehaviorSubject<UserProfile>(this.profile);
        this.oidcUser$ = new BehaviorSubject<User>(null);
        this.authority = this.settingsSvc.settings.oidc.authority.replace(/https?:\/\//,"") || "External";
        this.mgr = new UserManager(this.settingsSvc.settings.oidc);
        this.mgr.events.addUserLoaded(user => { this.onTokenLoaded(user); });
        this.mgr.events.addUserUnloaded(user => { this.onTokenUnloaded(user); });
        this.mgr.events.addAccessTokenExpiring(e => { this.onTokenExpiring(e); });
        this.mgr.events.addAccessTokenExpired(e => { this.onTokenExpired(e); });
        this.mgr.getUser().then((user) => {
            this.onTokenLoaded(user);
        });
    }

    isAuthenticated() : Promise<boolean> {
        if (!!this.currentUser)
            return Promise.resolve(
                this.profile.state == AuthTokenState.valid
                || this.profile.state == AuthTokenState.expiring
            );

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
        if (user && user.profile.sub) {
            this.profile.id = user.profile.sub;
            this.profile.name = user.profile.name;
            this.profile.state = AuthTokenState.valid;
            this.profile.isAdmin = true;
        }
        this.profile$.next(this.profile);
        this.currentUser = user;
        this.oidcUser$.next(this.currentUser);
    }

    private onTokenUnloaded(user) {
        this.profile = { state: AuthTokenState.invalid };
        this.profile$.next(this.profile);
        this.currentUser = user;
        this.oidcUser$.next(this.currentUser);
    }

    private onTokenExpiring(ev) {
        console.log(ev);
        if (Date.now() - this.lastCall < 30000)
            this.refreshToken();
        else {
            this.profile.state = AuthTokenState.expiring;
            this.profile$.next(this.profile);
        }
    }

    private onTokenExpired(ev) {
        console.log(ev);
        this.profile.state = AuthTokenState.expired;
        this.profile$.next(this.profile);
        //this.logout();
        //give any clean process 5 seconds or so.
        setTimeout(() => {
            this.mgr.removeUser();
        }, 5000);
    }

    externalLogin(url) {
        this.mgr.signinRedirect({ state: url })
            .then(() => {})
            .catch(err => {
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

    refreshToken() {
        this.mgr.signinSilent().then(() => { });
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
