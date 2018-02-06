import { Injectable, InjectionToken } from '@angular/core';
import { Observable, Subject } from 'rxjs/Rx';
import { HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
// import { SignalRConfiguration } from 'ng2-signalr';

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
        this.maintMsg = all['maintenanceMessage'];
        this.login = all['login'];
        this.hostUrl = window.location.origin;
    }

    all : any;
    oidc : any;
    urls : any;
    branding : any;
    lang : string;
    maintMsg: string;
    login : any;

    hostUrl: string;
    layout: Layout = new Layout();
    private _layout: Subject<Layout> = new Subject<Layout>();
    layout$: Observable<Layout> = this._layout.asObservable();


    changeLayout(layout : Layout) : void {
        this.layout = layout;
        this._layout.next(layout);
    }

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

export class Layout {
    embedded: boolean;
}

export const ORIGIN_URL = new InjectionToken<string>('ORIGIN_URL');
export function getOriginUrl() {
    return (window && window.location) ? window.location.origin : "";
}

export const SHOWDOWN_OPTS = new InjectionToken<string>('SHOWDOWN_OPTS');
export function getShowdownOpts() {
    return {
        strikethrough: true,
        tables: true,
        parseImgDimensions: true,
        smoothLivePreview: true,
        tasklists: true
    };
}

export function createTranslateLoader(http: HttpClient, baseHref) {
    // Temporary Azure hack
    if (baseHref === 'undefined' && typeof window !== 'undefined') {
        baseHref = window.location.origin;
    }

    // i18n files are in `wwwroot/lang/`
    return new TranslateHttpLoader(http, `/lang/`, '.json');
}

// export const SIGNALR_CONFIG = new InjectionToken<string>('SIGNALR_CONFIG');
// export function createSignalRConfig(): SignalRConfiguration {
//     const c = new SignalRConfiguration();
//     c.hubName = 'TopologyHub';
//     c.url = getOriginUrl();
//     c.logging = false;
//     return c;
// }