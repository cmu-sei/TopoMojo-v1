
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedGamespaceService } from "./gen/gamespace.service";
import { Gamespace,GameState,VmState } from "./gen/models";
import { ExternalNavService } from "../shared/external-nav.service";

@Injectable()
export class GamespaceService extends GeneratedGamespaceService {

    constructor(
       protected http: HttpClient,
       private nav: ExternalNavService
    ) { super(http); }

    public openConsole(id, name) {
        this.nav.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

    public getText(url : string) : Observable<string> {
        return this.http.get(url, { responseType: "text"});
    }
}
