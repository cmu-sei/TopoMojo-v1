
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedService } from "./_service";
import { ChangedProfile,Profile,ProfileSearchResult,Search } from "./models";

@Injectable()
export class GeneratedProfileService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getProfile(): Observable<Profile> {
		return this.http.get<Profile>(this.hostUrl + "/api/profile");
	}
	public getProfiles(search: Search) : Observable<ProfileSearchResult> {
		return this.http.get<ProfileSearchResult>(this.hostUrl + "/api/profiles" + this.paramify(search));
	}
	public postProfile(profile: ChangedProfile) : Observable<ChangedProfile> {
		return this.http.post<ChangedProfile>(this.hostUrl + "/api/profile", profile);
	}
	public privProfile(profile: Profile) : Observable<Profile> {
		return this.http.post<Profile>(this.hostUrl + "/api/profile/priv", profile);
	}

}

