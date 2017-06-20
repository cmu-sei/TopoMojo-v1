import { Injectable } from '@angular/core';
import { Http, Headers, RequestOptions, Response } from '@angular/http';
import { UserManager, UserManagerSettings, WebStorageStateStore, Log, MetadataService, User } from 'oidc-client';
import { Observable, Subject } from 'rxjs/Rx';

@Injectable()
export class AuthService {
    mgr: UserManager;
    currentUser: User;
    loggedIn: boolean = false;
    private userSource: Subject<User> = new Subject<User>();
    public user$: Observable<User> = this.userSource.asObservable();
    apiUrl : string;
    allowExternalLogin: boolean;
    redirectUrl: string;
    storageKey: string = "local-auth-user";

    constructor(
        private http: Http
    ) {
        Log.level = Log.DEBUG;
        Log.logger = console;
        this.apiUrl = window['clientAuthenticationSettings']['apiUrl'];
        this.allowExternalLogin = window['clientAuthenticationSettings']['authority'] != '';

        this.mgr = new UserManager(window['clientAuthenticationSettings']);
        this.mgr.events.addUserUnloaded(user => {
            console.log('authService: user unloaded');
            this.loadUser(user);
        });

        this.mgr.events.addUserLoaded(user => {
            console.log('authService: user loaded');
            this.loadUser(user);
        });

        this.mgr.events.addUserSignedOut(e => {
            console.log('authService: user signed out');
            this.removeUser();
            this.loadUser(null);
        });

        this.mgr.events.addAccessTokenExpiring(e => {
            //this.loadUser(null);
            console.log('authService: access token expiring...');
        });

        this.mgr.events.addAccessTokenExpired(user => {
            console.log('authService: access token expired');
            this.removeUser();
            this.loadUser(null);
            this.clearState();
        });
    }

    init() {
        this.getUser();
    }

    isAuthenticated() : Promise<boolean> {
        if (this.currentUser) {
            return Promise.resolve(true);
        }

        return Promise.resolve(this.getUser())
            .then(user => {
                return (this.currentUser != null);
            });
    }

    loadUser(user) {
        this.loggedIn = (user !== null);
        console.log("loadUser() = " + this.loggedIn);
        this.currentUser = user;
        this.userSource.next(user);
    }

    clearState() {
        this.mgr.clearStaleState().then(function () {
            console.log("clearStateState success");
        }).catch(function (e) {
            console.log("clearStateState error", e.message);
        });
    }

    getUser() : Promise<User> {
        return this.mgr.getUser().then((user) => {
            // if no external user, check for local user
            if (!user)
                user = JSON.parse(localStorage.getItem(this.storageKey));

            this.loadUser(user);
            return user;
        }).catch(function (err) {
            console.log(err);
        });
    }

    removeUser() {
        this.mgr.removeUser().then(() => {
            localStorage.removeItem(this.storageKey);
            console.log("user removed");
        }).catch(function (err) {
            console.log(err);
        });
    }

    localLogin(type, creds) {
        return this.http.post('/api/account/' + type, creds)
        .toPromise().then(response => {
            let user = response.json();
            // this.mgr.signinLocal(user).then(user => {
            //     console.log(user);
            //     this.loadUser(user);
            //     return user;
            // });
            localStorage.setItem(this.storageKey, JSON.stringify(user));
            return response;
        });
    }

    logout() {
        localStorage.removeItem(this.storageKey);
        this.getUser().then(user => {
            if (user)
                this.initiateLogout();
        });
    }

    confirm(u : string) {
        return this.http.post('/api/account/confirm', { username: u });
    }

    initiateLogin(url) {
        this.mgr.signinRedirect({ state: url }).then(function () {
            console.log("signinRedirect done");
        }).catch(function (err) {
            console.log(err);
        });
    }

    validateLogin(url) : Promise<User> {
        return this.mgr.signinRedirectCallback(url);
    }

    initiateLogout() {
        this.mgr.signoutRedirect().then(function (resp) {
            console.log("signed out", resp);
            setTimeout(5000, () => {
                console.log("testing to see if fired...");
            })
        }).catch(function (err) {
            console.log(err);
        });
    };

    finalizeLogout() {
        this.mgr.signoutRedirectCallback().then(function (resp) {
            console.log("signed out", resp);
        }).catch(function (err) {
            console.log(err);
        });
    };

    //todo: implement this!
    isAdmin() {
        return true;
    }
}

// const settings: UserManagerSettings = {
//     authority: 'http://localhost:5000',
//     client_id: 'sketch-browser',
//     redirect_uri: 'http://localhost:5002/auth',
//     post_logout_redirect_uri: 'http://localhost:5002',
//     response_type: 'id_token token',
//     scope: 'openid profile sketch-api',
//     automaticSilentRenew: false,
//     silent_redirect_uri: 'http://localhost:5000',
//     //silentRequestTimeout:10000,
//     filterProtocolClaims: true,
//     loadUserInfo: true
//     //userStore: new WebStorageStateStore({})
// };