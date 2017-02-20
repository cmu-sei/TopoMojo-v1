import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopoService } from './topo.service';

@Component({
    selector: 'topo-creator',
    templateUrl: './topo-creator.component.html',
    styleUrls: ['./topo-creator.component.css']
})
export class TopoCreatorComponent {
    name: string;
    description: string;
    errorMessage: string;

    constructor(private service: TopoService, private router: Router) {
    }

    ngOnInit(): void {
    };

    save() {
        this.service.createTopo({
            name: this.name,
            description: this.description
        })
        .subscribe(result => {
            this.router.navigate(['topo', result.id]);
        }, (err) => { this.service.onError(err); });
    }
}


