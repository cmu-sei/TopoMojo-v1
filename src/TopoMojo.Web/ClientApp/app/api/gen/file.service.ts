
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Observable';
import { GeneratedService } from "./_service";
import {  } from "./models";

@Injectable()
export class GeneratedFileService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public progressFile(id: string) : Observable<number> {
		return this.http.get<number>("/api/file/progress/" + id);
	}

}

