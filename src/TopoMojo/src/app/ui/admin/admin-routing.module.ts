import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';
import { AdminGuard } from '../../svc/admin-guard.service';
import { AdminComponent } from './admin.component';
import { AdminHomeComponent } from './admin-home.component';
import { UserManagerComponent } from './user-manager.component';
import { GameManagerComponent } from './game-manager.component';
import { TopoManagerComponent } from './topo-manager.component';
import { TemplateManagerComponent } from './template-manager.component';
import { AdminSettingsComponent } from './settings.component';
import { VmManagerComponent } from './vm-manager.component';

const routes: Routes = [
    {
        path: 'admin',
        component: AdminComponent,
        canActivate: [ AdminGuard, AuthGuard ],
        children: [
            {
                path: '',
                children: [
                    { path: 'templates', component: TemplateManagerComponent },
                    { path: 'users', component: UserManagerComponent },
                    { path: 'games', component: GameManagerComponent },
                    { path: 'vms', component: VmManagerComponent },
                    { path: 'topos', component: TopoManagerComponent },
                    { path: 'settings', component: AdminSettingsComponent },
                ]
            }
        ]
    }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class AdminRoutingModule {
    static components = [
        AdminComponent,
        AdminHomeComponent,
        UserManagerComponent,
        GameManagerComponent,
        TopoManagerComponent,
        TemplateManagerComponent,
        AdminSettingsComponent,
        VmManagerComponent
    ]
}