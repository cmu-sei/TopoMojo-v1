
import { Injectable } from "@angular/core";
import { HttpClient, HttpRequest, HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedDocumentService } from "./gen/document.service";
import { ImageFile } from "./gen/models";

@Injectable()
export class DocumentService extends GeneratedDocumentService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

    public getDocument(guid : string) : Observable<string> {
		return this.http.get("/docs/" + guid + ".md", { responseType: "text"});
	}

    public uploadImage(guid: string, file: File) : Observable<HttpEvent<ImageFile>> {
		let payload : FormData = new FormData();
        payload.append('file', file, file.name);
        return this.http.request<ImageFile>(
            new HttpRequest('POST', "/api/image/" + guid, payload, { reportProgress: true })
        );
    }

    public deleteImage(guid: string, filename: string) : Observable<ImageFile> {
		return this.http.delete("/api/image/" + guid + "?filename=" + filename);
	}

}
