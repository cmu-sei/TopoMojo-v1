
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { GameState, Gamespace, Player, VmState } from './models';

@Injectable()
export class GeneratedGamespaceService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getGamespaces(filter: string): Observable<Array<Gamespace>> {
        return this.http.get<Array<Gamespace>>(this.api.url + '/api/gamespaces' + this.paramify({filter: filter}));
    }
    public getGamespacePreview(id: number): Observable<GameState> {
        return this.http.get<GameState>(this.api.url + '/api/gamespace/' + id + '/preview');
    }
    public getGamespace(id: number): Observable<GameState> {
        return this.http.get<GameState>(this.api.url + '/api/gamespace/' + id);
    }
    public deleteGamespace(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/gamespace/' + id);
    }
    public postGamespaceLaunch(id: number): Observable<GameState> {
        return this.http.post<GameState>(this.api.url + '/api/gamespace/' + id + '/launch', {});
    }
    public getGamespaceState(id: number): Observable<GameState> {
        return this.http.get<GameState>(this.api.url + '/api/gamespace/' + id + '/state');
    }
    public postPlayerCode(code: string): Observable<boolean> {
        return this.http.post<boolean>(this.api.url + '/api/player/enlist/' + code, {});
    }
    public deletePlayer(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/player/' + id);
    }
    public getGamespacePlayers(id: number): Observable<Array<Player>> {
        return this.http.get<Array<Player>>(this.api.url + '/api/gamespace/' + id + '/players');
    }

}
