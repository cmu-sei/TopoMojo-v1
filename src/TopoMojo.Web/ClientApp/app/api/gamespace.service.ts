
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedGamespaceService } from "./gen/gamespace.service";
import { GameState,Gamespace,VmState } from "./gen/models";
import { SettingsService } from '../svc/settings.service';

@Injectable()
export class GamespaceService extends GeneratedGamespaceService {

    constructor(
       protected http: HttpClient,
       private settings: SettingsService
    ) { super(http); }

    public openConsole(id, name) {
        this.settings.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

    public getText(url : string) : Observable<string> {
        return this.http.get(url, { responseType: "text"});
    }
}
