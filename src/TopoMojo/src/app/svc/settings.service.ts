import { Injectable, InjectionToken } from '@angular/core';
import { Observable ,  Subject, of } from "rxjs";
import { HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import {throwError as observableThrowError } from 'rxjs';
import { UserManagerSettings } from 'oidc-client';
import { catchError } from 'rxjs/operators';
import { ShowdownOptions } from 'showdown';

@Injectable()
export class SettingsService {

    constructor(
        private http: HttpClient
    ) {
        this.hostUrl = window.location.origin;
    }

    settings : Settings = {
        lang: "en",
        maintMessage: "",
        branding: {
            applicationName: "TopoMojo"
        },
        showdown: {
            strikethrough: true,
            tables: true,
            parseImgDimensions: true,
            smoothLivePreview: true,
            tasklists: true
        }
    };

    url: string = "assets/config/settings.json";
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

    public load() : Promise<boolean> {
        return new Promise((resolve, reject) => {
            this.http.get<Settings>(this.url)
            .pipe(
                catchError((error: any): any => {
                    console.log("invalid settings.json");
                    return of(new Object());
                })
            )
            .subscribe((data : Settings) => {
                this.settings = {...this.settings, ...data};
                this.http.get(this.url.replace(/json$/, "env.json"))
                    .pipe(
                        catchError((error: any): any => {
                            return of(new Object());
                        })
                    )
                    .subscribe((data : Settings) => {
                        this.settings = {...this.settings, ...data};
                        resolve(true);
                    });
                });
            });
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
    return new TranslateHttpLoader(http, `/assets/lang/`, '.json');
}

export interface Settings {
    oidc? : UserManagerSettings,
    urls? : AppUrlSettings,
    branding? : BrandingSettings,
    lang?: string,
    maintMessage?: string,
    showdown?: ShowdownOptions
}

export interface AppUrlSettings {
    apiUrl?: string
}

export interface BrandingSettings {
    applicationName?: string,
    logoUrl?: string
}