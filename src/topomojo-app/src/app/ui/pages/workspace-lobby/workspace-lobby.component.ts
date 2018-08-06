import { Component, OnInit, OnDestroy } from '@angular/core';
import { ToolbarService } from '../../svc/toolbar.service';
import { TopologyService } from '../../../api/topology.service';
import { Search, Topology, TopologySearchResult, Profile } from '../../../api/gen/models';
import { UserService } from '../../../svc/user.service';
import { SettingsService } from '../../../svc/settings.service';

@Component({
  templateUrl: './workspace-lobby.component.html',
  styleUrls: ['./workspace-lobby.component.scss']
})
export class WorkspaceLobbyComponent implements OnInit, OnDestroy {

  more = false;
  showAdd = false;
  showGames = false;
  list: Array<Topology> = new Array<Topology>();
  model: Search = { sort: 'age', take: 25, filters: [ 'public' ] };
  filter = 'public';
  private profile: Profile;

  constructor(
    private toolbarSvc: ToolbarService,
    private workspaceSvc: TopologyService,
    private userSvc: UserService,
    private settingsSvc: SettingsService
  ) { }

  ngOnInit() {
    this.userSvc.profile$.subscribe(
      p =>  {
          this.profile = p;
          if (p) {
            this.filter = this.settingsSvc.localSettings.lobbyFilter || 'public';
          }
      }
    );

    this.toolbarSvc.term$.subscribe(
      (term: string) => {
        this.model.term = term;
        this.fetch_fresh();
      }
    );
    // this.filter = this.settingsSvc.localSettings.lobbyFilter || 'public';
    // this.model.filters = [ this.filter ];

    // this.fetch();

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

  filterChanged(): void {
    this.model.filters = [ this.filter ];
    this.fetch_fresh();
    this.settingsSvc.updateLobbyFilter(this.filter);
  }

  hasSomeGames(v: boolean): void {
    this.showGames = v;
  }

  hasProfile(): boolean {
    return !!this.profile.id;
  }

}
