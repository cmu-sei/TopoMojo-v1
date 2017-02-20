import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AdminRoutingModule } from './admin-routing.module';
import { AdminService } from './admin.service';
import { AdminGuard } from './admin-guard.service';

@NgModule({
    imports: [ SharedModule, AdminRoutingModule ],
    declarations: [ AdminRoutingModule.components ],
    providers: [ AdminService, AdminGuard ]
})
export class AdminModule { }