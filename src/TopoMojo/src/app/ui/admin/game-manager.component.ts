import { Component, OnInit } from '@angular/core';
import { GamespaceService } from '../../api/gamespace.service';
import { Gamespace, GameState, Player, VmState, Vm } from '../../api/gen/models';
import { VmService } from '../../api/vm.service';

@Component({
    selector: 'app-game-manager',
    templateUrl: 'game-manager.component.html',
    styleUrls: [ 'game-manager.component.css' ]
})
export class GameManagerComponent implements OnInit {

    constructor(
        private gameSvc: GamespaceService,
        private vmSvc: VmService
    ) { }

    games: Array<Gamespace>;
    selected: Gamespace;
    players: Array<Player>;
    vms: Array<VmState>;

    ngOnInit() {
        this.gameSvc.getGamespaces('all').subscribe(
            (result: Array<Gamespace>) => {
                this.games = result;
            }
        );
    }

    playerlist(game: Gamespace): string {
        return game.players.map(p => p.personName).join();
    }

    vmlist(game: Gamespace): void {
        this.selected = game;
        this.vmSvc.getVms(game.globalId).subscribe(
            (result: Array<Vm>) => {
                this.vms = result;
            }
        );
    }
    destroy(game: Gamespace): void {
        this.gameSvc.deleteGamespace(game.id).subscribe(
            (result: boolean) => {
                this.games.splice(this.games.indexOf(game), 1);
            }
        );
    }

    hasMore(): boolean {
        return false;
    }

    termChanged(term: string): void {

    }
}
