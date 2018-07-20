import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { GamespaceService } from '../../api/gamespace.service';
import { Gamespace } from '../../api/gen/models';

@Component({
    selector: 'app-gamespace',
    templateUrl: './gamespace.component.html',
    styleUrls: ['./gamespace.component.css']
})
export class GamespaceComponent implements OnInit {
    games: Gamespace[];
    loading = true;

    constructor(
        private service: GamespaceService,
        private router: Router
    ) {
    }

    ngOnInit(): void {
        this.loadActive();
    }

    loadActive() {
        this.loading = true;
        this.service.getGamespaces('')
        .subscribe(result => {
            this.games = result;
        },
        (err) => {},
        () => {
            this.loading = false;
        });
    }

    delete(id: number) {
        this.loading = true;
        this.service.deleteGamespace(id)
        .subscribe(result => {
            this.loadActive();
        });
    }
}


