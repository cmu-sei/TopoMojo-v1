import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Http, Response } from '@angular/http';
import { AuthHttp } from './auth-http';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { JwtHelper } from './auth-jwt';

import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/toPromise';

export class UserProfile {
    userName : string;
    userId : string;
    isLoggedIn : boolean;
    isAdmin : boolean;
}

@Injectable()
export class AuthService {
    bearerKey: string = 'Bearer';
    redirectUrl: string = '/';
    private profileSource: Subject<UserProfile> = new Subject<UserProfile>();
    profile$ : Observable<UserProfile> = this.profileSource.asObservable();
    private tokenUpdateSource: Subject<any> = new Subject<any>();
    private tokenUpdater : Observable<any> = this.tokenUpdateSource.asObservable();

    constructor(
        private http: AuthHttp,
        private router: Router
        ) {
            this.tokenUpdater.subscribe(token => {
                this.renew();
            })
        }

    init() {
        this.loadProfile();
    }

    login(u, p) {
        return this.http.post('/api/account/login', { u: u, p: p})
        .toPromise().then(bearer => {
            localStorage.setItem(this.bearerKey, bearer.access_token);
            this.loadProfile();
            return bearer;
        });
    }

    renew() {
        return this.http.get('/api/account/renew')
        .toPromise().then(bearer => {
            localStorage.setItem(this.bearerKey, bearer.access_token);
            this.loadProfile();
            console.log("## renewed auth token ##");
            return bearer;
        }, (err) => { console.error(err); });
    }

    logout() {
        return this.http.post('/api/account/logout', null)
        .toPromise().then(result => {
            localStorage.removeItem(this.bearerKey);
            this.loadProfile();
            this.redirectUrl = '/';
            return true;
        }, (err) => { console.error(err); });
    }

    reset(account) {
        return this.http.post('/api/account/forgotpassword', { email: account})
        .toPromise();
    }

    isAdmin() : boolean {
        //console.log('isAdmin#AuthService');

        let jwt: JwtHelper = this.getJwt();
        return (jwt) ? jwt.isAdmin() : false;
    }

    isAuthenticated(url) : boolean {
        let isValid : boolean = false;
        this.redirectUrl = url;

        let jwt: JwtHelper = this.getJwt();
        if (jwt !== null) {
            isValid = !jwt.isExpired();
            if (!isValid) {
                this.logout();
            } else {
                if (jwt.isExpiring()) {
                    this.tokenUpdateSource.next('renew');
                }
            }
        }
        //console.log('isAuthenticated#AuthService ' + isValid);
        return isValid;
    }

    private loadProfile() {
        let p = new UserProfile();
        let jwt = this.getJwt();
        if (jwt != null) {
            let claims = jwt.token.claims;
            p.userId = claims.sub;
            p.userName = claims.tmnm;
            p.isAdmin = (claims.tmad === 'True');
            p.isLoggedIn = (claims.sub) && (!jwt.isExpired());
        }
        //console.log(p);
        this.profileSource.next(p);
    }

    private getJwt() : JwtHelper {
        let bearerToken = localStorage.getItem(this.bearerKey);
        return (bearerToken !== null)
            ?  new JwtHelper(bearerToken)
            : null;
    }

    onError(err) {
        this.http.onError(err);
    }
}