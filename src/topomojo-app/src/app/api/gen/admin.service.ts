
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { CachedConnection } from './models';

@Injectable()
export class GeneratedAdminService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getAdminGetsettings(): Observable<string> {
        return this.http.get<string>(this.api.url + '/api/admin/getsettings');
    }
    public postAdminSavesettings(settings: object): Observable<boolean> {
        return this.http.post<boolean>(this.api.url + '/api/admin/savesettings', settings);
    }
    public postAdminAnnounce(text: string): Observable<boolean> {
        return this.http.post<boolean>(this.api.url + '/api/admin/announce', text);
    }
    public postAdminExport(ids: Array<number>): Observable<Array<string>> {
        return this.http.post<Array<string>>(this.api.url + '/api/admin/export', {});
    }
    public getAdminImport(): Observable<Array<string>> {
        return this.http.get<Array<string>>(this.api.url + '/api/admin/import');
    }
    public getAdminLive(): Observable<Array<CachedConnection>> {
        return this.http.get<Array<CachedConnection>>(this.api.url + '/api/admin/live');
    }

}
