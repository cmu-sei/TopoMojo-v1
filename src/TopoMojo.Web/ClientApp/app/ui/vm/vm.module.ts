import { NgModule } from '@angular/core';

import { SharedModule } from '../shared/shared.module';
import { VmToolbarComponent } from './vm-toolbar.component';
//import { VmService } from './vm.service';

@NgModule({
    imports: [ SharedModule ],
    declarations: [ VmToolbarComponent ],
    exports: [VmToolbarComponent],
    providers: []
})
export class VmModule { }