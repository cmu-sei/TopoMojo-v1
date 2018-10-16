import { Component, OnInit, OnDestroy } from '@angular/core';
import { TopologyService } from '../../../api/topology.service';
import { Topology, TopologySearchResult, Search } from '../../../api/gen/models';
import { ToolbarService } from '../../svc/toolbar.service';
import { Subscription, Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'topomojo-workspaces',
  templateUrl: './workspaces.component.html',
  styleUrls: ['./workspaces.component.scss']
})
export class WorkspacesComponent implements OnInit, OnDestroy {

  current = 0;
  topos: Array<Topology> = [];
  search: Search = {take: 26};
  hasMore = false;
  subs: Array<Subscription> = [];
  changedTopo = new Subject<Topology>();
  changedTopo$ = this.changedTopo.asObservable();
  constructor(
    private topoSvc: TopologyService,
    private toolbar: ToolbarService
  ) { }

  ngOnInit() {
    this.subs.push(
      this.toolbar.term$.subscribe(
        (term: string) => {
          this.search.term = term;
          this.fetch();
        }
      ),

      this.changedTopo$.pipe(
        debounceTime(500)
      ).subscribe((topo) => {
        this.topoSvc.putTopologyPriv(topo).subscribe();
      })
    );
    this.toolbar.search(true);
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    this.toolbar.reset();
  }

  fetch() {
    this.search.skip = 0;
    this.topos = [];
    this.more();
  }

  more() {
    this.topoSvc.getTopologies(this.search).subscribe(
      (data: TopologySearchResult) => {
        this.topos.push(...data.results);
        this.search.skip += data.results.length;
        this.hasMore = data.results.length === this.search.take;
      }
    );
  }

  filterChanged(e) {
    this.search.filters = [ e.value ];
    this.fetch();
  }

  workers(topo: Topology): string {
    return topo.workers.map(p => p.personName).join();
  }

  select(topo: Topology) {
    this.current = (this.current !== topo.id) ? topo.id : 0;
  }

  delete(topo: Topology) {
    this.topoSvc.deleteTopology(topo.id).subscribe(
      () => {
        this.topos.splice(this.topos.indexOf(topo), 1);
      }
    );
  }

  trackById(i: number, item: Topology): number {
    return item.id;
  }

  changeLimit(topo: Topology, count: number) {
    topo.templateLimit += count;
    this.changedTopo.next(topo);
  }
}
