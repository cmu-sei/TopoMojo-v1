
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedTopologyService } from "./gen/topology.service";
import { ChangedTopology,NewTopology,Search,Template,Topology,TopologyState,TopologySummary,TopologySummarySearchResult,VmOptions,Worker } from "./gen/models";

@Injectable()
export class TopologyService extends GeneratedTopologyService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
