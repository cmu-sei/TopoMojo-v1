import { Component, OnInit, Input, ViewChild, AfterViewChecked, AfterViewInit, Output, EventEmitter } from '@angular/core';
import { Topology, Profile, TopologyStateActionTypeEnum, ChangedTopology } from '../../../api/gen/models';
import { MatChipEvent } from '@angular/material/chips';
import { TopologyService } from '../../../api/topology.service';
import { UserService } from '../../../svc/user.service';
import { SettingsService } from '../../../svc/settings.service';
import { NgForm } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';

@Component({
  selector: 'topomojo-workspace-settings',
  templateUrl: './workspace-settings.component.html',
  styleUrls: ['./workspace-settings.component.scss']
})
export class WorkspaceSettingsComponent implements OnInit, AfterViewInit {

  @Input() workspace: Topology;
  @Output() deleted = new EventEmitter<boolean>();
  profile: Profile = {};
  hostUrl = '';
  @ViewChild('form') form: NgForm;

  constructor(
    private service: TopologyService,
    private userSvc: UserService,
    private settingsSvc: SettingsService
  ) { }

  ngOnInit() {
    this.hostUrl = this.settingsSvc.hostUrl;
    this.userSvc.profile$.subscribe(
      (profile: Profile) => { this.profile = profile; }
    );
  }

  ngAfterViewInit() {
    // this.form.valueChanges.pipe(debounceTime(1000)).subscribe(
    //   (model) => {
    //     if (this.form.valid) {
    //       this.service.putTopology(model as ChangedTopology).subscribe(
    //         // TODO: animate feedback
    //       );
    //     }
    //   }
    // );
  }

  update() {
    if (this.form.valid) {
      this.service.putTopology(this.form.value as ChangedTopology).subscribe(
        (t) => {
          this.form.reset(this.form.value);
        }
        // TODO: animate feedback

      );
    }
  }
  published(): void {
    const v = !this.workspace.isPublished;
    this.service.postTopologyAction(this.workspace.id, {
        id: this.workspace.id,
        type: v ? TopologyStateActionTypeEnum.publish : TopologyStateActionTypeEnum.unpublish
    })
    .subscribe(
      () => { this.workspace.isPublished = v; }
    );
  }

  removeMember(e: MatChipEvent): void {
    this.service.deleteWorker(e.chip.value)
      .subscribe(
        () => {
          const i = this.workspace.workers.findIndex((worker) => worker.id === e.chip.value);
          if (i > -1) {
            this.workspace.workers.splice(i, 1);
          }
        },
        () => { }
      );
  }

  shareUrl(): string {
    return `${this.hostUrl}/invitation/${this.workspace.shareCode}`;
  }

  generateNewShareUrl() {
    this.service.postTopologyAction(this.workspace.id, {
      id: this.workspace.id,
      type: TopologyStateActionTypeEnum.share
    })
      .subscribe(
        (data) => {
          this.workspace.shareCode = data.shareCode;
          // this.animateSuccess('share');
        },
        (err) => {
          // this.animateFailure('share');
          // this.onError(err);
        }
      );
  }

  onDelete() {
    this.deleted.emit(true);
  }
}
