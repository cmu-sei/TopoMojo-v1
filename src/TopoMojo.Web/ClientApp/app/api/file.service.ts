
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import {  } from "./api-models";

@Injectable()
export class FileService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public progressFile(id: string) : Observable<number> {
		return this.http.get("/api/file/progress/" + id);
	}

}
