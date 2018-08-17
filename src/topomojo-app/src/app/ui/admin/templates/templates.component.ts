import { Component, OnInit, OnDestroy } from '@angular/core';
import { TemplateService } from '../../../api/template.service';
import { ToolbarService } from '../../svc/toolbar.service';
import { Search, TemplateDetail, Template, TemplateSummarySearchResult, TemplateSummary } from '../../../api/gen/models';
import { Subscription } from 'rxjs';

@Component({
  selector: 'topomojo-templates',
  templateUrl: './templates.component.html',
  styleUrls: ['./templates.component.scss']
})
export class TemplatesComponent implements OnInit, OnDestroy {

  search: Search = { take: 25, filters: [ 'parents' ] }
  hasMore = false;
  current = 0;
  subs: Array<Subscription> = [];
  templates: Array<TemplateSummary> = [];
  detail: TemplateDetail;

  constructor(
    private templateSvc: TemplateService,
    private toolbar: ToolbarService
  ) { }

  ngOnInit() {

    this.subs.push(
        this.toolbar.term$.subscribe(
        (term: string) => {
          this.search.term = term;
          this.fetch();
        }
      )
    );

    this.toolbar.search(true);
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    this.toolbar.reset();
  }

  fetch() {
    this.search.skip = 0;
    this.templates = [];
    this.more();
  }

  more() {
    this.templateSvc.getTemplates(this.search).subscribe(
      (data: TemplateSummarySearchResult) => {
        this.templates.push(...data.results);
        this.search.skip += data.results.length;
        this.hasMore = data.results.length === this.search.take;
      }
    );
  }

  select(template: TemplateSummary) {
    this.current = (this.current !== template.id) ? template.id : 0;
    if (!!this.current) {
      this.detail = null;
      this.templateSvc.getTemplateDetailed(this.current).subscribe(
        (t: TemplateDetail) => {
          this.detail = t;
        }
      );
    }
  }
}
