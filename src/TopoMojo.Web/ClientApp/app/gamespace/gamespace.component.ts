import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GamespaceService } from './gamespace.service';

@Component({
    selector: 'gamespace',
    templateUrl: './gamespace.component.html',
    styleUrls: ['./gamespace.component.css']
})
export class GamespaceComponent {
    instances: any[];
    loading: boolean = true;

    constructor(
        private service: GamespaceService,
        private router: Router
    ) {
        this.service = service;
    }

    ngOnInit(): void {
        this.loadActive();
    };

    loadActive() {
        this.loading = true;
        this.service.activeInstances().subscribe(result => {
            this.instances = result;
        },
        (err) => {},
        () => {
            this.loading = false;
        });
    }

    destroyInstance(id: number) {
        this.loading = true;
        this.service.destroyInstance(id).subscribe(result => {
            this.loadActive();
        })
    }
}


