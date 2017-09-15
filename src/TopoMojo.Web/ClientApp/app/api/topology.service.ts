
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { TopologySearchResult,Search,Topology,ChangedTopology,Worker,Template,NewTopology,TopologyState,VmOptions } from "./api-models";

@Injectable()
export class TopologyService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public getTopologies(search : Search) : Observable<TopologySearchResult> {
		return this.http.get("/api/topologies" + UrlHelper.queryStringify(search));
	}
	public putTopology(model: ChangedTopology) : Observable<Topology> {
		return this.http.put("/api/topology", model);
	}
	public postTopology(model: NewTopology) : Observable<Topology> {
		return this.http.post("/api/topology", model);
	}
	public getTopology(id: number) : Observable<Topology> {
		return this.http.get("/api/topology/" + id);
	}
	public deleteTopology(id: number) : Observable<boolean> {
		return this.http.delete("/api/topology/" + id);
	}
	public publishTopology(id: number) : Observable<TopologyState> {
		return this.http.get("/api/topology/" + id + "/publish");
	}
	public unpublishTopology(id: number) : Observable<TopologyState> {
		return this.http.get("/api/topology/" + id + "/unpublish");
	}
	public shareTopology(id: number) : Observable<TopologyState> {
		return this.http.get("/api/topology/" + id + "/share");
	}
	public unshareTopology(id: number) : Observable<TopologyState> {
		return this.http.get("/api/topology/" + id + "/unshare");
	}
	public enlistWorker(code: string) : Observable<boolean> {
		return this.http.get("/api/worker/enlist/" + code);
	}
	public delistWorker(workerId: number) : Observable<boolean> {
		return this.http.delete("/api/worker/delist/" + workerId);
	}
	public isosTopology(id: string) : Observable<VmOptions> {
		return this.http.get("/api/topology/" + id + "/isos");
	}
	public netsTopology(id: string) : Observable<VmOptions> {
		return this.http.get("/api/topology/" + id + "/nets");
	}
}
