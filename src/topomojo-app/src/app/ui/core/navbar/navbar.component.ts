import { Component, OnInit, AfterViewInit, ViewChild, ElementRef, Input } from '@angular/core';
import { BreakpointObserver, Breakpoints, BreakpointState } from '@angular/cdk/layout';
import { Observable, Subject } from 'rxjs';
import { map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ToolbarService, NavbarButton, ToolbarState } from '../../svc/toolbar.service';
import { UserService } from '../../../svc/user.service';
import { Profile } from '../../../api/gen/models';
import { SettingsService } from '../../../svc/settings.service';

@Component({
  selector: 'topomojo-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  state: ToolbarState;
  searchOpen = true;
  altTheme = false;
  @Input() sidenav = false;
  public termSubject: Subject<string> = new Subject();
  profile: Profile = {};
  term = '';
  term$: Observable<string> = this.termSubject.asObservable()
    .pipe(
      debounceTime(500),
      distinctUntilChanged()
    );

  appName: string;

  constructor(
    private breakpointObserver: BreakpointObserver,
    private toolbarSvc: ToolbarService,
    private userSvc: UserService,
    private settingsSvc: SettingsService
  ) {}

  ngOnInit() {

    this.appName = this.settingsSvc.settings.branding.applicationName || 'TopoMojo';
    this.altTheme = this.settingsSvc.localSettings.altTheme;

    this.toolbarSvc.state$.subscribe(
      (state: ToolbarState) => {
        this.state = state;
      }
    );

    this.userSvc.profile$.subscribe(
      p =>  {
        this.profile = p;
      }
    );

    // this.toolbarSvc.term$ = this.term$;
    this.term$.subscribe(term => this.toolbarSvc.termChanged(term));
  }

  termChanged(): void {
    this.termSubject.next(this.term);
  }

  toggleTheme() {
    this.altTheme = !this.altTheme;
    this.settingsSvc.updateTheme(this.altTheme);
    this.toolbarSvc.themeChanged(this.altTheme);
  }

  toggleSearchInput() {
    this.searchOpen = !this.searchOpen;
    this.term = '';
    this.termChanged();
  }
}
