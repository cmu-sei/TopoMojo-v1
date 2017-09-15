import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GamespaceService } from '../api/gamespace.service';
import { Gamespace } from "../api/api-models";

@Component({
    selector: 'gamespace',
    templateUrl: './gamespace.component.html',
    styleUrls: ['./gamespace.component.css']
})
export class GamespaceComponent {
    instances: Gamespace[];
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
        this.service.getGamespaces()
        .subscribe(result => {
            this.instances = result;
        },
        (err) => {},
        () => {
            this.loading = false;
        });
    }

    destroyInstance(id: number) {
        this.loading = true;
        this.service.deleteGamespace(id)
        .subscribe(result => {
            this.loadActive();
        })
    }
}


