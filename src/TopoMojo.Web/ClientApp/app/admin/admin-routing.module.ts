import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService } from '../auth/auth-guard.service';
import { AdminGuard } from './admin-guard.service';
import { AdminComponent } from './admin.component';
import { AdminHomeComponent } from './admin-home.component';
import { UserManagerComponent } from './user-manager.component';
import { TemplateManagerComponent } from './template-manager.component';

const routes: Routes = [
    {
        path: 'admin',
        component: AdminComponent,
        canActivate: [ AdminGuard, AuthGuardService ],
        children: [
            {
                path: '',
                //canActivateChild: [ AdminGuard, AuthGuardService ],
                children: [
                    { path: 'templates', component: TemplateManagerComponent },
                    { path: 'users', component: UserManagerComponent },
                    //{ path: '', component: AdminComponent }
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
        TemplateManagerComponent
    ]
}