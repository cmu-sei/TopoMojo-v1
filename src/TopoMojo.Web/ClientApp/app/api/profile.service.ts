
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { ProfileSearchResult,Search,Profile } from "./api-models";

@Injectable()
export class ProfileService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public getProfiles(search : Search) : Observable<ProfileSearchResult> {
		return this.http.get("/api/profiles" + UrlHelper.queryStringify(search));
	}
}
