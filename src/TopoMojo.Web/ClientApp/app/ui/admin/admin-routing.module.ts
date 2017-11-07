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

const routes: Routes = [
    {
        path: 'admin',
        component: AdminComponent,
        canActivate: [ AdminGuard, AuthGuard ],
        children: [
            {
                path: '',
                //canActivateChild: [ AdminGuard, AuthGuard ],
                children: [
                    { path: 'templates', component: TemplateManagerComponent },
                    { path: 'users', component: UserManagerComponent },
                    { path: 'games', component: GameManagerComponent },
                    { path: 'topos', component: TopoManagerComponent }
                    // { path: '', component: AdminComponent }
                ]
            }
        ]
    },
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
        TemplateManagerComponent
    ]
}