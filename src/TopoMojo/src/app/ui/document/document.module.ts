import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { DocumentRoutingModule } from './document-routing.module';
import { DocumentEditorComponent } from './document-editor.component';

@NgModule({
    imports: [ SharedModule, DocumentRoutingModule ],
    declarations: [ DocumentRoutingModule.components ],
    exports: [ DocumentEditorComponent ]
})
export class DocumentModule { }
