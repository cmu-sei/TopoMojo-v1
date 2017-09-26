import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class ChatService {

    constructor(
        private http: HttpClient,
    ) { }

    members(id: string) : Observable<Array<ActorModel>> {
        return this.http.get<Array<ActorModel>>('/api/gamespace/' + id + '/members');
    }

    messages(id: string, page : PageModel) : Observable<Array<MessageModel>> {
        return this.http.get<Array<MessageModel>>('/ap/gamespace/' + id + '/messages', { params: page});
    }

    post(id: string, msg : MessageModel) : Observable<void> {
        return this.http.post<void>('/api/gamespace/' + id + '/message', msg);
    }


}

export class MessageModel {
    actor: string;
    text: string;
    time: string;
}

export class ActorModel {
    id: number;
    name: string;
    online: boolean;
}

export class PageModel extends HttpParams {
    skip: number;
    take: number;
}
