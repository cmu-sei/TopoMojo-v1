import { Injectable } from '@angular/core';
import { AuthHttp } from '../auth/auth-http';
import { SettingsService } from '../auth/settings.service';

@Injectable()
export class VmService {

    constructor(
        private http: AuthHttp,
        private settings: SettingsService
    ) { }

    private url() {
        return this.settings.urls.apiUrl;
    }

    refresh(id) {
        return this.http.get(this.url() + '/vm/refresh/'+id);
    }

    initialize(id) {
        return this.http.post(this.url() + '/vm/initialize/'+id, {});
    }

    deploy(id) {
        return this.http.post(this.url() + '/vm/deploy/'+id, {});
    }

    start(id) {
        return this.http.post(this.url() + '/vm/start/'+id, {});
    }

    stop(id) {
        return this.http.post(this.url() + '/vm/stop/'+id, {});
    }

    revert(id) {
        return this.http.post(this.url() + '/vm/revert/'+id, {});
    }

    save(id) {
        return this.http.post(this.url() + '/vm/save/'+id, {});
    }

    delete(id) {
        return this.http.delete(this.url() + '/vm/delete/'+id);
    }

    answer(vid, qid, cid) {
        return this.http.post(this.url() + `/vm/answer/${vid}/${qid}/${cid}`, {});
    }

    ticket(id) {
        return this.http.get(this.url() + '/vm/ticket/' + id);
    }

    display(id, name) {
        this.launchPage('/console/' + id + "/" + name.match(/[^#]*/)[0]);
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