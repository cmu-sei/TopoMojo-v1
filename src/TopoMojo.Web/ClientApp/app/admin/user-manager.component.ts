import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ProfileService} from '../api/profile.service';
import { Search, ProfileSearchResult, Profile } from "../api/api-models";

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
    roster: Profile[] = [];
    icon: string = 'fa fa-user';
    hasMore: number;
    model: Search = {
        term: '',
        skip: 0,
        take: 50,
        filters: []
    }

    constructor(
        private service : ProfileService,
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
        this.service.getProfiles(this.model)
        .subscribe(data => {
            this.roster = this.roster.concat(data.results);
            this.hasMore = data.total - (data.search.skip+data.search.take);
            //this.person = null;
        }, (err) => { });
    }

    load(person) {
        //this.person = person;
    }

    // grant(person) {
    //     this.service.grant(person)
    //     .subscribe(result => {
    //         person.isAdmin = true;
    //         //this.person = result as any;
    //     }, (err) => { this.service.onError(err) });

    // }
    // deny(person) {
    //     this.service.deny(person)
    //     .subscribe(result => {
    //         person.isAdmin = false;
    //         //this.person = result as any;
    //     }, (err) => { this.service.onError(err) });
    // }

    toggleUpload() {
        this.userUploadVisible = !this.userUploadVisible;

    }
    // upload() {
    //     this.service.upload(this.plist)
    //     .subscribe(result => {
    //         this.search();
    //     }, (err) => { this.service.onError(err) });
    // }
}