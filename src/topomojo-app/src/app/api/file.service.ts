
import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedFileService } from './gen/file.service';
import {  } from './gen/models';

@Injectable()
export class FileService extends GeneratedFileService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public uploadIso(id: string, file: File): Observable<HttpEvent<boolean>> {
        const payload: FormData = new FormData();
        payload.append('meta', `size=${file.size}&group-key=${id}`);
        payload.append('file', file, file.name);
        return this.http.request<boolean>(
            new HttpRequest('POST', this.api.url + '/api/file/upload', payload, { reportProgress: true })
        );
    }
}
