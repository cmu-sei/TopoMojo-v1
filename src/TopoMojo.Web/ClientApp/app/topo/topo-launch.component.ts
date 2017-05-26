//@ts-name

import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopoService } from './topo.service';
import { Converter } from 'showdown/dist/showdown';
import { SHOWDOWN_OPTS } from '../shared/constants/ui-params';
import { VmService } from '../vm/vm.service';

@Component({
    // moduleId: module.id,
    selector: 'topo-launch',
    templateUrl: 'topo-launch.component.html'
})
export class TopoLaunchComponent implements OnInit {
    summary: any;
    errorMessage: string;
    renderedDocument: string;
    status: string = "Verifying topology...";
    private converter : Converter;

    constructor(
        private router: Router,
        private route: ActivatedRoute,
        private service : TopoService,
        private vmService : VmService
    ) {
        this.converter = new Converter(SHOWDOWN_OPTS);
    }

    ngOnInit() {
        //fire launch/id api method
        this.route.params
        .switchMap((params: Params) => this.service.launchInstance(params['id']))
            .subscribe(result => {
                console.log(result);
                this.summary = result;
                this.status = 'Loading document...';
                this.service.loadUrl(this.summary.document).subscribe(text => {
                    //todo: update console urls
                    let newHtml = this.converter.makeHtml(text);
                    //newHtml = newHtml.replace(/href="console"/g, 'href="console" target="console"');

                    console.log(newHtml);
                    this.renderedDocument = newHtml;
                    this.status = '';
                })
            }, (err) => {
                this.errorMessage = err.json().message;
                //this.service.onError(err);
            });

    }

    destroy() {
        this.service.destroyInstance(this.summary.id)
        .subscribe(result => {
            this.summary = null;
            this.router.navigate(['/']);
        });
    }

    launchConsole(id) {
        console.log('launch console ' + id);
        this.vmService.launchPage("/vm/display/" + id);
    }
    // parseConsoleAnchors(html : string) {
    //     let result = '';
    //     let pattern = /<a href='console'>(.+)<\/a>/;
    //     html.replace(pattern, function(m, t) {
    //         //lookup t in this.summary.vms
    //         let id = '';
    //         for (let i = 0; i < this.summary.vms; i++) {
    //             if (this.summary.vms[i].name == t) {
    //                 id = this.summary.vms[i].id;
    //                 break;
    //             }
    //         }

    //         return result;
    //     });
    // }
}