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

  search: Search = { take: 25, filters: [ 'parents' ] };
  hasMore = false;
  current = 0;
  subs: Array<Subscription> = [];
  templates: Array<TemplateSummary> = [];
  detail: TemplateDetail;
  showCreator = false;

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
    this.toolbar.addButtons([
      {
        icon: 'add_circle',
        clicked: () => this.showCreator = !this.showCreator
      }
    ]);
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

  filterChanged(e) {
    this.search.filters = [ e.value ];
    this.fetch();
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

  created(template: TemplateDetail) {
    this.current = template.id;
    this.detail = template;
    this.templates.unshift(template as TemplateSummary);
    this.showCreator = false;
  }

  delete(template: TemplateSummary) {
    this.templateSvc.deleteTemplate(template.id).subscribe(
      () => {
        if (this.current === template.id) {
          this.current = 0;
          this.detail = null;
        }
        this.templates.splice(this.templates.indexOf(template), 1);
      }
    );
  }

  trackById(i: number, item: TemplateSummary): number {
    return item.id;
  }
}
