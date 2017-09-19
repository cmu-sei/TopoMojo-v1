
import { Injectable } from "@angular/core";
import { HttpClient, HttpRequest, HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { ImageFile } from "./api-models";

@Injectable()
export class CustomService {

    constructor(
        //private http: AuthHttp
        private http: HttpClient
    ) { }

	pageRefs: any = {};
    public openTab(url : string) : void {
        if ( typeof this.pageRefs[url] == 'undefined' || this.pageRefs[url].closed )
        {
            this.pageRefs[url] = window.open(url);
        } else {
            this.pageRefs[url].focus()
        }
    }

    public getText(url : string) : Observable<string> {
		return this.http.get(url, { responseType: "text"});
    }

    public openConsole(id, name) {
        this.openTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

    public getDocument(guid : string) : Observable<string> {
		return this.http.get("/docs/" + guid + ".md", { responseType: "text"});
	}

    public uploadImage(guid: string, file: File) : Observable<HttpEvent<ImageFile>> {
		let payload : FormData = new FormData();
        payload.append('file', file, file.name);
        return this.uploadWithProgress('/api/image/' + guid, payload);
    }

    public deleteImage(guid: string, filename: string) : Observable<ImageFile> {
		return this.http.delete("/api/image/" + guid + "?filename=" + filename);
	}
    public uploadIso(id: string, progressKey: string, file: File) : Observable<HttpEvent<boolean>> {
        let payload: FormData = new FormData();
        payload.append('meta', `size=${file.size}&group-key=${id}&scope=private&monitor-key=${progressKey}`);
        payload.append('file', file, file.name);
        return this.uploadWithProgress('/api/file/upload', payload);
    }

    private uploadWithProgress<T>(url: string, data: FormData) {
        return this.http.request<T>(
            new HttpRequest('POST', url, data, { reportProgress: true })
        );
    }
}
