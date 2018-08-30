import { Component, OnInit, ViewChild, AfterViewInit, ElementRef, EventEmitter, Output, Input } from '@angular/core';
import { DataSource, CollectionViewer } from '@angular/cdk/collections';
import { TemplateSummary, TemplateSummarySearchResult, Search } from '../../../api/gen/models';
import { Observable, BehaviorSubject, of, fromEvent } from 'rxjs';
import { TemplateService } from '../../../api/template.service';
import { catchError, tap, debounceTime, distinctUntilChanged, map } from 'rxjs/operators';
import { MatPaginator } from '@angular/material/paginator';
import { TemplateDataSource } from '../../datasources';

@Component({
  selector: 'topomojo-template-selector',
  templateUrl: './template-selector.component.html',
  styleUrls: ['./template-selector.component.scss']
})
export class TemplateSelectorComponent implements OnInit {
  tableColumns = [ 'name', 'description' ];
  term = '';
  take = 10;
  dataSource: TemplateDataSource;
  @Output() selected = new EventEmitter<TemplateSummary>();

  constructor(
    private templateSvc: TemplateService
  ) { }

  ngOnInit() {
    this.dataSource = new TemplateDataSource(this.templateSvc);
  }

  clicked(item): void {
    this.selected.emit(item);
  }

}
