import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { ConsoleComponent } from './console.component';
import { AuthGuardService } from '../auth/auth-guard.service';

const routes: Routes = [
    { path: 'console', component: ConsoleComponent, canActivate: [AuthGuardService]}
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class ConsoleRoutingModule {
    static components = [ ConsoleComponent ]
}