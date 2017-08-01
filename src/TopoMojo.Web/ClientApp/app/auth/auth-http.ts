import { Injectable } from '@angular/core';
import { Http, Headers, Response } from '@angular/http';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import 'rxjs/add/operator/map';

@Injectable()
export class AuthHttp {
    api: string;

    constructor(
        private http: Http,
        private router: Router,
        private auth: AuthService
        ) {
            this.api = auth.apiUrl;
        }

    mapData(response : Response) {
        return response.json();
    }

    get(url, opts = {}) {
        this.addAuth(opts);
        return this.http.get(url, opts).map(this.mapData);
    }

    delete(url, opts = {}) {
        this.addAuth(opts);
        return this.http.delete(url, opts).map(this.mapData);
    }

    post(url, data, opts={}) {
        this.addAuth(opts);
        return this.http.post(url, data, opts).map(this.mapData);
    }

    put(url, data, opts={}) {
        this.addAuth(opts);
        return this.http.put(url, data, opts).map(this.mapData);
    }

    mapText(response : Response) {
        return response.text();
    }

    gettext(url, opts = {}) {
        this.addAuth(opts);
        return this.http.get(url, opts).map(this.mapText);
    }

    addAuth(opts: any) {
        if (this.auth.currentUser) {
            this.auth.markAction();
            if (opts.headers == null) opts.headers = new Headers();
            //let scheme = this.auth.currentUser.token_type;
            let token = this.auth.currentUser.access_token;
            //console.log(scheme);
            opts.headers.set("Authorization", `Bearer ${token}`);
        }
    }

    appendAuth() : string {
        return "?jwt=" + this.auth.currentUser.access_token;
    }

    onError(err : Response) : string {
        console.error(err);
        // if (err.status === 401) {
        //     this.router.navigate(['/login'], { queryParams: { url: this.router.url }});
        // }
        return err.statusText + ' ' + err.text;
    }

    encodeKVP(key : string, value: string) {
        return encodeURIComponent(key) + "=" + encodeURIComponent(value);
    }
    queryStringify(obj : object, prefix? : string) {
        var keys = (obj) ? Object.keys(obj) : [];
        if (keys.length > 0) {
            var segments = [];
            for (var i = 0; i < keys.length; i++) {
                let prop = obj[keys[i]];
                if (prop !== undefined) {
                    if (Array.isArray(prop)) {
                        prop.forEach(element => {
                            segments.push(this.encodeKVP(keys[i], element));
                        });
                    } else {
                        segments.push(this.encodeKVP(keys[i], prop));
                    }
                }
            }
            return (prefix||'') + segments.join('&');
        }
        return "";
    }
}