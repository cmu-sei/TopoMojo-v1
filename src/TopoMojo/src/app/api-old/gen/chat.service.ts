
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs';
import { GeneratedService } from "./_service";
import { ChangedMessage,Message,NewMessage } from "./models";

@Injectable()
export class GeneratedChatService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getChat(id: string, marker: number, take: number) : Observable<Array<Message>> {
		return this.http.get<Array<Message>>(this.hostUrl + '/api/chats/' + id + this.paramify({marker: marker, take: take}));
	}
	public deleteChat(id: number) : Observable<Message> {
		return this.http.delete<Message>(this.hostUrl + "/api/chat/" + id);
	}
	public putChat(model: ChangedMessage) : Observable<Message> {
		return this.http.put<Message>(this.hostUrl + "/api/chat", model);
	}
	public postChat(model: NewMessage) : Observable<Message> {
		return this.http.post<Message>(this.hostUrl + "/api/chat", model);
	}

}

