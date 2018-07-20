
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { ChangedTopology, GameState, NewTopology, Search, Template, Topology, TopologySearchResult, TopologyState, TopologyStateAction, TopologyStateActionTypeEnum, TopologySummary, TopologySummarySearchResult, VmOptions, VmState, Worker } from './models';

@Injectable()
export class GeneratedTopologyService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getTopologySummaries(search: Search): Observable<TopologySummarySearchResult> {
        return this.http.get<TopologySummarySearchResult>(this.api.url + '/api/topology/summaries' + this.paramify(search));
    }
    public getTopologies(search: Search): Observable<TopologySearchResult> {
        return this.http.get<TopologySearchResult>(this.api.url + '/api/topologies' + this.paramify(search));
    }
    public putTopology(model: ChangedTopology): Observable<Topology> {
        return this.http.put<Topology>(this.api.url + '/api/topology', model);
    }
    public postTopology(model: NewTopology): Observable<Topology> {
        return this.http.post<Topology>(this.api.url + '/api/topology', model);
    }
    public getTopology(id: number): Observable<Topology> {
        return this.http.get<Topology>(this.api.url + '/api/topology/' + id);
    }
    public deleteTopology(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/topology/' + id);
    }
    public getTopologyGames(id: number): Observable<Array<GameState>> {
        return this.http.get<Array<GameState>>(this.api.url + '/api/topology/' + id + '/games');
    }
    public deleteTopologyGames(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/topology/' + id + '/games');
    }
    public postTopologyAction(id: number, action: TopologyStateAction): Observable<TopologyState> {
        return this.http.post<TopologyState>(this.api.url + '/api/topology/' + id + '/action', action);
    }
    public postWorkerCode(code: string): Observable<boolean> {
        return this.http.post<boolean>(this.api.url + '/api/worker/enlist/' + code, {});
    }
    public deleteWorker(id: number): Observable<boolean> {
        return this.http.delete<boolean>(this.api.url + '/api/worker/' + id);
    }
    public getTopologyIsos(id: string): Observable<VmOptions> {
        return this.http.get<VmOptions>(this.api.url + '/api/topology/' + id + '/isos');
    }
    public getTopologyNets(id: string): Observable<VmOptions> {
        return this.http.get<VmOptions>(this.api.url + '/api/topology/' + id + '/nets');
    }

}
