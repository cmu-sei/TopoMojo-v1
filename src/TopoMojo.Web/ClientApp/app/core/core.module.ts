import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { CoreRoutingModule } from './core-routing.module';
import { SharedModule } from '../shared/shared.module';
import { NavbarComponent } from './navbar.component';

@NgModule({
    imports: [
        HttpModule,
        SharedModule,
        CoreRoutingModule
    ],
    declarations: [ CoreRoutingModule.components ],
    exports: [ NavbarComponent ],
    providers: [ ]
})
export class CoreModule { }