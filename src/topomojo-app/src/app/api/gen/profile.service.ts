
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { ChangedProfile, Profile, ProfileSearchResult, Search } from './models';

@Injectable()
export class GeneratedProfileService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getProfiles(search: Search): Observable<ProfileSearchResult> {
        return this.http.get<ProfileSearchResult>(this.api.url + '/api/profiles' + this.paramify(search));
    }
    public getProfile(): Observable<Profile> {
        return this.http.get<Profile>(this.api.url + '/api/profile');
    }
    public putProfile(profile: ChangedProfile): Observable<any> {
        return this.http.put<any>(this.api.url + '/api/profile', profile);
    }
    public putProfilePriv(profile: Profile): Observable<any> {
        return this.http.put<any>(this.api.url + '/api/profile/priv', profile);
    }
    public deleteProfile(id: number): Observable<any> {
        return this.http.delete<any>(this.api.url + '/api/profile/' + id);
    }

}
