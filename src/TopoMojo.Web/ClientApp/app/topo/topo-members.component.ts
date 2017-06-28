import { Component, OnInit, Input } from '@angular/core';
import { TopoService } from './topo.service';

@Component({
    //moduleId: module.id,
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
    permissions : any[];
    addUserEmail: string = "add-user-email(s) ";
    @Input() topoId: number;

    constructor(
        private service: TopoService
    ) { }

    ngOnInit() {
        this.refresh();
    }

    refresh() {
        this.service.listMembers(this.topoId)
        .subscribe(result => {
            this.permissions = result as any[];
        }, (err) => { this.service.onError(err); });
    }

    // addUser() {
    //     this.service.addMembers(this.topoId, this.addUserEmail)
    //     .subscribe(data => {
    //         this.addUserEmail = "add-user-email(s) ";
    //         this.refresh();
    //     }, (err) => { this.service.onError(err)});
    // }

    delist(personId) {
        this.service.delist(this.topoId, personId)
        .subscribe(data => {
            this.refresh();
        }, (err) => { this.service.onError(err)});
    }

}