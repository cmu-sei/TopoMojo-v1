import { Component, OnInit, Input, Output, OnChanges, SimpleChanges, EventEmitter } from '@angular/core';
import { VmService } from './vm.service';

@Component({
    //moduleId: module.id,
    selector: 'vm-toolbar',
    templateUrl: 'vm-toolbar.component.html',
    styleUrls: [ 'vm-toolbar.component.css']

})
export class VmToolbarComponent implements OnChanges {
    @Input() tref: any;
    @Output() onLoaded: EventEmitter<any> = new EventEmitter<any>();
    vm: any = { status: null };
    timer: any;
    working: boolean = true;
    error: string;

    constructor(private service: VmService) { }

    // ngOnInit() {
    //     //setInterval(()=> {this.refresh()}, 4000);
    //     //this.refresh();
    // }
    ngOnChanges(changes: SimpleChanges) {
        if (changes['tref']) {
            this.vm.Status = "created";
            this.startRefresh();
        }
    }

    refresh() {
        this.service.refresh(this.tref.id)
        .subscribe(data => {
            this.vm = data as any;
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
        this.service.initialize(this.tref.id)
        .subscribe(result => {
           this.startRefresh();
        }, (err) => { this.onError(err); });
    }

    deploy() {
        this.working = true;
        this.vm.task = { name: "deploying" };
        this.service.deploy(this.tref.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); });
    }

    delete() {
        this.working = true;
        this.vm.task = { name: "deleting" };
        this.service.delete(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.refresh();
        }, (err) => { this.onError(err); })
    }

    start() {
        this.working = true;
        this.vm.task = { name: "starting" };
        this.service.start(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    stop() {
        this.working = true;
        this.vm.task = { name: "stopping" };
        this.service.stop(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    revert() {
        this.working = true;
        this.vm.task = { name: "reverting" };
        this.service.revert(this.vm.id)
        .subscribe(data => {
            this.vm = data;
            this.onLoaded.emit(this.vm);
            this.working = false;
        }, (err) => { this.onError(err); })
    }

    save() {
        this.working = true;
        this.vm.task = { name: "saving" };
        this.service.save(this.vm.id)
        .subscribe(data => {
            this.startRefresh();
        }, (err) => { this.onError(err); });
    }

    answer(c) {
        this.service.answer(this.vm.id, this.vm.question.id, c.key)
        .subscribe(data => {
            this.vm = data;
        });
    }

    isRunning() {
        return this.vm.state == 1;
    }

    display() {
        this.service.display(this.vm.id);
    }

    clearError() {
        this.error = null;
        this.startRefresh();
    }

    onError(err) {
        this.error = JSON.parse(err.text()).message;
        this.service.onError(err);
    }
}