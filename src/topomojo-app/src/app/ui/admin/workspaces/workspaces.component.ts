import { Component, OnInit, OnDestroy } from '@angular/core';
import { TopologyService } from '../../../api/topology.service';
import { Topology, TopologySearchResult, Search } from '../../../api/gen/models';
import { ToolbarService } from '../../svc/toolbar.service';
import { Subscription } from 'rxjs';

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

  trackById(i: number, item: Topology): number {
    return item.id;
  }
}
