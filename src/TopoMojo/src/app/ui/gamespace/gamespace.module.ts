import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { GamespaceRoutingModule } from './gamespace.routing';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ChatModule } from '../chat/chat.module';

@NgModule({
    imports: [ SharedModule, VmModule, GamespaceRoutingModule, DocumentModule, ChatModule ],
    declarations: [ GamespaceRoutingModule.components ],
    providers: []
})
export class GamespaceModule { }
