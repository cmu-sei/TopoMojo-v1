
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedProfileService } from './gen/profile.service';
import { ChangedProfile, Profile, ProfileSearchResult, Search } from './gen/models';

@Injectable()
export class ProfileService extends GeneratedProfileService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }
}
