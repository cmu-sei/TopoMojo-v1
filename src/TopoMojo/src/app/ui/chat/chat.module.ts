import { NgModule } from '@angular/core';
import {SharedModule } from '../shared/shared.module';
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
     ]
})
export class ChatModule { }
