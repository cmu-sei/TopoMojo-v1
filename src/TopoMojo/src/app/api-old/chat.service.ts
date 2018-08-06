
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedChatService } from "./gen/chat.service";
import { ChangedMessage,Message,NewMessage,TemplateDetail } from "./gen/models";

@Injectable()
export class ChatService extends GeneratedChatService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
