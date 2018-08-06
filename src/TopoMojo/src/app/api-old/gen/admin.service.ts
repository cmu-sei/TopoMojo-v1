
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedService } from "./_service";
import {  } from "./models";

@Injectable()
export class GeneratedAdminService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getsettingsAdmin() : Observable<any> {
		return this.http.get<any>(this.hostUrl + "/api/admin/getsettings");
	}
	public savesettingsAdmin(settings: object) : Observable<any> {
		return this.http.post<any>(this.hostUrl + "/api/admin/savesettings", settings);
	}
	public announceAdmin(text: string) : Observable<boolean> {
		return this.http.post<boolean>(this.hostUrl + "/api/admin/announce", text);
	}
	public exportAdmin(ids: Array<number>) : Observable<Array<string>> {
		return this.http.post<Array<string>>(this.hostUrl + "/api/admin/export", ids);
	}
	public importAdmin() : Observable<Array<string>> {
		return this.http.get<Array<string>>(this.hostUrl + "/api/admin/import");
	}
	public getLiveUsers() : Observable<Array<any>> {
		return this.http.get<Array<any>>(this.hostUrl + "/api/admin/live");
	}
}

