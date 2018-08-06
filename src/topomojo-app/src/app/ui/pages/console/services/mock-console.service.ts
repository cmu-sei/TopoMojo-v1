import { Injectable } from '@angular/core';
import { ConsoleService } from './console.service';

@Injectable()
export class MockConsoleService implements ConsoleService {
  counter = 0;
  stateChanged: Function = () => { };

  constructor() { }

  connect(url: string, stateCallback: Function, options: any) {
    if (stateCallback === Function) { this.stateChanged = stateCallback; }
    if (this.counter % 3 === 2) {
      stateCallback('connected');
      setTimeout(() => {
        stateCallback('disconnected');
      }, 10000);
    }

    if (this.counter % 3 === 1) { stateCallback('failed'); }
    if (this.counter % 3 === 0) { stateCallback('forbidden'); }
    this.counter += 1;
  }
  disconnect() {
    this.stateChanged('disconnected');
  }
  sendCAD() {}
  refresh() {}
  toggleScale() {}
  fullscreen() {}
  showKeyboard() {}
  showExtKeypad() {}
  showTrackpad() {}
  dispose() {}
}
