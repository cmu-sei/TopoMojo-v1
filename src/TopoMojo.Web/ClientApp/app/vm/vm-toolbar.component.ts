import { Component, OnInit, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { VmService } from "../api/vm.service";
import { Template, VirtualVm, VirtualVmAnswer, VirtualVmStateEnum } from "../api/api-models";

@Component({
    //moduleId: module.id,
    selector: 'vm-toolbar',
    templateUrl: 'vm-toolbar.component.html',
    styleUrls: [ 'vm-toolbar.component.css']

})
export class VmToolbarComponent implements OnChanges {
    @Input() template: Template;
    @Output() onLoaded: EventEmitter<any> = new EventEmitter<any>();
    vm: VirtualVm;
    status: string;
    timer: any;
    working: boolean = true;
    error: string;

    constructor(
        private service: VmService
    ) { }

    // ngOnInit() {
    //     //setInterval(()=> {this.refresh()}, 4000);
    //     //this.refresh();
    // }
    ngOnChanges(changes: SimpleChanges) {
        if (changes['template']) {
            this.status = "created";
            this.startRefresh();
        }
    }

    refresh() {
        let svc = (this.vm)
            ? this.service.loadVm(this.vm.id)
            : this.service.resolveVm(this.template.id);
        svc.subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            if (this.vm.task && this.vm.task.progress < 100) {
                this.startRefresh();
            }
            else {
                this.working = false;
                //setTimeout(() => { this.startRefresh(); }, 10000);
            }
        }, (err) => { this.onError(err) });
    }

    startRefresh() {
        this.working = true;
        if (this.timer) clearTimeout(this.timer);
        this.timer = setTimeout(() => { this.refresh(); }, 2000);
    }

    initialize() {
        this.working = true;
        this.vm.task = { name: 'initializing', progress: 0 };
        this.service.initVm(this.template.id)
        .subscribe(result => {
           this.startRefresh();
        }, (err) => { this.onError(err); });
    }

    deploy() {
        this.working = true;
        this.vm.task = { name: "deploying" };
        this.service.deployVm(this.template.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); });
    }

    delete() {
        this.working = true;
        this.vm.task = { name: "deleting" };
        this.service.deleteVm(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.refresh();
        }, (err) => { this.onError(err); })
    }

    start() {
        this.working = true;
        this.vm.task = { name: "starting" };
        this.service.startVm(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    stop() {
        this.working = true;
        this.vm.task = { name: "stopping" };
        this.service.stopVm(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    revert() {
        this.working = true;
        this.vm.task = { name: "reverting" };
        this.service.revertVm(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    save() {
        this.working = true;
        this.vm.task = { name: "saving" };
        this.service.saveVm(this.vm.id)
        .subscribe(data => {
            this.startRefresh();
        }, (err) => { this.onError(err); });
    }

    answer(c) {
        this.service.answerVm(this.vm.id, c as VirtualVmAnswer)
        .subscribe(data => {
            this.vm = data;
        });
    }

    isRunning() {
        return this.vm.state == VirtualVmStateEnum.running;
    }

    isLinked() {
        return (this.template.parent);
    }

    display() {
        this.service.ticketVm(this.vm.id);
    }

    clearError() {
        this.error = null;
        this.startRefresh();
    }

    onError(err) {
        this.error = JSON.parse(err.text()).message;
    }
}