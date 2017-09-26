import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { DocumentEditorComponent } from './document-editor.component';
import { ImageManagerComponent } from './image-manager.component';

@NgModule({
    imports: [ SharedModule ],
    declarations: [ ImageManagerComponent, DocumentEditorComponent ],
    exports: [ImageManagerComponent, DocumentEditorComponent],
    providers: [  ]
})
export class DocumentModule { }