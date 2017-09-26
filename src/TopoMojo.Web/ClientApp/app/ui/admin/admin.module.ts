import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AdminRoutingModule } from './admin-routing.module';
import { AdminGuard } from '../../svc/admin-guard.service';

@NgModule({
    imports: [ SharedModule, AdminRoutingModule ],
    declarations: [ AdminRoutingModule.components ],
    providers: [ AdminGuard ]
})
export class AdminModule { }