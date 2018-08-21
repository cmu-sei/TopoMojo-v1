import { Component, OnInit } from '@angular/core';
import { GamespaceService } from '../../../api/gamespace.service';
import { Gamespace, Vm } from '../../../api/gen/models';
import { Observable } from 'rxjs';
import { VmService } from '../../../api/vm.service';

@Component({
  selector: 'topomojo-gamespaces',
  templateUrl: './gamespaces.component.html',
  styleUrls: ['./gamespaces.component.scss']
})
export class GamespacesComponent implements OnInit {
  current = 0;
  games: Array<Gamespace> = [];
  vms: Array<Vm> = [];

  constructor(
    private gameSvc: GamespaceService,
    private vmSvc: VmService
  ) { }

  ngOnInit() {
    this.gameSvc.getGamespaces('all').subscribe(
      (games: Array<Gamespace>) => {
        this.games = games;
      }
    );
  }

  select(game: Gamespace) {
    if (this.current !== game.id) {
      this.current = game.id;
      this.vmSvc.getVms(game.globalId).subscribe(
        (result: Vm[]) => {
          this.vms = result;
        }
      );
    } else {
      this.current = 0;
    }
  }

  players(game: Gamespace): string {
    return game.players.map(p => p.personName).join();
  }

  delete(game: Gamespace): void {
    this.gameSvc.deleteGamespace(game.id).subscribe(
        (result: boolean) => {
            this.games.splice(this.games.indexOf(game), 1);
        }
    );
  }

  trackById(i: number, item: Gamespace): number {
    return item.id;
  }
}
