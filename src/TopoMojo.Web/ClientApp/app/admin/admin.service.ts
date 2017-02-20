import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { AuthHttp } from '../core/auth-http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class AdminService {

    constructor(
        private http: AuthHttp
        ) { }

    roster(search) {
        return this.http.post('/api/account/roster', search);
    }

    grant(p) {
        return this.http.post('/api/account/grant', p);
    }

    deny(p) {
        return this.http.post('/api/account/deny', p);
    }
    promote(m) {
        return this.http.post('/api/account/addtopouser', m);
    }
    demote(m) {
        return this.http.post('/api/account/removetopouser', m);
    }
    upload(list) {
        return this.http.post('/api/account/upload', { list: list});
    }

    onError(err) {
        this.http.onError(err);
    }
}