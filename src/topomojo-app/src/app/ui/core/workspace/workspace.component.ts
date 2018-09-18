import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Topology, Template, TemplateSummary } from '../../../api/gen/models';
import { TopologyService } from '../../../api/topology.service';
import { NotificationService } from '../../../svc/notification.service';
import { SettingsService } from '../../../svc/settings.service';
import { Subscription } from 'rxjs';
import { ToolbarService, NavbarButton } from '../../svc/toolbar.service';
import { MatDrawer, MatSidenav } from '@angular/material/sidenav';
import { TemplateService } from '../../../api/template.service';
import { ExpiringDialogComponent } from '../../shared/expiring-dialog/expiring-dialog.component';
import { MatDialogRef, MatDialog } from '@angular/material/dialog';
import { VmControllerComponent } from '../../shared/vm-controller/vm-controller.component';

@Component({
  templateUrl: './workspace.component.html',
  styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit, OnDestroy {

  id: number;
  workspace: Topology;
  gameCount = 0;
  private subs: Subscription[] = [];
  errors: any[] = [];
  settingsOpened = false;
  collabButton: NavbarButton = {
    icon: 'group',
    description: 'Collaborate',
    clicked: () => { this.toolbar.toggleSide(); }
  };
  private dialogRef: MatDialogRef<ExpiringDialogComponent>;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private service: TopologyService,
    private templateSvc: TemplateService,
    private notifier: NotificationService,
    private settingsSvc: SettingsService,
    private toolbar: ToolbarService,
    private dialogSvc: MatDialog
  ) { }

  ngOnInit() {

    this.id = +this.route.snapshot.paramMap.get('id');

    this.toolbar.addButtons([ this.collabButton ]);
    // setTimeout(() => this.toolbar.sideComponent = 'chat', 1);

    this.service.getTopology(this.id).subscribe(
      (result: Topology) => {
          this.workspace = result;
          // this.router.navigate([{ outlets: { sidenav: ['chat', this.workspace.globalId]}}]);
          this.startListening();
          this.settingsOpened = this.workspace.templates.length === 0;
      },
      (err) => {
        this.onError(err);
      }
    );
  }

  ngOnDestroy(): void {
    this.toolbar.reset();
    this.notifier.stop();
    this.subs.forEach(sub => { sub.unsubscribe(); });
  }

  startListening(): void {
    this.subs.push(
      this.notifier.topoEvents.subscribe(
          (event) => {
              switch (event.action) {
                  case 'TOPO.UPDATED':
                  this.workspace = event.model;
                  break;

                  case 'TOPO.DELETED':
                  this.dialogRef = this.dialogSvc.open(ExpiringDialogComponent, {
                    disableClose: true,
                    closeOnNavigation: true,
                    data: { title: 'Workspace Deleted', button: 'Continue' }
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
      this.notifier.templateEvents.subscribe(
          (event) => {
              switch (event.action) {
                  case 'TEMPLATE.ADDED':
                  this.workspace.templates.push(event.model);
                  break;

                  case 'TEMPLATE.UPDATED':
                  this.templateModified(event.model);
                  break;

                  case 'TEMPLATE.REMOVED':
                  this.templateRemoved(event.model);
                  break;
              }
          }
      )
    );

    this.loadWorkers().then(() => {
      this.notifier.start(this.workspace.globalId);
    });
  }

  loadWorkers(): Promise<boolean> {
    return new Promise((resolve) => {
      this.notifier.actors = this.workspace.workers.map(
          (worker) => {
              return {
                  id: worker.personGlobalId,
                  name: worker.personName,
                  online: false
              };
          }
      );
      resolve(true);
    });
  }

  templateAdded(template: TemplateSummary) {
    this.templateSvc.postTemplateLink({
      templateId: template.id,
      topologyId: this.workspace.id
    })
    .subscribe(
        (t: Template) => { this.workspace.templates.push(t); },
        (err) => {  this.onError(err); }
    );
  }

  templateModified(template: Template) {
    const i = this.workspace.templates.findIndex((t) => t.id === template.id);
    if (i >= 0) {
      this.workspace.templates[i] = template;
    }
}

  templateRemoved(template: Template) {
    const i = this.workspace.templates.findIndex((t) => t.id === template.id);
    if (i >= 0) {
      this.workspace.templates.splice(i, 1);
    }
  }

  templateCloned(template: Template) {
    const i = this.workspace.templates.findIndex((t) => t.id === template.id);
    if (i >= 0) {
      this.workspace.templates.splice(i, 1, template);
    }
  }

  deleted() {
    this.service.deleteTopology(this.workspace.id).subscribe(
      (r: boolean) => {
        this.router.navigate(['/topo']);
      }
    );
  }

  deleteGamespaces() {
    this.service.deleteTopologyGames(this.workspace.id).subscribe(
      (result) => {
          if (result) {
              this.workspace.gamespaceCount = 0;
          }
      },
      (err) => {
          this.onError(err);
      }
    );
  }

  deleteGamesText(): string {
    return `Delete ${this.workspace.gamespaceCount} Gamespaces`;
  }

  atLimit(): boolean {
    return this.workspace.templates.length >= this.workspace.templateLimit;
  }

  trackById(i: number, item: Template): number {
    return item.id;
  }

  onError(err) {
    this.errors.push(err.error);
    // console.debug(err.error.message);
  }
}
