import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { ApiModule } from '../../api/api.module';
import { GamespaceRoutingModule } from './gamespace.routing';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ChatModule } from '../chat/chat.module';

@NgModule({
    imports: [ SharedModule, ApiModule, VmModule, GamespaceRoutingModule, DocumentModule, ChatModule ],
    declarations: [ GamespaceRoutingModule.components ],
    providers: []
})
export class GamespaceModule { }