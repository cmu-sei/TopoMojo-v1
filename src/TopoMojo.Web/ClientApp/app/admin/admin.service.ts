import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { AuthHttp } from '../auth/auth-http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';
import { SettingsService } from '../auth/settings.service';

@Injectable()
export class AdminService {

    constructor(
        private http: AuthHttp,
        private settings: SettingsService
        ) { }

    private url() {
        return this.settings.urls.apiUrl;
    }

    roster(search) {
        return this.http.post(this.url() + '/account/roster', search);
    }

    grant(p) {
        return this.http.post(this.url() + '/account/grant', p);
    }

    deny(p) {
        return this.http.post(this.url() + '/account/deny', p);
    }
    promote(m) {
        return this.http.post(this.url() + '/account/addtopouser', m);
    }
    demote(m) {
        return this.http.post(this.url() + '/account/removetopouser', m);
    }
    upload(list) {
        return this.http.post(this.url() + '/account/upload', { list: list});
    }

    onError(err) {
        this.http.onError(err);
    }
}