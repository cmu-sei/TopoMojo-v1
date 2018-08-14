import { Component, OnInit, OnDestroy } from '@angular/core';
import { ToolbarService } from '../../svc/toolbar.service';
import { TopologyService } from '../../../api/topology.service';
import { Search, Topology, TopologySearchResult, Profile } from '../../../api/gen/models';
import { UserService } from '../../../svc/user.service';
import { SettingsService } from '../../../svc/settings.service';
import { distinctUntilChanged, debounceTime, map } from 'rxjs/operators';

@Component({
  templateUrl: './workspace-lobby.component.html',
  styleUrls: ['./workspace-lobby.component.scss']
})
export class WorkspaceLobbyComponent implements OnInit, OnDestroy {

  more = false;
  showAdd = false;
  showGames = false;
  list: Array<Topology> = new Array<Topology>();
  model: Search = { sort: 'age', take: 25 };
  filter = '';
  private profile: Profile = {};
  hasProfile = false;

  constructor(
    private toolbarSvc: ToolbarService,
    private workspaceSvc: TopologyService,
    private userSvc: UserService,
    private settingsSvc: SettingsService
  ) { }

  ngOnInit() {
    this.userSvc.profile$.pipe(
      debounceTime(500),
      map(p => !!p.id))
      .subscribe(
        p =>  {
          this.hasProfile = p;

          const f = p ? 'private' : 'public';
          if (f != this.filter) {
            this.filter = f;
            this.filterChanged({ value: f });
          }
        }
    );

    this.toolbarSvc.term$.subscribe(
      (term: string) => {
        this.model.term = term;
        this.fetch_fresh();
      }
    );

    this.toolbarSvc.search(true);
    this.toolbarSvc.addButtons([{
      icon: 'add_circle',
      text: 'New Workspace',
      clicked: () => this.showAdd = !this.showAdd,
    }]);
  }

  ngOnDestroy() {
    this.toolbarSvc.reset();
  }

  fetch_fresh(): void {
    this.model.skip = 0;
    this.fetch();
  }

  fetch(): void {
    if (!this.model.filters || !this.model.filters.length) {
      return;
    }

    this.workspaceSvc.getTopologySummaries(this.model)
    .subscribe(
      (data: TopologySearchResult) => {
        if (this.model.skip > 0) {
          this.list.concat(data.results);
        } else {
          this.list = data.results;
        }
        this.model.skip += data.results.length;
        this.more = data.results.length === this.model.take;
      }
    );
  }

  filterChanged(e): void {
    if (e.value) {
      this.model.filters = [ e.value ];
      this.fetch_fresh();
    }
  }

  hasSomeGames(v: boolean): void {
    this.showGames = v;
  }

  // hasProfile(): boolean {
  //   return !!this.profile.id;
  // }

}
