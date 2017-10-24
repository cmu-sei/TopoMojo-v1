import { Component, OnInit } from '@angular/core';
import { GamespaceService } from '../../api/gamespace.service';
import { Gamespace, GameState, Player, VmState, VirtualVm } from '../../api/gen/models';
import { VmService } from '../../api/vm.service';

@Component({
    selector: 'game-manager',
    templateUrl: 'game-manager.component.html'
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
        this.gameSvc.allGamespaces().subscribe(
            (result: Array<Gamespace>) => {
                this.games = result;
            }
        )
    }

    onSelect(game: Gamespace): void {
        this.selected = game;
        this.gameSvc.playersGamespace(game.id).subscribe(
            (result: Array<Player>) => {
                this.players = result;
            }
        )
    }

    playerlist() : string {
        return this.players && this.players.map(p => {return p.personName}).join();
    }

    vmlist(game: Gamespace) : void {
        this.selected = game;
        this.vmSvc.findVms(game.globalId).subscribe(
            (result: Array<VirtualVm>) => {
                this.vms = result;
            }
        )
    }
    destroy(game: Gamespace): void {
        this.gameSvc.deleteGamespace(game.id).subscribe(
            (result: boolean) => {
                this.games.splice(this.games.indexOf(game), 1);
            }
        )
    }
}