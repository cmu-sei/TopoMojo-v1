
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Observable';
import { GeneratedService } from "./_service";
import { GameState,Gamespace,Player,VmState } from "./models";

@Injectable()
export class GeneratedGamespaceService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getGamespaces() : Observable<Array<Gamespace>> {
		return this.http.get<Array<Gamespace>>("/api/gamespaces");
	}
	public allGamespaces() : Observable<Array<Gamespace>> {
		return this.http.get<Array<Gamespace>>("/api/gamespaces/all");
	}
	public getGamespace(id: number) : Observable<GameState> {
		return this.http.get<GameState>("/api/gamespace/" + id);
	}
	public deleteGamespace(id: number) : Observable<boolean> {
		return this.http.delete<boolean>("/api/gamespace/" + id);
	}
	public launchGamespace(id: number) : Observable<GameState> {
		return this.http.get<GameState>("/api/gamespace/" + id + "/launch");
	}
	public stateGamespace(id: number) : Observable<GameState> {
		return this.http.get<GameState>("/api/gamespace/" + id + "/state");
	}
	public enlistPlayer(code: string) : Observable<boolean> {
		return this.http.get<boolean>("/api/player/enlist/" + code);
	}
	public delistPlayer(playerId: number) : Observable<boolean> {
		return this.http.delete<boolean>("/api/player/delist/" + playerId);
	}
	public playersGamespace(id: number) : Observable<Array<Player>> {
		return this.http.get<Array<Player>>("/api/gamespace/" + id + "/players");
	}

}

