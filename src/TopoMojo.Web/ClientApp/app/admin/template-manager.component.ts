import { Component, OnInit, ViewChild, NgZone } from '@angular/core';
import { TopoService } from '../topo/topo.service';

@Component({
    //moduleId: module.id,
    selector: 'template-manager',
    templateUrl: 'template-manager.component.html',
    styleUrls: [ 'template-manager.component.css' ]
})
export class TemplateManagerComponent implements OnInit {
    template: any;
    templates: any[];
    icon: string = 'fa fa-clipboard';
    private term: string = '';
    errorMessage: string;

    constructor(
        private service : TopoService,
        private _ngZone : NgZone
        ) { }

    @ViewChild('focusTag') focusTagEl;

    ngOnInit() {
        this.search('');
    }

    search(term) {
        this.term = term;
        this.service.listTemplates({
            term: term,
            take: 50,
            searchFilters: []
        })
        .subscribe(data => {
            this.templates = data.results as any[];
            this.template = null;
        }, (err) => { this.service.onError(err) })
    }

    load(template) {
        this.template = template;

        this._ngZone.runOutsideAngular(() => {
            setTimeout(() => this.focusEditor(), 5);
        });
        // this.service.loadTemplate(id)
        // .subscribe(result => {
        //     this.template = result as any;
        // })
    }

    focusEditor() {
        this.focusTagEl.nativeElement.focus();
    }

    create() {
        this.template = {
            name: ""
        };
    }

    save() {
        this.service.saveTemplate(this.template)
        .subscribe(data => {
            this.template = data as any;
        }, (err) => { this.service.onError(err) });
    }

    delete() {
        this.service.deleteTemplate(this.template.id)
        .subscribe(data => {
            this.template = null;
            this.search(this.term);
        }, (err) => {
            this.errorMessage = JSON.parse(err.text()).message;
            this.service.onError(err);
        })
    }

    clearError() {
        this.errorMessage = null;
    }
}