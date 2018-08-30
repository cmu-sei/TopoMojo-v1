import { Injectable } from '@angular/core';
import { UserManager, User, WebStorageStateStore, Log } from 'oidc-client';
import { BehaviorSubject } from 'rxjs';
import { SettingsService } from './settings.service';

@Injectable()
export class AuthService {
    mgr: UserManager;
    authority: string;
    redirectUrl: string;
    lastCall: number;
    oidcUser: User;
    public tokenState$: BehaviorSubject<AuthTokenState> = new BehaviorSubject<AuthTokenState>(AuthTokenState.invalid);

    constructor(
        private settingsSvc: SettingsService
    ) {
        // Log.level = Log.DEBUG;
        // Log.logger = console;
        this.authority = this.settingsSvc.settings.oidc.authority.replace(/https?:\/\//, '');
        this.mgr = new UserManager(this.settingsSvc.settings.oidc);
        this.mgr.events.addUserLoaded(user => { this.onTokenLoaded(user); });
        this.mgr.events.addUserUnloaded(user => { this.onTokenUnloaded(user); });
        this.mgr.events.addAccessTokenExpiring(e => { this.onTokenExpiring(); });
        this.mgr.events.addAccessTokenExpired(e => { this.onTokenExpired(); });
        this.mgr.getUser().then((user) => {
            this.onTokenLoaded(user);
        });
    }

    isAuthenticated(): Promise<boolean> {
        const state = this.tokenState$.getValue();
        if (state === AuthTokenState.valid || state === AuthTokenState.expiring) {
            return Promise.resolve(true);
        }

        return this.mgr.getUser().then(
            (user) => {
                if (!!user) {
                    return Promise.resolve(true);
                }
            }
        );
    }

    access_token(): string {
        return ((this.oidcUser)
            ? this.oidcUser.access_token
            : 'no_token');
    }

    auth_header(): string {
        this.markAction();
        return ((this.oidcUser)
            ? this.oidcUser.token_type + ' ' + this.oidcUser.access_token
            : 'no_token');
    }

    markAction() {
        this.lastCall = Date.now();
    }

    private onTokenLoaded(user) {
        this.oidcUser = user;
        this.tokenState$.next(
            (user)
            ? AuthTokenState.valid
            : AuthTokenState.invalid
        );
    }

    private onTokenUnloaded(user) {
        this.oidcUser = user;
        this.tokenState$.next(AuthTokenState.invalid);
    }

    private onTokenExpiring() {
        if (Date.now() - this.lastCall < 30000) {
            this.silentLogin();
        } else {
            this.tokenState$.next(AuthTokenState.expiring);
        }
    }

    private onTokenExpired() {
        this.tokenState$.next(AuthTokenState.expired);

        // this.logout();
        // give any clean process 5 seconds or so.
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

    externalLoginCallback(url): Promise<User> {
        return this.mgr.signinRedirectCallback(url);
    }

    logout() {
        if (this.oidcUser) {
            this.mgr.signoutRedirect()
            .then(() => {})
            .catch(err => {
                console.log(err.text());
            });
        }
    }

    silentLogin() {
        this.mgr.signinSilent().then(() => { });
    }

    silentLoginCallback(): void {
        this.mgr.signinSilentCallback();
    }

    clearStaleState(): void {
        this.mgr.clearStaleState();
    }

    expireToken(): void {
        this.mgr.removeUser();
    }

    cleanUrl(url) {
        return (url || '')
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
