import { Component, Input, Output, OnChanges, SimpleChanges, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { VmService } from '../../api/vm.service';
import { Template, Vm, VmAnswer, VmOperationTypeEnum } from '../../api/gen/models';
import { NotificationService } from '../../svc/notification.service';
import { Subscription } from 'rxjs';

@Component({
    // moduleId: module.id,
    selector: 'app-vm-toolbar',
    templateUrl: 'vm-toolbar.component.html',
    styleUrls: [ 'vm-toolbar.component.css']

})
export class VmToolbarComponent implements OnInit, OnChanges, OnDestroy {
    @Input() template: Template;
    @Input() isPublished: boolean;
    @Input() vm: Vm;
    @Output() loaded: EventEmitter<any> = new EventEmitter<any>();
    status: string;
    timer: any;
    working = true;
    errors: any[] = [];
    subs: Subscription[] = [];
    constructor(
        private service: VmService,
        private notifier: NotificationService
    ) { }

    ngOnInit() {
        this.startRefresh();
    }

    ngOnChanges(changes: SimpleChanges) {
        if (changes['template']) {
            this.status = 'created';
            this.startRefresh();
            this.subs.push(
                this.notifier.topoEvents.subscribe(
                    (event) => {
                        if (this.template.name === event.model.name
                            || event.model.id === this.vm.id) {

                            switch (event.action) {
                                case 'VM.DELETE':
                                this.vm = null;
                                break;

                                // default:
                                // this.startRefresh();
                                // break;
                            }
                            this.startRefresh();

                        }
                    }
                ),
            );
        }
    }

    ngOnDestroy() {
        this.subs.forEach(
            (sub) => {
                sub.unsubscribe();
            }
        );
    }

    refresh() {
        const svc = (this.vm && this.vm.id)
            ? this.service.getVm(this.vm.id)
            : this.service.getTemplateVm(this.template.id);
        svc.subscribe(data => {
            this.vm = data;
            this.loaded.emit(this.vm);
            if (this.vm.task && this.vm.task.progress < 100) {
                this.startRefresh();
            } else {
                this.working = false;
                // setTimeout(() => { this.startRefresh(); }, 10000);
            }
        }, (err) => { this.onError(err); });
    }

    startRefresh() {
        this.working = true;
        if (this.timer) { clearTimeout(this.timer); }
        this.timer = setTimeout(() => { this.refresh(); }, 2000);
    }

    initialize() {
        this.working = true;
        this.vm.task = { name: 'initializing', progress: 0 };
        this.service.postTemplateDisks(this.template.id)
        .subscribe(() => {
                this.startRefresh();
            }, (err) => { this.onError(err); });
    }

    deploy() {
        this.working = true;
        this.vm.task = { name: 'deploying' };
        this.service.postTemplateDeploy(this.template.id)
        .subscribe(
            data => {
                this.vm = data;
                this.loaded.emit(this.vm);
            },
            (err) => { this.onError(err); },
            () => { this.working = false; }
        );
    }

    delete() {
        this.vm.status = 'confirm';
    }

    cancelDelete() {
        this.vm.status = 'deployed';
    }

    confirmDelete() {
        this.working = true;
        this.vm.task = { name: 'deleting' };
        this.service.deleteVm(this.vm.id)
        .subscribe(() => {
                this.vm = null;
                this.startRefresh();
            }, (err) => { this.onError(err); });
    }

    vmaction(type: VmOperationTypeEnum): void {
        this.working = true;
        this.vm.task = { name: type.toString() };
        this.service.postVmAction({
            id: this.vm.id,
            type: type,
            workspaceId: this.template.topologyId
        }).subscribe(
            (vm: Vm) => {
                this.vm = vm;
                this.loaded.emit(this.vm);
            },
            (err) => {
                this.onError(err);
            },
            () => { this.working = false; }
        );
    }
    start() {
        this.vmaction(VmOperationTypeEnum.start);
    //     this.working = true;
    //     this.vm.task = { name: 'starting' };
    //     this.service.getVmStart(this.vm.id)
    //     .subscribe(data => {
    //         this.vm = data;
    //         this.loaded.emit(this.vm);
    //         this.working = false;
    //     }, (err) => { this.onError(err); });
    }

    stop() {
        this.vmaction(VmOperationTypeEnum.stop);
    //     this.working = true;
    //     this.vm.task = { name: 'stopping' };
    //     this.service.getVmStop(this.vm.id)
    //     .subscribe(data => {
    //         this.vm = data;
    //         this.loaded.emit(this.vm);
    //         this.working = false;
    //     }, (err) => { this.onError(err); });
    }

    revert() {
        this.vmaction(VmOperationTypeEnum.revert);
    //     this.working = true;
    //     this.vm.task = { name: 'reverting' };
    //     this.service.getVmRevert(this.vm.id)
    //     .subscribe(data => {
    //         this.vm = data;
    //         this.loaded.emit(this.vm);
    //         this.working = false;
    //     }, (err) => { this.onError(err); });
    }

    save() {
        this.vmaction(VmOperationTypeEnum.save);
    //     this.working = true;
    //     this.vm.task = { name: 'saving' };
    //     this.service.postVmAction({
    //         type: 'save',
    //         id: this.vm.id,
    //         topoId: this.template.topologyId
    //     })
    //     .subscribe(() => {
    //             this.startRefresh();
    //         }, (err) => { this.onError(err); });
    }

    answer(c) {
        this.service.postVmAnswer(this.vm.id, { questionId: this.vm.question.id, choiceKey: c.key } as VmAnswer)
        .subscribe(data => {
            this.vm = data;
        });
    }

    isRunning() {
        return this.vm.state === 1; // VmStateEnum.running;
    }

    hasParent() {
        return (!this.template || this.template.parentId);
    }

    display() {
        this.service.openConsole(this.vm.id, this.vm.name);
    }

    errorCleared() {
        this.startRefresh();
    }

    onError(err) {
        this.errors.push(err.error);
        this.working = false;
    }

}
