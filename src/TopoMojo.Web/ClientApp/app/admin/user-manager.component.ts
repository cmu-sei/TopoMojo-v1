import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AdminService} from './admin.service';

@Component({
    //moduleId: module.id,
    selector: 'user-manager',
    templateUrl: 'user-manager.component.html',
    styleUrls: ['user-manager.component.css']
})
export class UserManagerComponent implements OnInit {
    plist: string;
    activeTopoId: number;
    userUploadVisible: boolean;
    //person: any;
    roster: any[] = [];
    icon: string = 'fa fa-user';
    hasMore: number;
    model: any = {
        term: '',
        skip: 0,
        take: 50,
        filters: []
    }

    constructor(
        private service : AdminService,
        private router : Router,
        ) { }

    ngOnInit() {
        this.search();
    }

    more() {
        this.model.skip += this.model.take;
        this.search();
    }

    termChanged(term) {
        this.hasMore = 0;
        this.model.term = term;
        this.model.skip = 0;
        this.roster = [];
        this.search();
    }
    search() {
        this.service.roster(this.model)
        .subscribe(data => {
            this.roster = this.roster.concat(data.results);
            this.hasMore = data.total - (data.skip+data.take);
            //this.person = null;
        }, (err) => { this.service.onError(err) });
    }

    load(person) {
        //this.person = person;
    }

    grant(person) {
        this.service.grant(person)
        .subscribe(result => {
            person.isAdmin = true;
            //this.person = result as any;
        }, (err) => { this.service.onError(err) });

    }
    deny(person) {
        this.service.deny(person)
        .subscribe(result => {
            person.isAdmin = false;
            //this.person = result as any;
        }, (err) => { this.service.onError(err) });
    }

    toggleUpload() {
        this.userUploadVisible = !this.userUploadVisible;

    }
    upload() {
        this.service.upload(this.plist)
        .subscribe(result => {
            this.search();
        }, (err) => { this.service.onError(err) });
    }
}