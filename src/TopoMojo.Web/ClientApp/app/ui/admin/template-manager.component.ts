import { Component, OnInit, ViewChild, NgZone } from '@angular/core';
import { TemplateService } from '../../api/template.service';
import { Search, TemplateSummary, TemplateDetail } from '../../api/gen/models';

@Component({
    //moduleId: module.id,
    selector: 'template-manager',
    templateUrl: 'template-manager.component.html',
    styleUrls: [ 'template-manager.component.css' ]
})
export class TemplateManagerComponent implements OnInit {
    template: TemplateDetail;
    //templateLinker: any;
    templates: TemplateSummary[] = [];
    selected: number;
    icon: string = 'fa fa-clipboard';
    //private term: string = '';
    errorMessage: string;
    hasMore: number;
    model: Search = { filters: [ 'parents' ]};
    loading: boolean;

    constructor(
        private service : TemplateService,
        private _ngZone : NgZone
        ) { }

    @ViewChild('focusTag') focusTagEl;

    ngOnInit() {
        this.fireSearch();
    }

    more() {
        this.model.skip += this.model.take;
        this.fireSearch();
    }

    termChanged(term) {
        this.model.term = term;
        this.search();
    }

    search() {
        this.model.skip = 0;
        this.hasMore = 0;
        this.templates = [];
        this.fireSearch();
    }

    fireSearch() {
        this.loading = true;
        this.service.getTemplates(this.model)
        .subscribe(data => {
            this.templates = this.templates.concat(data.results);
            this.hasMore = data.total - (data.search.skip+data.search.take);
            this.template = null;
            }, (err) => { },
            () => {
                this.loading = false;
            })
    }

    select(selected : number) {
        if (this.selected === selected) {
            this.template = null;
            this.selected = 0;
        } else {
            //load template detail
            this.selected = selected;
            this.service.detailedTemplate(selected)
            .subscribe(
                (result: TemplateDetail) => {
                    this.template = result;
                    this._ngZone.runOutsideAngular(() => {
                        setTimeout(() => this.focusEditor(), 5);
                    });
                }
            )
        }
    }

    focusEditor() {
        this.focusTagEl.nativeElement.focus();
    }

    create() {
        this.service.createTemplate({ name: 'new-template'})
        .subscribe(data => {
            this.selected = data.id;
            this.templates.unshift({ id: data.id, name: data.name });
            //this.templates.push(data);
            this.template = data;
            this._ngZone.runOutsideAngular(() => {
                setTimeout(() => this.focusEditor(), 5);
            });
        }, (err) => { });

    }

    save() {

        try {
            let s = JSON.parse(this.template.detail);
            this.service.configureTemplate(this.template).subscribe(
                (data) => {
                    this.templates.find((v) => v.id == data.id).name = data.name;
                    this.select(this.template.id);
                },
                (err) => { }
            );
        } catch(Error) {
            this.errorMessage = Error.message;
        }


    }

    delete() {
        this.service.deleteTemplate(this.template.id)
        .subscribe(data => {
            this.template = null;
            this.termChanged(this.model.term);
        }, (err) => {
            this.errorMessage = err.error.message;
        })
    }

    clearError() {
        this.errorMessage = null;
    }
}
