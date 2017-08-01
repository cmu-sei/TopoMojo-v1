import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopoService } from './topo.service';
import 'rxjs/add/operator/switchMap';

@Component({
    //moduleId: module.id,
    //selector: 'enlist',
    template: `
        <p>Validating enlistment code...</p>
    `
})
export class TopoEnlistComponent implements OnInit {

    constructor(
        private service: TopoService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.route.params
            .switchMap((params: Params) => this.service.enlist(params['code']))
            .subscribe(result => {
                this.router.navigate(['/topo']);
            }, (err) => { this.service.onError(err); });

    }



}