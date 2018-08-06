import { Component, OnInit, Input, ViewChild, ElementRef, EventEmitter, Output, AfterViewInit } from '@angular/core';
import { DataSource } from '@angular/cdk/collections';
import { MatPaginator } from '@angular/material/paginator';
import { fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap } from 'rxjs/operators';
import { IsoDataSource, IDataSource } from '../../datasources';

@Component({
  selector: 'topomojo-select-table',
  templateUrl: './select-table.component.html',
  styleUrls: ['./select-table.component.scss']
})
export class SelectTableComponent implements OnInit, AfterViewInit {
  @Input() term = '';
  @Input() take = 10;
  @Input() dataSource: IDataSource<any>;
  @Input() tableColumns: Array<string> = ['name'];
  @Input() filters: Array<string> = [];
  @Output() selected = new EventEmitter<any>();
  @ViewChild('input') input: ElementRef;
  @ViewChild(MatPaginator) paginator: MatPaginator;

  constructor() { }

  ngOnInit() {
  }

  ngAfterViewInit() {

    fromEvent(this.input.nativeElement, 'input').pipe(
      debounceTime(250),
      distinctUntilChanged()
    ).subscribe(() => {
      this.term = this.term;
      this.queryChanged();
    });

    this.paginator.page
      .pipe(tap(() => this.queryChanged()))
      .subscribe();

    this.queryChanged();
  }

  queryChanged(): void {
    this.dataSource.load({
      term: this.term,
      skip: this.paginator.pageIndex * this.paginator.pageSize,
      take: this.paginator.pageSize,
      filters: this.filters
    });
  }

  clicked(item): void {
    this.selected.emit(item);
  }

}
