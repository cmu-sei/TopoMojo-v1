import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService } from '../auth/auth-guard.service';
import { TopoComponent } from './topo.component';
import { TopoBrowserComponent } from './topo-browser.component';
import { WorkBrowserComponent } from './work-browser.component';
import { TopoDetailComponent } from './topo-detail.component';
import { TopoCreatorComponent } from './topo-creator.component';
import { TopoMembersComponent } from './topo-members.component';
import { TemplateEditorComponent} from './template-editor.component';
import { TopoEnlistComponent } from './enlist.component';
import { ConnectionResolver } from '../shared/connection.resolver';
import { IsoManagerComponent } from './iso-manager.component';

const routes: Routes = [
    {
        path: 'browse',
        component: TopoComponent,
        //canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                component: TopoBrowserComponent
            }
        ]
    },
    {
        path: 'topo',
        component: TopoComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                //canActivateChild: [ AuthGuardService ],
                children: [
                    {
                        path: ':id',
                        component: TopoDetailComponent,
                        resolve: { connection: ConnectionResolver }
                     },
                    { path: '', component: WorkBrowserComponent },
                ]
            }
        ]
    },
    {
        path: 'enlist',
        component: TopoComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                //canActivateChild: [ AuthGuardService ],
                children: [
                    { path: ':code', component: TopoEnlistComponent },
                    { path: '', component: TopoComponent }
                ]
            }
        ]
    }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class TopoRoutingModule {
    static components = [
        TopoComponent,
        TopoBrowserComponent,
        TopoDetailComponent,
        TopoCreatorComponent,
        TopoMembersComponent,
        TemplateEditorComponent,
        WorkBrowserComponent,
        TopoEnlistComponent,
        IsoManagerComponent
     ]
}