import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { SharedModule } from '../shared/shared.module';
import { VmToolbarComponent } from './vm-toolbar.component';
import { ConsoleComponent } from './console.component';
import { AuthGuard } from '../../svc/auth-guard.service';

@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild([
            {
                path: 'console/:id/:name',
                component: ConsoleComponent,
                canActivate: [AuthGuard],
            }
        ])
    ],
    declarations: [ VmToolbarComponent, ConsoleComponent ],
    exports: [VmToolbarComponent, ConsoleComponent],
    providers: []
})
export class VmModule { }
