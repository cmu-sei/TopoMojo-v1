import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { GamespaceRoutingModule } from './gamespace.routing';
import { GamespaceService } from './gamespace.service';
import { VmModule } from '../vm/vm.module';
import { DocumentModule } from '../document/document.module';
import { ProfileModule } from '../profile/profile.module';
import { ChatModule } from '../chat/chat.module';

@NgModule({
    imports: [ SharedModule, VmModule, GamespaceRoutingModule, DocumentModule, ProfileModule, ChatModule ],
    declarations: [ GamespaceRoutingModule.components ],
    providers: [ GamespaceService ]
})
export class GamespaceModule { }