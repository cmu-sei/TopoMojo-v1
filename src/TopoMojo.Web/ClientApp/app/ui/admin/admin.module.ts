import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AdminRoutingModule } from './admin-routing.module';
import { AdminGuard } from '../../svc/admin-guard.service';
import { VmModule } from '../vm/vm.module';

@NgModule({
    imports: [ SharedModule, AdminRoutingModule, VmModule ],
    declarations: [ AdminRoutingModule.components ],
    providers: [ AdminGuard ]
})
export class AdminModule { }