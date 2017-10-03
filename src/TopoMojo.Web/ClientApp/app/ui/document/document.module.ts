import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { DocumentRoutingModule } from './document-routing.module';

@NgModule({
    imports: [ SharedModule, DocumentRoutingModule ],
    declarations: [ DocumentRoutingModule.components ]
})
export class DocumentModule { }