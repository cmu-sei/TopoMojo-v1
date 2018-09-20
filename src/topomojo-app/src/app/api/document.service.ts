
import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedDocumentService } from './gen/document.service';
import { ImageFile } from './gen/models';

@Injectable()
export class DocumentService extends GeneratedDocumentService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getDocument(guid: string): Observable<string> {
        return this.http.get(this.api.docUrl + '/docs/' + guid + '.md?ts=' + Date.now(), { responseType: 'text'});
    }

    public uploadImage(guid: string, file: File): Observable<HttpEvent<ImageFile>> {
        const payload: FormData = new FormData();
        payload.append('file', file, file.name);
        return this.http.request<ImageFile>(
            new HttpRequest('POST', this.api.url + '/api/image/' + guid, payload, { reportProgress: true })
        );
    }
}
