
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedTemplateService } from './gen/template.service';
import { ChangedTemplate, Search, Template, TemplateDetail, TemplateSummary, TemplateSummarySearchResult } from './gen/models';

@Injectable()
export class TemplateService extends GeneratedTemplateService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }
}
