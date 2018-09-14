import { Component, OnInit, OnDestroy } from '@angular/core';
import { Search, Profile, ProfileSearchResult } from '../../../api/gen/models';
import { Subscription } from 'rxjs';
import { ProfileService } from '../../../api/profile.service';
import { ToolbarService } from '../../svc/toolbar.service';

@Component({
  selector: 'topomojo-people',
  templateUrl: './people.component.html',
  styleUrls: ['./people.component.scss']
})
export class PeopleComponent implements OnInit, OnDestroy {
  search: Search = { take: 25 };
  hasMore = false;
  current = 0;
  subs: Array<Subscription> = [];
  people: Array<Profile> = [];

  constructor(
    private profileSvc: ProfileService,
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
    this.people = [];
    this.more();
  }

  more() {
    this.profileSvc.getProfiles(this.search).subscribe(
      (data: ProfileSearchResult) => {
        this.people.push(...data.results);
        this.search.skip += data.results.length;
        this.hasMore = data.results.length === this.search.take;
      }
    );
  }

  filterChanged(e) {
    this.search.filters = [ e.value ];
    this.fetch();
  }

  select(p: Profile) {
    this.current = (this.current !== p.id) ? p.id : 0;
  }

  trackById(i: number, item: Profile): number {
    return item.id;
  }

  onDeleted(p: Profile) {
    this.people.splice(this.people.indexOf(p),1);
  }
}
