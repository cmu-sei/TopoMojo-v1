import { Component, OnInit, Input } from '@angular/core';
import { TopologyService } from '../../api/topology.service';
import { Worker } from '../../api/gen/models';

@Component({
    selector: 'topo-members',
    templateUrl: 'topo-members.component.html',
    styles: [`
        ul {
            display: inline-block;
        }
        li {
            padding: 2px 8px;
        }
    `]
})
export class TopoMembersComponent implements OnInit {
    @Input() workers : Worker[];

    constructor(
        private service: TopologyService
    ) { }

    ngOnInit() {
        //this.refresh();
    }

    // refresh() {
    //     this.service.listMembers(this.topoId)
    //     .subscribe(result => {
    //         this.permissions = result as any[];
    //     }, (err) => { this.service.onError(err); });
    // }

    // addUser() {
    //     this.service.addMembers(this.topoId, this.addUserEmail)
    //     .subscribe(data => {
    //         this.addUserEmail = "add-user-email(s) ";
    //         this.refresh();
    //     }, (err) => { this.service.onError(err)});
    // }

    delist(workerId) {
        this.service.delistWorker(workerId)
        .subscribe(data => {
            let w = this.workers.find((worker) => worker.id == workerId);
            if (w) {
                let index = this.workers.indexOf(w);
                this.workers.splice(index, 1);
            }
        }, (err) => { });
    }

}