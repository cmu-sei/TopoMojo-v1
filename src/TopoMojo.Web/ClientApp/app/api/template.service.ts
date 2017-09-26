
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedTemplateService } from "./gen/template.service";
import { ChangedTemplate,Search,Template,TemplateDetail,TemplateSummary,TemplateSummarySearchResult } from "./gen/models";

@Injectable()
export class TemplateService extends GeneratedTemplateService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
