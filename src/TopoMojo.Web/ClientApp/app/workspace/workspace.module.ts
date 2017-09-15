import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { TopoRoutingModule } from './topo-routing.module';
import { TopologyService } from '../api/topology.service';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ProfileModule } from '../profile/profile.module';
import { ApiModule } from '../api/api.module';

@NgModule({
    imports: [
        SharedModule,
        ApiModule,
        VmModule,
        TopoRoutingModule,
        DocumentModule,
        ProfileModule
    ],
    declarations: [ TopoRoutingModule.components ],
    providers: [  ]
})
export class WorkspaceModule { }