
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedChatService } from './gen/chat.service';
import { ChangedMessage, Message, NewMessage } from './gen/models';

@Injectable()
export class ChatService extends GeneratedChatService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }
}
