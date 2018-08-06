
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedGamespaceService } from "./gen/gamespace.service";
import { LayoutService } from "../svc/layout.service";

@Injectable()
export class GamespaceService extends GeneratedGamespaceService {

    constructor(
       protected http: HttpClient,
       private layoutSvc: LayoutService
    ) { super(http); }

    public openConsole(id, name) {
        this.layoutSvc.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

    public getText(url : string) : Observable<string> {
        return this.http.get(url, { responseType: "text"});
    }
}
