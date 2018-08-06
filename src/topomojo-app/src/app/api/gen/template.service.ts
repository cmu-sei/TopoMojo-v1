
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { ChangedTemplate, Search, Template, TemplateDetail, TemplateLink, TemplateSummary, TemplateSummarySearchResult } from './models';

@Injectable()
export class GeneratedTemplateService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getTemplates(search: Search): Observable<TemplateSummarySearchResult> {
        return this.http.get<TemplateSummarySearchResult>(this.api.url + '/api/templates' + this.paramify(search));
    }
    public getTemplate(id: number): Observable<Template> {
        return this.http.get<Template>(this.api.url + '/api/template/' + id);
    }
    public deleteTemplate(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/template/' + id);
    }
    public putTemplate(template: ChangedTemplate): Observable<Template> {
        return this.http.put<Template>(this.api.url + '/api/template', template);
    }
    public postTemplateLink(link: TemplateLink): Observable<Template> {
        return this.http.post<Template>(this.api.url + '/api/template/link', link);
    }
    public postTemplateUnlink(link: TemplateLink): Observable<Template> {
        return this.http.post<Template>(this.api.url + '/api/template/unlink', link);
    }
    public getTemplateDetailed(id: number): Observable<TemplateDetail> {
        return this.http.get<TemplateDetail>(this.api.url + '/api/template/' + id + '/detailed');
    }
    public postTemplateDetailed(model: TemplateDetail): Observable<TemplateDetail> {
        return this.http.post<TemplateDetail>(this.api.url + '/api/template/detailed', model);
    }
    public putTemplateDetail(template: TemplateDetail): Observable<TemplateDetail> {
        return this.http.put<TemplateDetail>(this.api.url + '/api/template/detail', template);
    }

}
