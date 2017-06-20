import { NgModule } from '@angular/core';
import { RouterModule, Routes, ActivatedRouteSnapshot } from '@angular/router';
import { CoreAuthGuard } from './auth-guard.service';
import { HomeComponent } from './home.component';
import { NotFoundComponent } from './notfound.component';
import { NotAllowedComponent } from './notallowed.component';
//import { LoginComponent } from './login.component';
import { ResetComponent } from './reset.component';
import { AboutPanelComponent } from './about-panel.component';
import { HelpPanelComponent } from './help-panel.component';
import { ProfileEditorComponent } from './profile.component';

const routes: Routes = [
    { path: 'home', component: HomeComponent },
    //{ path: 'login', component: LoginComponent },
    { path: 'reset', component: ResetComponent },
    { path: 'profile', component: ProfileEditorComponent },
    { path: 'about', component: AboutPanelComponent },
    { path: 'help' , component: HelpPanelComponent },
    { path: 'notfound', component: NotFoundComponent },
    { path: 'notallowed', component: NotAllowedComponent },
    //{ path: 'admin', loadChildren: 'app/admin/admin.module#AdminModule'},
    // { path: 'topo', loadChildren: 'app/topo/topo.module#TopoModule'}
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class CoreRoutingModule {
    static components = [
        HomeComponent,
        NotFoundComponent,
        NotAllowedComponent,
        //LoginComponent,
        ResetComponent,
        AboutPanelComponent,
        HelpPanelComponent,
        ProfileEditorComponent
    ]
}