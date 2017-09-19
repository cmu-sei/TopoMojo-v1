
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { ImageFile } from "./api-models";

@Injectable()
export class DocumentService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public putDocument(guid: string, text: string) : Observable<boolean> {
		return this.http.put("/api/document/" + guid, text);
	}
	public getImages(guid: string) : Observable<Array<ImageFile>> {
		return this.http.get("/api/images/" + guid);
	}
	public deleteImage(guid: string, filename: string) : Observable<ImageFile> {
		return this.http.delete("/api/image/" + guid);
	}

}
