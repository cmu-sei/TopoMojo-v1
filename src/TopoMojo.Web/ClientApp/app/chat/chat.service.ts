import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { AuthHttp } from '../auth/auth-http';
import { SettingsService } from "../auth/settings.service";

import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class ChatService {

    constructor(
        private http: AuthHttp,
        private settings: SettingsService
    ) { }

    members(id: string) : Observable<Array<ActorModel>> {
        return this.http.get('/api/gamespace/' + id + '/members');
    }

    messages(id: string, page : PageModel) : Observable<Array<MessageModel>> {
        return this.http.get('/ap/gamespace/' + id + '/messages', this.http.queryStringify(page, '?'));
    }

    post(id: string, msg : MessageModel) : Observable<void> {
        return this.http.post('/api/gamespace/' + id + '/message', msg);
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

export class PageModel {
    skip: number;
    take: number;
}