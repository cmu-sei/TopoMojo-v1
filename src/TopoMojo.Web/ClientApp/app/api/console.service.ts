
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import {  } from "./api-models";

@Injectable()
export class ConsoleService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public getConsole(id: string, name: string) : Observable<any> {
		return this.http.get("/console/" + id + "/" + name);
	}

}
