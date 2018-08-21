import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { GamespaceService } from '../../../api/gamespace.service';
import { Gamespace } from '../../../api/gen/models';

@Component({
  selector: 'topomojo-gamespace-lobby',
  templateUrl: './gamespace-lobby.component.html',
  styleUrls: ['./gamespace-lobby.component.scss']
})
export class GamespaceLobbyComponent implements OnInit {
  games = Array<Gamespace>();
  @Output() activated = new EventEmitter<boolean>();

  constructor(
    private gamespaceSvc: GamespaceService
  ) { }

  ngOnInit() {
    this.reload();
  }

  reload() {
    this.gamespaceSvc.getGamespaces('').subscribe(
      (games: Array<Gamespace>) => {
        this.games = games;
        this.activated.emit(!!games.length);
      }
    );
  }

  delete(id: number) {
    this.gamespaceSvc.deleteGamespace(id)
    .subscribe(result => {
        this.reload();
    });
  }

  trackById(i: number, item: Gamespace): number {
    return item.id;
  }
}
