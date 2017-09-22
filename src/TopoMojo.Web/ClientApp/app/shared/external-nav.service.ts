import { Injectable } from '@angular/core';

@Injectable()
export class ExternalNavService {

    constructor() { }

    tabRefs: any = {};
    public showTab(url : string) : void {
        if ( typeof this.tabRefs[url] == 'undefined' || this.tabRefs[url].closed )
        {
            this.tabRefs[url] = window.open(url);
        } else {
            this.tabRefs[url].focus()
        }
    }
}