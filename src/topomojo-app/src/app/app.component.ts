import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { AuthService, AuthTokenState } from './svc/auth.service';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { ExpiringDialogComponent } from './ui/shared/expiring-dialog/expiring-dialog.component';
import { Subscription } from 'rxjs';
import { filter } from 'rxjs/operators';
import { ToolbarService } from './ui/svc/toolbar.service';
import { MatSidenav } from '@angular/material/sidenav';
import { SettingsService } from './svc/settings.service';

@Component({
  selector: 'topomojo-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
  title = 'app';
  altTheme = false;
  @ViewChild(MatSidenav) sidenav: MatSidenav;
  private dialogRef: MatDialogRef<ExpiringDialogComponent>;
  private dialogCloseSubscription: Subscription;
  private tokenState: AuthTokenState = AuthTokenState.invalid;
  private subs: Array<Subscription> = [];
  private lastRoute = '';

  constructor(
    private tokenSvc: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private dialogSvc: MatDialog,
    private toolbar: ToolbarService,
    private settingSvc: SettingsService
  ) { }

  ngOnInit() {

    // this.altTheme = this.settingSvc.localSettings.altTheme;
    this.setTheme(this.settingSvc.localSettings.altTheme);
    this.toolbar.sidenav = this.sidenav;

    this.subs.push(
      this.router.events
      .pipe(
        filter((e: Event) => e instanceof NavigationEnd)
      ).subscribe(
        (e: NavigationEnd) => {
          this.lastRoute = e.url;
        }
      ),

      this.tokenSvc.tokenState$.subscribe(
        (state: AuthTokenState) => {

          // Don't pop Still-Here dialog on consoles
          if (this.lastRoute.startsWith('/console/')) {
            return;
          }

          if (state === AuthTokenState.expiring) {
            this.dialogRef = this.dialogSvc.open(ExpiringDialogComponent, {
              disableClose: true,
              closeOnNavigation: true,
              data: { title: 'Still Here?', button: 'Continue' }
            });
            this.dialogCloseSubscription = this.dialogRef.afterClosed().subscribe(
              () => {
                this.tokenSvc.silentLogin();
              }
            );
          }
          if (state !== this.tokenState && state === AuthTokenState.invalid) {
            if (this.dialogRef) {
              this.dialogCloseSubscription.unsubscribe();
              this.dialogRef.close();
            }
            this.router.navigate(['/']);
          }
          this.tokenState = state;
        }
      ),

      this.toolbar.theme$.subscribe(alt => this.setTheme(alt))
    );
  }

  setTheme(useAlt: boolean) {
    const body = document.getElementsByTagName('body')[0];
    if (useAlt) {
      body.classList.add('alt-theme');
    } else {
      body.classList.remove('alt-theme');
    }
  }
  ngAfterViewInit() {
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
  }
}
