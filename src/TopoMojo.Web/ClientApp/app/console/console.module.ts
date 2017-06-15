import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { ConsoleRoutingModule } from './console-routing.module';

@NgModule({
    imports: [ SharedModule, ConsoleRoutingModule ],
    declarations: [ ConsoleRoutingModule.components ],
})
export class ConsoleModule { }