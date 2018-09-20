import { DataSource, CollectionViewer } from '@angular/cdk/collections';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { TemplateService } from '../api/template.service';
import { TemplateSummary, Search, VmOptions, TemplateSummarySearchResult } from '../api/gen/models';
import { catchError } from 'rxjs/operators';
import { TopologyService } from '../api/topology.service';

export interface IDataSource<T> extends DataSource<T> {
    total: number;
    load(search: Search): void;
}

export class VmNetDataSource implements IDataSource<IsoFile> {

    private subject = new BehaviorSubject<Array<IsoFile>>([]);
    total = 0;
    private cache: Array<any>;

    constructor(
        private svc: TopologyService,
        private topoId: string
    ) { }

    connect(collectionViewer: CollectionViewer): Observable<Array<IsoFile>> {
        return this.subject.asObservable();
    }

    disconnect(collectionViewer: CollectionViewer): void {
        this.subject.complete();
    }

    load(search: Search): void {
        if (this.cache && (search.term || search.skip)) {
            this.emitResults(search);
            return;
        }

        this.svc.getTopologyNets(this.topoId).pipe(
        catchError(() => of([])),
        // finally(() => { })
        ).subscribe(
        (result: VmOptions) => {
            this.cache = result.net.map(s => ({ path: s, name: s.split('#').reverse().pop() }));
            this.total = this.cache.length;
            this.emitResults(search);
        });
    }

    emitResults(search: Search) {
        this.subject.next(
            this.cache.filter(i => i.name.match(search.term))
            .slice(search.skip, search.take)
       );
    }
}

export class IsoDataSource implements IDataSource<IsoFile> {

    private subject = new BehaviorSubject<Array<IsoFile>>([]);
    total = 0;
    private cache: Array<any>;

    constructor(
        private svc: TopologyService,
        private topoId: string
    ) { }

    connect(collectionViewer: CollectionViewer): Observable<Array<IsoFile>> {
        return this.subject.asObservable();
    }

    disconnect(collectionViewer: CollectionViewer): void {
        this.subject.complete();
    }

    load(search: Search): void {
        if (this.cache && (search.term || search.skip)) {
            this.emitResults(search);
            return;
        }

        this.svc.getTopologyIsos(this.topoId).pipe(
        catchError(() => of([])),
        // finally(() => { })
        ).subscribe(
        (result: VmOptions) => {
            this.cache = result.iso.map(s => ({ path: s, name: s.split('/').pop() }));
            this.total = this.cache.length;
            this.emitResults(search);
        });
    }

    emitResults(search: Search) {
        this.subject.next(
            this.filterCache(search)
            .sort((a, b) => a.name.localeCompare(b.name))
            .slice(search.skip, search.skip + search.take)
       );
    }

    filterCache(search: Search): Array<IsoFile> {
        const r = (search.term)
        ? this.cache.filter(i => i.name.match(search.term))
        : this.cache;

        this.total = r.length;
        return r;
    }
}

export interface IsoFile {
    path?: string;
    name?: string;
}

export class TemplateDataSource implements IDataSource<TemplateSummary> {

    private subject = new BehaviorSubject<TemplateSummary[]>([]);
    total = 0;
    constructor(
      private svc: TemplateService
    ) { }

    connect(collectionViewer: CollectionViewer): Observable<TemplateSummary[]> {
      return this.subject.asObservable();
    }

    disconnect(collectionViewer: CollectionViewer): void {
      this.subject.complete();
    }

    load(search: Search): void {
      this.svc.getTemplates(search).pipe(
        catchError(() => of([])),
        // finally(() => { })
      ).subscribe(
        (result: TemplateSummarySearchResult) => {
          this.total = result.total;
          this.subject.next(result.results);
        }
      );
    }
  }
