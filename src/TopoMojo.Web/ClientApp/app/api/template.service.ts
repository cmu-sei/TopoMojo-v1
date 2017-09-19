
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { TemplateSummarySearchResult,Search,TemplateSummary,Template,TemplateDetail,ChangedTemplate } from "./api-models";

@Injectable()
export class TemplateService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public getTemplates(search : Search) : Observable<TemplateSummarySearchResult> {
		return this.http.get("/api/templates" + UrlHelper.queryStringify(search));
	}
	public getTemplate(id: number) : Observable<Template> {
		return this.http.get("/api/template/" + id);
	}
	public deleteTemplate(id: number) : Observable<boolean> {
		return this.http.delete("/api/template/" + id);
	}
	public detailedTemplate(id: number) : Observable<TemplateDetail> {
		return this.http.get("/api/template/" + id + "/detailed");
	}
	public createTemplate(model: TemplateDetail) : Observable<TemplateDetail> {
		return this.http.post("/api/template/create", model);
	}
	public configureTemplate(template: TemplateDetail) : Observable<TemplateDetail> {
		return this.http.put("/api/template/configure", template);
	}
	public linkTemplate(id: number, topoId: number) : Observable<Template> {
		return this.http.get("/api/template/" + id + "/link/" + topoId);
	}
	public unlinkTemplate(id: number) : Observable<Template> {
		return this.http.get("/api/template/" + id + "/unlink");
	}
	public putTemplate(template: ChangedTemplate) : Observable<Template> {
		return this.http.put("/api/template", template);
	}

}
