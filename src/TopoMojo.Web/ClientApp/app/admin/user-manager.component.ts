import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AdminService} from './admin.service';

@Component({
    //moduleId: module.id,
    selector: 'user-manager',
    templateUrl: 'user-manager.component.html'
})
export class UserManagerComponent implements OnInit {
    plist: string;
    activeTopoId: number;
    person: any;
    roster: any[];
    icon: string = 'fa fa-user';

    constructor(
        private service : AdminService,
        private router : Router,
        ) { }

    ngOnInit() {
        this.search('');
    }

    search(term) {
        this.service.roster({
            term: term,
            take: 50,
            searchFilters: []
        })
        .subscribe(data => {
            this.roster = data.results;
            this.person = null;
        }, (err) => { this.service.onError(err) });
    }

    load(person) {
        this.person = person;
    }

    grant() {
        this.service.grant(this.person)
        .subscribe(result => {
            this.person = result.json() as any;
        }, (err) => { this.service.onError(err) });

    }
    deny() {
        this.service.grant(this.person)
        .subscribe(result => {
            this.person = result.json() as any;
        }, (err) => { this.service.onError(err) });
    }
    promote() {
        this.service.promote({ tid: this.activeTopoId, pid: this.person.id})
        .subscribe(result => {
        }, (err) => { this.service.onError(err) });
    }
    demote() {
        this.service.demote({ tid: this.activeTopoId, pid: this.person.id})
        .subscribe(result => {
        }, (err) => { this.service.onError(err) });
    }

    upload() {
        this.service.upload(this.plist)
        .subscribe(result => {
            this.search('');
        }, (err) => { this.service.onError(err) });
    }
}