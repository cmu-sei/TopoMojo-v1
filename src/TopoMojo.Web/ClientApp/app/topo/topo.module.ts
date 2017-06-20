import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { TopoRoutingModule } from './topo-routing.module';
import { TopoService } from './topo.service';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ProfileModule } from '../profile/profile.module';

@NgModule({
    imports: [ SharedModule, VmModule, TopoRoutingModule, DocumentModule, ProfileModule ],
    declarations: [ TopoRoutingModule.components ],
    providers: [ TopoService ]
})
export class TopoModule { }