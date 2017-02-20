import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../core/auth-guard.service';
import { TopoComponent } from './topo.component';
import { TopoBrowserComponent } from './topo-browser.component';
import { TopoDetailComponent } from './topo-detail.component';
import { TopoCreatorComponent } from './topo-creator.component';
import { TopoMembersComponent } from './topo-members.component';
import { TemplateEditorComponent} from './template-editor.component';

const routes: Routes = [
    {
        path: 'topo',
        component: TopoComponent,
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                canActivateChild: [ AuthGuard ],
                children: [
                    { path: ':id', component: TopoDetailComponent },
                    { path: '', component: TopoBrowserComponent }
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
        TemplateEditorComponent
     ]
}