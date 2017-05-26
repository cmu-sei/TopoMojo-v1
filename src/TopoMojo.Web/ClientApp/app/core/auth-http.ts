import { Injectable } from '@angular/core';
import { Http, Headers, Response } from '@angular/http';
import { Router } from '@angular/router';

@Injectable()
export class AuthHttp {
    authKey : string = 'Bearer';

    constructor(
        private http: Http,
        private router: Router,
        ) { }

    mapData(response : Response) {
        return response.json();
    }

    mapText(response : Response) {
        return response.text();
    }

    gettext(url, opts = {}) {
        this.addAuth(opts);
        return this.http.get(url, opts).map(this.mapText);
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

    addAuth(opts: any) {
        let token = localStorage.getItem(this.authKey);
        if (token) {
            if (opts.headers == null) opts.headers = new Headers();
            opts.headers.set("Authorization", `Bearer ${token}`);
        }
    }

    onError(err : Response) : string {
        console.error(err);
        if (err.status === 401) {
            this.router.navigate(['/login'], { queryParams: { url: this.router.url }});
        }
        return err.statusText + ' ' + err.text;
    }

}