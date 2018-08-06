
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedService } from "./_service";
import { ChangedTopology,GameState,NewTopology,Search,Template,Topology,TopologySearchResult,TopologyState,TopologySummary,TopologySummarySearchResult,VmOptions,Worker } from "./models";

@Injectable()
export class GeneratedTopologyService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public getTopologies(search: Search) : Observable<TopologySummarySearchResult> {
		return this.http.get<TopologySummarySearchResult>(this.hostUrl + "/api/topologies" + this.paramify(search));
	}
	public allTopologies(search: Search) : Observable<TopologySearchResult> {
		return this.http.get<TopologySearchResult>(this.hostUrl + "/api/topologies/all" + this.paramify(search));
	}
	public putTopology(model: ChangedTopology) : Observable<Topology> {
		return this.http.put<Topology>(this.hostUrl + "/api/topology", model);
	}
	public postTopology(model: NewTopology) : Observable<Topology> {
		return this.http.post<Topology>(this.hostUrl + "/api/topology", model);
	}
	public getTopology(id: number) : Observable<Topology> {
		return this.http.get<Topology>(this.hostUrl + "/api/topology/" + id);
	}
	public deleteTopology(id: number) : Observable<boolean> {
		return this.http.delete<boolean>(this.hostUrl + "/api/topology/" + id);
	}
	public publishTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/publish");
	}
	public unpublishTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/unpublish");
	}
	public lockTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/lock");
	}
	public unlockTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/unlock");
	}
	public shareTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/share");
	}
	public unshareTopology(id: number) : Observable<TopologyState> {
		return this.http.get<TopologyState>(this.hostUrl + "/api/topology/" + id + "/unshare");
	}
	public enlistWorker(code: string) : Observable<boolean> {
		return this.http.get<boolean>(this.hostUrl + "/api/worker/enlist/" + code);
	}
	public delistWorker(workerId: number) : Observable<boolean> {
		return this.http.delete<boolean>(this.hostUrl + "/api/worker/delist/" + workerId);
	}
	public isosTopology(id: string) : Observable<VmOptions> {
		return this.http.get<VmOptions>(this.hostUrl + "/api/topology/" + id + "/isos");
	}
	public netsTopology(id: string) : Observable<VmOptions> {
		return this.http.get<VmOptions>(this.hostUrl + "/api/topology/" + id + "/nets");
	}
	public getGames(id: number) : Observable<Array<GameState>> {
		return this.http.get<Array<GameState>>(this.hostUrl + "/api/topology/" + id + "/games");
	}
	public deleteGames(id: number) : Observable<boolean> {
		return this.http.delete<boolean>(this.hostUrl + "/api/topology/" + id + "/games");
	}
}
