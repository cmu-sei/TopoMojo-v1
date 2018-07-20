import { Component, OnInit } from '@angular/core';
import { VmService } from '../../api/vm.service';
import { Vm, VmStateEnum } from '../../api/gen/models';
import { SettingsService } from '../../svc/settings.service';

@Component({
    selector: 'app-vm-manager',
    templateUrl: 'vm-manager.component.html',
    styleUrls: [ 'vm-manager.component.css' ]
})
export class VmManagerComponent implements OnInit {

    constructor(
        private vmSvc: VmService    ) { }

    machines: Array<Vm>;

    ngOnInit() {

    }

    load(): void {
        this.vmSvc.getVms('').subscribe(
            (vms) => {
                this.machines = vms;
            }
        );
    }

    launch(vm: Vm): void {
        this.vmSvc.openConsole(vm.id, vm.name);
    }

    destroy(vm: Vm): void {
        this.vmSvc.deleteVm(vm.id).subscribe(
            (result) => {
                this.machines.splice(this.machines.indexOf(vm), 1);
            }
        );
    }
}
