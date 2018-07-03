
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Observable';
import { GeneratedService } from "./_service";
import {  } from "./models";

@Injectable()
export class GeneratedAdminService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getsettingsAdmin() : Observable<any> {
		return this.http.get<any>("/api/admin/getsettings");
	}
	public savesettingsAdmin(settings: object) : Observable<any> {
		return this.http.post<any>("/api/admin/savesettings", settings);
	}
	public announceAdmin(text: string) : Observable<boolean> {
		return this.http.post<boolean>("/api/admin/announce", text);
	}
	public exportAdmin(ids: Array<number>) : Observable<Array<string>> {
		return this.http.post<Array<string>>("/api/admin/export", ids);
	}
	public importAdmin() : Observable<Array<string>> {
		return this.http.get<Array<string>>("/api/admin/import");
	}
	public getLiveUsers() : Observable<Array<any>> {
		return this.http.get<Array<any>>("/api/admin/live");
	}
}

