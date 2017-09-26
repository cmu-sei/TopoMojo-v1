import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRouteSnapshot, Params } from '@angular/router';
import { TopologyService } from '../../api/topology.service';

@Component({
    template: `
        <p>Validating enlistment code...</p>
    `
})
export class TopoEnlistComponent implements OnInit {

    constructor(
        private service: TopologyService,
        private route: ActivatedRouteSnapshot,
        private router: Router
    ) { }

    ngOnInit(): void {
        let code = this.route.paramMap.get('code');
        this.service.enlistWorker(code)
            .subscribe(result => {
                this.router.navigate(['/topo']);
            }, (err) => { });

    }



}