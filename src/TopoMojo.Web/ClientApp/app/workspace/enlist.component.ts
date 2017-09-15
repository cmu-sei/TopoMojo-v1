import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopologyService } from '../api/topology.service';
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
        private service: TopologyService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.route.params
            .switchMap((params: Params) => this.service.enlistWorker(params['code']))
            .subscribe(result => {
                this.router.navigate(['/topo']);
            }, (err) => { });

    }



}