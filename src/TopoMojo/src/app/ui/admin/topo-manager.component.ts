import { Component, OnInit } from '@angular/core';
import { TopologyService } from '../../api/topology.service';
import { Topology, TopologyState, Worker, VmState, Vm, Search, TopologyStateActionTypeEnum } from '../../api/gen/models';
import { VmService } from '../../api/vm.service';
import { AdminService } from '../../api/admin.service';
import { LayoutService } from '../../svc/layout.service';

@Component({
    selector: 'app-topo-manager',
    templateUrl: 'topo-manager.component.html',
    styleUrls: [ 'topo-manager.component.css' ]
})
export class TopoManagerComponent implements OnInit {

    constructor(
        private topoSvc: TopologyService,
        private vmSvc: VmService,
        private layoutSvc: LayoutService,
        private adminSvc: AdminService
    ) { }

    topos: Array<Topology> = new Array<Topology>();
    selected: Array<number> = new Array<number>();
    players: Array<Worker>;
    vms: Array<VmState>;
    seachParams: Search = { skip: 0, take: 0, filters: [ 'all' ] };
    hasMore = 0;
    exports: Array<Topology> = new Array<Topology>();
    exportResult = '';

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

    search(): void {
        this.topoSvc.getTopologies(this.seachParams).subscribe(
            (result) => {
                this.topos.push(...result.results);
                this.hasMore = result.total - (result.search.skip + result.search.take);
            }
        );
    }

    more() {
        this.seachParams.skip += this.seachParams.take || 50;
        this.search();
    }

    workerlist(topo: Topology): string {
        return topo.workers.map(p => p.personName).join();
    }

    toggleSelected(topo: Topology): void {
        const found = this.selected.filter(
            (v) => {
                return v === topo.id;
            }
        );
        if (found && found.length > 0) {
            this.selected.splice(this.selected.indexOf(found[0]), 1);
        } else {
            this.selected.push(topo.id);
            this.vmSvc.getVms(topo.globalId).subscribe(
                (result: Array<Vm>) => {
                    this.vms = result;
                }
            );
        }
    }

    toggleLocked(topo: Topology): void {
        const q = (topo.isLocked )
            ? this.topoSvc.postTopologyAction(topo.id, {
                id: topo.id,
                type: TopologyStateActionTypeEnum.unlock
            })
            : this.topoSvc.postTopologyAction(topo.id, {
                id: topo.id,
                type: TopologyStateActionTypeEnum.lock
            });

        q.subscribe(
            (result: TopologyState) => {
                topo.isLocked = result.isLocked;
            }
        );
    }

    isSelected(id: number): boolean {
        return this.selected.indexOf(id) >= 0;
    }

    destroy(topo: Topology): void {
        this.topoSvc.deleteTopology(topo.id).subscribe(
            () => {
                this.topos.splice(this.topos.indexOf(topo), 1);
            }
        );
    }

    showDoc(topo: Topology): void {
        this.layoutSvc.showTab(topo.document); // Url || `/docs/${topo.globalId}.md`);
    }

    hasExports(): boolean {
        return this.exports.length > 0 || !!this.exportResult;
    }

    addExport(t: Topology): void {
        const found = this.exports.find((v) => v.id === t.id);
        if (!found) {
            this.exports.push(t);
        }
    }

    removeExport(t: Topology): void {
        // let found = this.exports.find((v) => v.id == t.id);
        // if (found)
            this.exports.splice(this.exports.indexOf(t), 1);
    }

    export(): void {
        this.adminSvc.postAdminExport(this.exports.map((v) => v.id)).subscribe(
            (result: Array<string>) => {
                this.exports = new Array<Topology>();
                this.exportResult = result.join('\n');
            }
        );
    }
}
