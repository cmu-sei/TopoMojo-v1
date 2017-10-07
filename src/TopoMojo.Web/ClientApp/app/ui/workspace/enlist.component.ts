import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { TopologyService } from '../../api/topology.service';

@Component({
    templateUrl: 'enlist.component.html'
})
export class TopoEnlistComponent implements OnInit {

    constructor(
        private service: TopologyService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    complete: boolean;
    errors: any[] = [];

    ngOnInit(): void {
        let code = this.route.snapshot.paramMap.get('code');
        this.service.enlistWorker(code)
            .finally(() => { this.complete = true; })
            .subscribe(result => {
                this.router.navigate(['/topo']);
            }, (err) => { this.onError(err); });

    }

    onError(err) {
        this.errors.push(err.error);
        //console.debug(err.error.message);
    }

}