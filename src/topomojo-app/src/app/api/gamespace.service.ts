
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedGamespaceService } from './gen/gamespace.service';
import { GameState, Gamespace, Player, VmState } from './gen/models';
import { SettingsService } from '../svc/settings.service';

@Injectable()
export class GamespaceService extends GeneratedGamespaceService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings,
       private settingsSvc: SettingsService
    ) { super(http, api); }

    public openConsole(id, name) {
        this.settingsSvc.showTab('/console/' + id + '/' + name.match(/[^#]*/)[0]);
    }

    public getText(url: string): Observable<string> {
        return this.http.get(url, { responseType: 'text'});
    }
}
