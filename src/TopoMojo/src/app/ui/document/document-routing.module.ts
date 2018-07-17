import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';
import { DocumentComponent } from './document.component';
import { DocumentEditorComponent } from './document-editor.component';
import { ImageManagerComponent } from './image-manager.component';

const routes: Routes = [
    // {
    //     path: 'doc',
    //     component: DocumentComponent,
    //     canActivate: [AuthGuard],
    //     children: [
    //         {
    //             path: ':id',
    //             component: DocumentBuilderComponent
    //         }
    //     ]
    // }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class DocumentRoutingModule {
    static components = [
        DocumentComponent,
        DocumentEditorComponent,
        ImageManagerComponent
     ]
}