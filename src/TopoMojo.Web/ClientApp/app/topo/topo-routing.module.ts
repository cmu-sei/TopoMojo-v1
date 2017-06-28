import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService } from '../auth/auth-guard.service';
import { TopoComponent } from './topo.component';
import { TopoBrowserComponent } from './topo-browser.component';
import { WorkBrowserComponent } from './work-browser.component';
import { TopoLaunchComponent } from './topo-launch.component';
import { TopoDetailComponent } from './topo-detail.component';
import { TopoCreatorComponent } from './topo-creator.component';
import { TopoMembersComponent } from './topo-members.component';
import { TemplateEditorComponent} from './template-editor.component';
import { GamespaceComponent } from './gamespace.component';
import { TopoEnlistComponent } from './enlist.component';

const routes: Routes = [
    {
        path: 'browse',
        component: TopoBrowserComponent,
        canActivate: [AuthGuardService]
    },
    {
        path: 'topo',
        component: TopoComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                canActivateChild: [ AuthGuardService ],
                children: [
                    { path: ':id', component: TopoDetailComponent },
                    { path: '', component: WorkBrowserComponent },
                    // { path: '', component: TopoBrowserComponent }
                ]
            }
        ]
    },
    {
        path: 'mojo',
        component: TopoComponent,
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                canActivateChild: [ AuthGuardService ],
                children: [
                    { path: ':id', component: TopoLaunchComponent },
                    { path: '', component: GamespaceComponent }
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
                canActivateChild: [ AuthGuardService ],
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
        TopoLaunchComponent,
        TopoDetailComponent,
        TopoCreatorComponent,
        TopoMembersComponent,
        TemplateEditorComponent,
        WorkBrowserComponent,
        GamespaceComponent,
        TopoEnlistComponent
     ]
}