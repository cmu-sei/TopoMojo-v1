
import { Injectable } from "@angular/core";
import { HttpClient, HttpRequest, HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedFileService } from "./gen/file.service";
import {  } from "./gen/models";

@Injectable()
export class FileService extends GeneratedFileService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

    public uploadIso(id: string, progressKey: string, file: File) : Observable<HttpEvent<boolean>> {
        let payload: FormData = new FormData();
        payload.append('meta', `size=${file.size}&group-key=${id}&scope=private&monitor-key=${progressKey}`);
        payload.append('file', file, file.name);
        return this.http.request<boolean>(
            new HttpRequest('POST', '/api/file/upload', payload, { reportProgress: true })
        );
    }
}
