
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedService } from "./_service";
import { ImageFile } from "./models";

@Injectable()
export class GeneratedDocumentService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public putDocument(guid: string, text: string) : Observable<boolean> {
		return this.http.put<boolean>("/api/document/" + guid, text);
	}
	public getImages(guid: string) : Observable<Array<ImageFile>> {
		return this.http.get<Array<ImageFile>>("/api/images/" + guid);
	}
	public deleteImage(guid: string, filename: string) : Observable<ImageFile> {
		return this.http.delete<ImageFile>("/api/image/" + guid + this.paramify({filename: filename}));
	}

}

