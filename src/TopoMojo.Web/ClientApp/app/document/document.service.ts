import { AuthHttp } from '../auth/auth-http';
import { Injectable } from '@angular/core';
import { SettingsService } from "../auth/settings.service";

@Injectable()
export class DocumentService {

    constructor(
        private http: AuthHttp,
        private settings: SettingsService
        ) {}

    private url() {
        return this.settings.urls.apiUrl;
    }

    public loadDoc(id) {
        return this.http.gettext('/docs/' + id + '.md');
    }

    public saveDoc(id, text) {
        return this.http.post(this.url() + '/document/save/' + id, text);
    }

    public upload(id : string, file : File) {
        let payload : FormData = new FormData();
        payload.append('file', file, file.name);
        return this.http.post(this.url() + '/document/upload/' + id, payload);
    }

    public delete(id: string, fn: string) {
        return this.http.delete(this.url() + '/document/delete/' + id + '/' + fn);
    }

    public listFiles(id : string) {
        return this.http.get(this.url() + '/document/images/' + id);
    }
}