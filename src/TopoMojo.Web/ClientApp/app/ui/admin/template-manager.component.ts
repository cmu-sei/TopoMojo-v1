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
    model: Search = {};
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

    // load(template) {
    //     this.template = template;

    //     this._ngZone.runOutsideAngular(() => {
    //         setTimeout(() => this.focusEditor(), 5);
    //     });
    //     // this.service.loadTemplate(id)
    //     // .subscribe(result => {
    //     //     this.template = result as any;
    //     // })
    // }

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
        this.service.configureTemplate(this.template)
        .subscribe(data => {
            // for (let i=0; i<this.templates.length; i++) {
            //     if (this.templates[i].id == data.id) {
            //         this.templates[i] = data;
            //         return;
            //     }
            // }
            // this.templates.push(data);

            //TODO: find cleaner way to keep summary sync'd with detail
            this.templates.map(
                (v) => {
                    if (v.id == data.id) v.name = data.name;
                }
            );
            this.template = null;
        }, (err) => { });
    }

    delete() {
        this.service.deleteTemplate(this.template.id)
        .subscribe(data => {
            this.template = null;
            this.termChanged(this.model.term);
        }, (err) => {
            this.errorMessage = JSON.parse(err.text()).message;
        })
    }

    clearError() {
        this.errorMessage = null;
    }

    // selectLinker(template) {
    //     if (this.templateLinker === template) {
    //         this.templateLinker = null;
    //     } else {
    //         this.templateLinker = template;
    //     }
    // }
}