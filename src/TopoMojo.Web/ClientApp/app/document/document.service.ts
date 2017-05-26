import { AuthHttp } from '../core/auth-http';
import { Injectable } from '@angular/core';

@Injectable()
export class DocumentService {

    constructor(
        private http: AuthHttp
        ) {}

    public saveDoc(id, text) {
        return this.http.post('/api/topology/savedocument/' + id, text);
    }

    public loadDoc(id) {
        return this.http.gettext('/docs/' + id + '.md');
    }

    public upload(id : string, file : File) {
        let payload : FormData = new FormData();
        payload.append('file', file, file.name);
        return this.http.post('/api/document/upload/' + id, payload);
    }

    public delete(id: string, fn: string) {
        return this.http.delete('/api/document/delete/' + id + '/' + fn);
    }

    public listFiles(id : string) {
        return this.http.get('/api/document/images/' + id);
    }
}