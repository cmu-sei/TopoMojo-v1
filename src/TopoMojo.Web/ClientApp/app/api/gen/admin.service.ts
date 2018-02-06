
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
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

}

