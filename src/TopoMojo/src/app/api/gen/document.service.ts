
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { ImageFile } from './models';

@Injectable()
export class GeneratedDocumentService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public putDocument(id: string, text: string): Observable<boolean> {
        return this.http.put<boolean>(this.api.url + '/api/document/' + id, text);
    }
    public getImages(id: string): Observable<Array<ImageFile>> {
        return this.http.get<Array<ImageFile>>(this.api.url + '/api/images/' + id);
    }
    public deleteImage(id: string, filename: string): Observable<ImageFile> {
        return this.http.delete<ImageFile>(this.api.url + '/api/image/' + id + this.paramify({filename: filename}));
    }

}
