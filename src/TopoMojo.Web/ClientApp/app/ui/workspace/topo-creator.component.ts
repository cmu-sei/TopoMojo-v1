import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopologyService } from '../../api/topology.service';
import { NewTopology } from '../../api/gen/models';

@Component({
    selector: 'topo-creator',
    templateUrl: './topo-creator.component.html',
    styleUrls: ['./topo-creator.component.css']
})
export class TopoCreatorComponent {
    topo: NewTopology = {
        name : "",
        description : ""
    }
    errorMessage: string;

    constructor(
        private service: TopologyService,
        private router: Router
    ) {
    }

    errors: Array<Error> = new Array<Error>();

    ngOnInit(): void {
    };

    save() {
        this.service.postTopology(this.topo)
        .subscribe(result => {
            this.router.navigate(['topo', result.id]);
        }, (err) => { this.errors.push(err.error) });
    }
}


