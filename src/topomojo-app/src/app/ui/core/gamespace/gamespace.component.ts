import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Converter } from 'showdown';
import { GamespaceService } from '../../../api/gamespace.service';
import { GameState, VmState, Vm, Player, Profile } from '../../../api/gen/models';
import { catchError, finalize, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { of, pipe, Subscription } from 'rxjs';
import { SettingsService } from '../../../svc/settings.service';
import { UserService } from '../../../svc/user.service';
import { NotificationService } from '../../../svc/notification.service';
import { VmService } from '../../../api/vm.service';
import { ExpiringDialogComponent } from '../../shared/expiring-dialog/expiring-dialog.component';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { ToolbarService, NavbarButton } from '../../svc/toolbar.service';
import { MatChipEvent } from '@angular/material/chips';

@Component({
  selector: 'topomojo-gamespace',
  templateUrl: './gamespace.component.html',
  styleUrls: ['./gamespace.component.scss']
})
export class GamespaceComponent implements OnInit, OnDestroy {
  id: number;
  title: string;
  dockLink: string;
  markdownDoc: string;
  renderedDoc: string;
  loading = true;
  live = false;
  private converter: Converter;
  game: GameState;
  errors: Array<Error> = [];
  private subs = new Array<Subscription>();
  private dialogRef: MatDialogRef<ExpiringDialogComponent>;
  private dialogCloseSubscription: Subscription;
  private profile: Profile = {};
  collabButton: NavbarButton = {
    icon: 'group',
    description: 'Collaborate',
    clicked: () => { this.toolbar.toggleSide(); }
  };

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private service: GamespaceService,
    private vmSvc: VmService,
    private settingsSvc: SettingsService,
    private userSvc: UserService,
    private notifier: NotificationService,
    private dialogSvc: MatDialog,
    private toolbar: ToolbarService
  ) {
    this.converter = new Converter(this.settingsSvc.settings.showdown);
  }

  ngOnInit() {
    this.id = +this.route.snapshot.paramMap.get('id');
    // this.live = this.route.snapshot.url[this.route.snapshot.url.length].path === 'live';
    this.userSvc.profile$.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(
      (profile) => {
        this.profile = profile;
        const query = (profile.id)
          ? this.service.getGamespace(this.id)
          : this.service.getGamespacePreview(this.id);

        this.loading = true;
        query.pipe(
          finalize(() => this.loading = false)
        ).subscribe(
          (result: GameState) => {
            this.game = result;
            this.title = result.name;
            if (profile.id) { this.initGame(result); }
            if (this.game.topologyDocument.startsWith('http')) {
              this.dockLink = this.game.topologyDocument;
            } else {
              this.service.getText(this.settingsSvc.settings.urls.docUrl + result.topologyDocument + '?ts=' + Date.now()).pipe(
                catchError(err => of('[No document provided]'))
              ).subscribe(
                (text) => {
                  this.markdownDoc = text;
                  this.renderedDoc = `<div class="reader">${this.converter.makeHtml(this.markdownDoc)}</div>`;
                }
              );
            }
          },
          (err) => {
            this.errors.push(err.error || err);
          }
        );
      }
    );

  }

  ngOnDestroy() {
    // this.toolbar.buttons = [];
    this.toolbar.reset();
    // this.router.navigate([{ outlets: { sidenav: null}}]);
    this.notifier.stop();
    this.subs.forEach(s => s.unsubscribe());
  }

  launch() {
    if (this.userSvc.profile.id) {
      this.loading = true;
      this.service.postGamespaceLaunch(this.id).pipe(
        finalize(() => this.loading = false)
      ).subscribe(
            (result) => {
                this.initGame(result);
            },
            (err) => {
                this.errors.push(err.error || err);
            }
        );

    } else {
      this.router.navigate(['/mojo', this.id, 'live']);
    }
  }

  private initGame(game: GameState) {

    this.game = game;

    if (!this.game.globalId) {
        return;
    }

    this.subs.push(
        this.notifier.gameEvents.subscribe(
            (event) => {
                switch (event.action) {
                    case 'GAME.OVER':
                    this.dialogRef = this.dialogSvc.open(ExpiringDialogComponent, {
                      disableClose: true,
                      closeOnNavigation: true,
                      data: { title: 'GAME OVER', button: 'Continue' }
                    });
                    this.subs.push(
                      this.dialogRef.afterClosed().subscribe(
                        () => {
                          this.router.navigate(['/topo']);
                        }
                      )
                    );
                    break;
                }
            }
        ),
        this.notifier.vmEvents.subscribe(
            (event) => {
                this.onVmUpdated(event.model as VmState);
            }
        ),
        this.notifier.chatEvents.subscribe(
            (event) => {
                switch (event.action) {
                    case 'CHAT.MESSAGE':
                    // if (!this.collaborationVisible) {
                    //     this.messageCount += 1;
                    // }
                    break;
                }
            }
        )
    );

    // this.router.navigate([{ outlets: { sidenav: ['chat', this.game.globalId]}}]);
    this.loadPlayers().then(() => {
      this.notifier.start(this.game.globalId);
      this.toolbar.addButtons([ this.collabButton ]);
      // setTimeout(() => this.toolbar.sideComponent = 'chat', 1);
    });
  }

  loadPlayers(): Promise<boolean> {
    return new Promise((resolve) => {
      this.notifier.actors = this.game.players.map(
          (player) => {
              return {
                  id: player.personGlobalId,
                  name: player.personName,
                  online: false
              };
          }
      );
      resolve(true);
    });
  }

  onVmUpdated(vm: VmState) {
    this.game.vms.forEach(
        (v, i) => {
            // console.log(v);
            if (v.id === vm.id) {
                v.isRunning = vm.isRunning;
                this.game.vms.splice(i, 1, v);
            }
        }
    );
  }

  console(vm: Vm) {
    this.vmSvc.openConsole(vm.id, vm.name);
  }

  delete() {
    this.loading = true;
    this.service.deleteGamespace(this.game.id)
        .subscribe(
            () => {
              this.router.navigate(['/topo']);
            },
            (err) => {
                this.errors.push(err.error || err);
            },
            () => {
                this.loading = false;
            }
        );
  }

  notMe(player: Player): boolean {
    return player.personGlobalId !== this.profile.globalId;
  }

  canManage(): boolean {
    return !!this.game.players.find(p => p.personGlobalId === this.profile.globalId && p.canManage);
  }

  removeMember(e: MatChipEvent): void {
    this.service.deletePlayer(e.chip.value)
      .subscribe(
        () => {
          const i = this.game.players.findIndex((player) => player.id === e.chip.value);
          if (i > -1) {
            this.game.players.splice(i, 1);
          }
        },
        () => { }
      );
  }

  shareUrl(): string {
    return `${this.settingsSvc.hostUrl}/mojo/enlist/${this.game.shareCode}`;
  }

}
