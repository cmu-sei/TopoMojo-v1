import { Component, OnInit } from '@angular/core';
import { TopologyService } from '../../api/topology.service';
import { Topology, TopologySummary, TopologyState, TemplateSummary, Worker, VmState, VirtualVm, Search } from '../../api/gen/models';
import { VmService } from '../../api/vm.service';
import { SettingsService } from '../../svc/settings.service';

@Component({
    selector: 'topo-manager',
    templateUrl: 'topo-manager.component.html'
})
export class TopoManagerComponent implements OnInit {

    constructor(
        private topoSvc: TopologyService,
        private vmSvc: VmService,
        private settingSvc: SettingsService
    ) { }

    topos: Array<Topology> = new Array<Topology>();
    selected: Array<number> = new Array<number>();
    players: Array<Worker>;
    vms: Array<VmState>;
    seachParams: Search = { skip: 0, take: 0 };
    hasMore: number = 0;
    hostField: string;

    ngOnInit() {
        this.search();
    }

    termChanged(term) {
        this.hasMore = 0;
        this.seachParams.term = term;
        this.seachParams.skip = 0;
        this.topos = [];
        this.search();
    }

    search() : void {
        this.topoSvc.allTopologies(this.seachParams).subscribe(
            (result) => {
                this.topos.push(...result.results);
                this.hasMore = result.total - (result.search.skip+result.search.take);
            }
        )
    }

    more() {
        this.seachParams.skip += this.seachParams.take || 50;
        this.search();
    }

    workerlist(topo: Topology) : string {
        return topo.workers.map(p => p.personName).join();
    }

    toggleSelected(topo: Topology) : void {
        let found = this.selected.filter(
            (v) => {
                return v == topo.id;
            }
        );
        if (found && found.length > 0) {
            this.selected.splice(this.selected.indexOf(found[0]), 1);
        } else {
            this.selected.push(topo.id);
            this.vmSvc.findVms(topo.globalId).subscribe(
                (result: Array<VirtualVm>) => {
                    this.vms = result;
                }
            )
        }
    }

    toggleLocked(topo: Topology) : void {
        let q = (topo.isLocked )
            ? this.topoSvc.unlockTopology(topo.id)
            : this.topoSvc.lockTopology(topo.id);

        q.subscribe(
            (result : TopologyState) => {
                topo.isLocked = result.isLocked;
            }
        )
    }

    isSelected(id: number) : boolean {
        return this.selected.indexOf(id) >= 0;
    }

    destroy(topo: Topology): void {
        this.topoSvc.deleteTopology(topo.id).subscribe(
            (result: boolean) => {
                this.topos.splice(this.topos.indexOf(topo), 1);
            }
        )
    }

    showDoc(topo: Topology) : void {
        this.settingSvc.showTab(topo.document); //Url || `/docs/${topo.globalId}.md`);
    }

    reloadHost() : void {
        this.vmSvc.reloadHost(this.hostField).subscribe(
            () => {

            }
        );
    }
}