
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedTopologyService } from "./gen/topology.service";
import { TopologySummarySearchResult,Search,TopologySummary,ChangedTopology,Topology,Worker,Template,NewTopology,TopologyState,VmOptions } from "./gen/models";

@Injectable()
export class TopologyService extends GeneratedTopologyService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
