import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopologyService } from '../../api/topology.service';
import { NewTopology, Topology } from '../../api/gen/models';

@Component({
    selector: 'app-topo-creator',
    templateUrl: './topo-creator.component.html',
    styleUrls: ['./topo-creator.component.css']
})
export class TopoCreatorComponent implements OnInit {
    topo: NewTopology = {
        name : '',
        description : ''
    };
    errorMessage: string;

    constructor(
        private service: TopologyService,
        private router: Router
    ) {
    }

    errors: Array<Error> = new Array<Error>();

    ngOnInit(): void { }

    save() {
        this.service.postTopology(this.topo)
        .subscribe(
            (topo: Topology) => {
                this.router.navigate(['topo', topo.id]);
            },
            (err) => { this.errors.push(err.error); }
        );
    }
}


