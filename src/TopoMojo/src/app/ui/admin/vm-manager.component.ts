import { Component, OnInit } from '@angular/core';
import { VmService } from '../../api/vm.service';
import { VirtualVm, VirtualVmStateEnum } from '../../api/gen/models';
import { SettingsService } from '../../svc/settings.service';

@Component({
    selector: 'vm-manager',
    templateUrl: 'vm-manager.component.html',
    styleUrls: [ 'vm-manager.component.css' ]
})
export class VmManagerComponent implements OnInit {

    constructor(
        private vmSvc: VmService    ) { }

    machines: Array<VirtualVm>;

    ngOnInit() {

    }

    load() : void {
        this.vmSvc.findVms("").subscribe(
            (vms) => {
                this.machines = vms;
            }
        );
    }

    launch(vm: VirtualVm) : void {
        this.vmSvc.openConsole(vm.id, vm.name);
    }

    destroy(vm: VirtualVm) : void {
        this.vmSvc.deleteVm(vm.id).subscribe(
            (result) => {
                this.machines.splice(this.machines.indexOf(vm), 1);
            }
        )
    }
}