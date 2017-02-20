import { Injectable } from '@angular/core';
import { AuthHttp } from '../core/auth-http';

@Injectable()
export class VmService {

    constructor(private http: AuthHttp) { }

    refresh(id) {
        return this.http.get('/api/vm/refresh/'+id);
    }

    initialize(id) {
        return this.http.post('/api/vm/initialize/'+id, {});
    }

    deploy(id) {
        return this.http.post('/api/vm/deploy/'+id, {});
    }

    start(id) {
        return this.http.post('/api/vm/start/'+id, {});
    }

    stop(id) {
        return this.http.post('/api/vm/stop/'+id, {});
    }

    revert(id) {
        return this.http.post('/api/vm/revert/'+id, {});
    }

    save(id) {
        return this.http.post('/api/vm/save/'+id, {});
    }

    delete(id) {
        return this.http.delete('/api/vm/delete/'+id);
    }

    answer(vid, qid, cid) {
        return this.http.post(`/api/vm/answer/${vid}/${qid}/${cid}`, {});
    }

    display(id) {
        this.launchPage('/vm/display/'+id);
    }

    pageRefs: any = {};
    public launchPage(url) {
        if ( typeof this.pageRefs[url] == 'undefined' || this.pageRefs[url].closed )
        {
            this.pageRefs[url] = window.open(url);
        } else {
            this.pageRefs[url].focus()
        }
    }

    public onError(err) {
        this.http.onError(err);
    }
}