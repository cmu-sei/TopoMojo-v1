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
    templates: any[] = [];
    icon: string = 'fa fa-clipboard';
    //private term: string = '';
    errorMessage: string;
    hasMore: number;
    model: any = {
        term: '',
        skip: 0,
        take: 20,
        filters: []
    }

    constructor(
        private service : TopoService,
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
        this.service.listTemplates(this.model)
            .subscribe(data => {
                this.templates = this.templates.concat(data.results as any[]);
                this.hasMore = data.total - (data.skip + data.take);
                this.template = null;                
                // traverse templates to see if they have an owner topology
                if (this.templates != null) {
                    for (let templateLocal of this.templates) {
                        // only obtain topology if this template has a true owner
                        if (templateLocal.ownerId != 0) {
                            this.service.loadTopo(templateLocal.ownerId)
                                .subscribe(topoData => {
                                    // store the topology identifier in the template object.  if no name, using globalId for now
                                    if (topoData.name != null && topoData.name != "") {
                                        templateLocal.ownerName = topoData.name;
                                    } 
                                    else {
                                        templateLocal.ownerName = topoData.globalId;
                                    }
                                }, (err) => { this.service.onError(err) })

                            // TODO - get linked items
                            //this.service.listTopoTemplates(templateLocal.ownerId)
                            //    .subscribe(topoData => {
                            //        if (topoData != null) 
                            //        {
                            //            templateLocal.linkers = [];
                            //            for (let linkerList of topoData)
                            //            {
                            //                // only display subsequent linked topos
                            //                if (!linkerList.owned) {
                            //                    templateLocal.linkers.push(linkerList);
                            //                }
                            //            }
                            //        }
                            //    }, (err) => { this.service.onError(err) })
                        }
                    }
                }
            }, (err) => { this.service.onError(err) })           
    }
    select(template) {
        if (this.template === template) {
            this.template = null;
        } else {
            this.template = template;
            this._ngZone.runOutsideAngular(() => {
                setTimeout(() => this.focusEditor(), 5);
            });
        }
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
        this.service.saveTemplate({ name: 'new-template'})
        .subscribe(data => {
            this.templates.push(data);
            this.template = data;
            this._ngZone.runOutsideAngular(() => {
                setTimeout(() => this.focusEditor(), 5);
            });
        }, (err) => { this.service.onError(err) });

    }

    save() {
        this.service.saveTemplate(this.template)
        .subscribe(data => {
            // for (let i=0; i<this.templates.length; i++) {
            //     if (this.templates[i].id == data.id) {
            //         this.templates[i] = data;
            //         return;
            //     }
            // }
            // this.templates.push(data);
            this.template = null;
        }, (err) => { this.service.onError(err) });
    }

    delete() {
        this.service.deleteTemplate(this.template.id)
        .subscribe(data => {
            this.template = null;
            this.termChanged(this.model.term);
        }, (err) => {
            this.errorMessage = JSON.parse(err.text()).message;
            this.service.onError(err);
        })
    }

    clearError() {
        this.errorMessage = null;
    }
}