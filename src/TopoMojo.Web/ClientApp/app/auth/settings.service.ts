import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';

@Injectable()
export class SettingsService {

    constructor(
    ) {
        let all = window['clientSettings'];
        this.all = all;
        this.oidc = all['oidc'];
        this.urls = all['urls'];
        this.branding = all['branding'];
        this.lang = all['lang'] || 'en';
        this.hostUrl = window.location.origin;
    }

    all : any;
    oidc : any;
    urls : any;
    branding : any;
    lang : string;

    hostUrl: string;
    layout: Layout = new Layout();
    private _layout: Subject<Layout> = new Subject<Layout>();
    layout$: Observable<Layout> = this._layout.asObservable();

    changeLayout(layout : Layout) : void {
        this.layout = layout;
        this._layout.next(layout);
    }
}

export class Layout {
    embedded: boolean;
}