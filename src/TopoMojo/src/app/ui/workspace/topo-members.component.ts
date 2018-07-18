import { Component, OnInit, Input } from '@angular/core';
import { TopologyService } from '../../api/topology.service';
import { Worker, Profile } from '../../api/gen/models';
import { UserService } from '../../svc/user.service';

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
    profile: Profile;

    constructor(
        private service: TopologyService,
        private userSvc: UserService
    ) { }

    ngOnInit() {
        this.userSvc.profile$.subscribe(
            (p: Profile) => {
                this.profile = p;
            }
        )
    }

    canManage() : boolean {
        if (this.profile.isAdmin)
            return true;

        let actor = this.workers.find((w) => { return w.personGlobalId == this.profile.globalId });
        return actor && actor.canManage;
    }

    delist(workerId) {
        this.service.delistWorker(workerId)
        .subscribe(() => {
                let w = this.workers.find((worker) => worker.id == workerId);
                if (w) {
                    let index = this.workers.indexOf(w);
                    this.workers.splice(index, 1);
                }
            }, () => { });
    }

}