import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';
import { TopoComponent } from './topo.component';
import { TopoBrowserComponent } from './topo-browser.component';
import { WorkBrowserComponent } from './work-browser.component';
import { TopoCreatorComponent } from './topo-creator.component';
import { TopoMembersComponent } from './topo-members.component';
import { TemplateEditorComponent} from './template-editor.component';
import { TopoEnlistComponent } from './enlist.component';
import { IsoManagerComponent } from './iso-manager.component';
import { WorkspaceComponent } from './workspace.component';

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
                        component: WorkspaceComponent
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
export class WorkspaceRoutingModule {
    static components = [
        TopoComponent,
        TopoBrowserComponent,
        TopoCreatorComponent,
        TopoMembersComponent,
        TemplateEditorComponent,
        WorkBrowserComponent,
        TopoEnlistComponent,
        IsoManagerComponent,
        WorkspaceComponent
     ];
}
