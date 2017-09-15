
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { Gamespace,GameState,VmState } from "./api-models";

@Injectable()
export class GamespaceService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public getGamespaces() : Observable<Array<Gamespace>> {
		return this.http.get("/api/gamespaces");
	}
	public getGamespace(id: number) : Observable<GameState> {
		return this.http.get("/api/gamespace/" + id);
	}
	public deleteGamespace(id: number) : Observable<boolean> {
		return this.http.delete("/api/gamespace/" + id);
	}
	public launchGamespace(id: number) : Observable<GameState> {
		return this.http.get("/api/gamespace/" + id + "/launch");
	}
	public stateGamespace(id: number) : Observable<GameState> {
		return this.http.get("/api/gamespace/" + id + "/state");
	}
	public enlistPlayer(code: string) : Observable<boolean> {
		return this.http.get("/api/player/enlist/" + code);
	}
	public delistPlayer(playerId: number) : Observable<boolean> {
		return this.http.delete("/api/player/delist/" + playerId);
	}
}
