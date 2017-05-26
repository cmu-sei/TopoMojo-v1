import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { TopoRoutingModule } from './topo-routing.module';
import { TopoService } from './topo.service';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';

@NgModule({
    imports: [ SharedModule, VmModule, TopoRoutingModule, DocumentModule ],
    declarations: [ TopoRoutingModule.components ],
    providers: [ TopoService ]
})
export class TopoModule { }