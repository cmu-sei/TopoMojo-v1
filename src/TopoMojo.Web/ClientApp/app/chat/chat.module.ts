import { NgModule } from '@angular/core';
import {SharedModule } from '../shared/shared.module';
import { ChatService } from './chat.service';
import { MessagesComponent } from './messages.component';
import { MessageComponent } from './message.component';

@NgModule({
    imports: [ SharedModule ],
    declarations: [
        MessagesComponent,
        MessageComponent
     ],
     exports: [
         MessagesComponent
     ],
     providers: [
         ChatService
     ]
})
export class ChatModule { }