
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedService } from "./_service";
import {  } from "./models";

@Injectable()
export class GeneratedConsoleService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getConsole(id: string, name: string) : Observable<any> {
		return this.http.get<any>("/console/" + id + "/" + name);
	}

}

