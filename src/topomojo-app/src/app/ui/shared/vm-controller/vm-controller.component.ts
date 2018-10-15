import { Component, OnInit, Input, Output, EventEmitter, OnDestroy } from '@angular/core';
import { Template, Vm, VmOperationTypeEnum } from '../../../api/gen/models';
import { VmService } from '../../../api/vm.service';
import { catchError, finalize } from 'rxjs/operators';
import { of, Subscription } from 'rxjs';
import { NotificationService } from '../../../svc/notification.service';

@Component({
  selector: 'topomojo-vm-controller',
  templateUrl: './vm-controller.component.html',
  styleUrls: ['./vm-controller.component.scss']
})
export class VmControllerComponent implements OnInit, OnDestroy {

  @Input() vm: Vm = {};
  @Input() template: Template = {};
  @Output() loaded: EventEmitter<Vm> = new EventEmitter<Vm>();
  timer: any;
  confirmingDelete = false;
  errors: Array<Error> = [];
  subs: Array<Subscription> = [];
  taskRunning = false;

  constructor(
    private vmSvc: VmService,
    private notifier: NotificationService
  ) { }

  ngOnInit() {
    if (!this.vm.id) { this.refresh(); }

    this.subs.push(
      this.notifier.vmEvents.subscribe(
        (event) => {
          if (event.model.id === this.vm.id) {
            console.log(event.action + ' ' + event.model.id);
            if (event.action === 'VM.DELETE') { this.vm = {}; }
            this.refresh();
          }
          if (event.action === 'VM.DEPLOY' && +event.model.id === this.template.id) {
            this.refresh();
          }
        }
      )
    );
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    if (this.timer) { clearTimeout(this.timer); }
  }

  vmStatus(): string {
    return 'testing';
  }
  vmTask(): string {
    return 'tasking';
  }

  load() {
    if (!this.template.id && !this.vm.id) { return; }

    const q = (this.vm && this.vm.id)
      ? this.vmSvc.getVm(this.vm.id)
      : this.vmSvc.getTemplateVm(this.template.id);

      q.subscribe(
      (vm: Vm) => {
        this.vm = vm;
        this.taskRunning = this.vm.task && this.vm.task.progress < 100;
        if (this.taskRunning) {
          if (this.timer) { clearTimeout(this.timer); }
          this.timer = setTimeout(() => { this.load(); }, 3000);
        } else {
          this.vm.task = null;
          this.loaded.emit(this.vm);
        }
      },
      (err) => { this.onError(err.error || err); }
    );
  }

  refresh() {
    this.setTask('load');
    if (this.timer) { clearTimeout(this.timer); }
    this.timer = setTimeout(() => { this.load(); }, 2000);
  }

  deploy() {
    this.setTask('deploy');
    this.vmSvc.postTemplateDeploy(this.template.id)
    .subscribe(
        data => {
            this.vm = data;
            this.loaded.emit(this.vm);
        },
        (err) => { this.onError(err.error || err); },
        () => { this.vm.task = null; }
    );
  }

  vmaction(type: VmOperationTypeEnum): void {
    this.setTask(type.toString());
    this.vmSvc.postVmAction({
        id: this.vm.id,
        type: type,
        workspaceId: this.template.topologyId
    }).subscribe(
      (vm: Vm) => {
        this.vm = vm;
        if (this.vm.task && this.vm.task.name) {
          this.load();
        }
      },
      (err) => {
        this.onError(err.error || err);
      }
    );
  }

  toggleConfirm(): void {
    this.confirmingDelete = !this.confirmingDelete;
  }

  delete() {
    this.confirmingDelete = false;
    this.setTask('delete');
    this.vmSvc.deleteVm(this.vm.id)
      .subscribe(
        () => {
            this.vm = {};
            this.loaded.emit(this.vm);
            this.refresh();
        },
        (err) => { this.onError(err.error || err); });
  }

  initialize() {
    this.setTask('initializing');
    this.vmSvc.postTemplateDisks(this.template.id)
    .subscribe(() => {
            this.load();
        }, (err) => { this.onError(err.error || err); });
  }

  display() {
    this.vmSvc.openConsole(this.vm.id, this.vm.name);
  }

  errorCleared() {
    this.refresh();
  }

  setTask(task: string): void {
    this.vm.task = { name: task, progress: -1 };
    this.taskRunning = true;
  }

  canSave(): boolean {
    return !!this.vm.id && !!this.template.id && !this.template.parentId;
  }

  onError(e: Error): void {
    this.errors.push(e);
    this.vm.task = null;
  }
}
