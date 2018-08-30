import { Injectable } from '@angular/core';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { UserService } from '../../svc/user.service';
import { Profile } from '../../api/gen/models';
import { MatSidenav } from '@angular/material/sidenav';

@Injectable({
  providedIn: 'root'
})
export class ToolbarService {

  private state: ToolbarState = { visible: true };
  public sidenav: MatSidenav;
  public state$: BehaviorSubject<ToolbarState> = new BehaviorSubject<ToolbarState>(this.state);
  public term: BehaviorSubject<string> = new BehaviorSubject<string>('');
  public term$: Observable<string> = this.term.asObservable();
  public theme$ = new Subject<boolean>();
  public searchShowing = false;
  sideComponent = '';
  sideData: any;

  constructor(
  ) {
  }

  termChanged(term: string) {
    this.term.next(term);
  }

  themeChanged(v: boolean) {
    this.theme$.next(v);
  }

  visible(visible: boolean): void {
    this.state.visible = visible;
    this.state$.next(this.state);
  }

  search(visible: boolean): void {
    this.state.search = visible;
    this.state$.next(this.state);
  }

  toggleSide() {
    this.state.sidenav = !this.state.sidenav;
    this.sidenav.toggle();
    // this.state$.next(this.state);
  }

  addButtons(buttons: Array<NavbarButton>) {
    this.state.buttons = buttons;
    this.state$.next(this.state);
  }

  reset() {
    this.sidenav.close();
    this.state = { visible: true, search: false, buttons: [] };
    this.state$.next(this.state);
  }

  updateButtonBadge(icon: string, badge: string) {
    const b = this.state.buttons.find(bn => bn.icon === icon);
    if (b) {
      b.badge = badge;
      this.state$.next(this.state);
    }
  }
}

export interface NavbarButton {
  icon?: string;
  text?: string;
  attr?: string;
  color?: string;
  badge?: string;
  description?: string;
  clicked?: Function;
}

export interface ToolbarState {
  search?: boolean;
  sidenav?: boolean;
  visible?: boolean;
  buttons?: Array<NavbarButton>;
}
