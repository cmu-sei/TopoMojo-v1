import { Injectable } from '@angular/core';
import { Observable ,  Subject } from "rxjs";

@Injectable()
export class LayoutService {

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
