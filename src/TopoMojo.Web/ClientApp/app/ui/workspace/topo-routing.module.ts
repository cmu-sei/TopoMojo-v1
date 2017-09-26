import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';
import { TopoComponent } from './topo.component';
import { TopoBrowserComponent } from './topo-browser.component';
import { WorkBrowserComponent } from './work-browser.component';
import { TopoDetailComponent } from './topo-detail.component';
import { TopoCreatorComponent } from './topo-creator.component';
import { TopoMembersComponent } from './topo-members.component';
import { TemplateEditorComponent} from './template-editor.component';
import { TopoEnlistComponent } from './enlist.component';
import { ConnectionResolver } from '../../svc/connection.resolver';
import { IsoManagerComponent } from './iso-manager.component';

const routes: Routes = [
    {
        path: 'browse',
        component: TopoComponent,
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
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                children: [
                    {
                        path: ':id',
                        component: TopoDetailComponent
                        //resolve: { connection: ConnectionResolver }
                    },
                    {
                        path: 'enlist',
                        children: [
                            { path: ':code', component: TopoEnlistComponent }
                        ]
                    },
                    { path: '', component: WorkBrowserComponent },
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