import { Injectable } from '@angular/core';
import { ConsoleService } from './console.service';

@Injectable()
export class MockConsoleService implements ConsoleService {
  counter = 0;
  stateChanged: Function = () => { };

  constructor() { }

  connect(url: string, stateCallback: Function, options: any): void {
    if (typeof stateCallback === 'function') { this.stateChanged = stateCallback; }

    if (this.counter % 3 === 2) {
      this.stateChanged('connected');
      setTimeout(() => {
        this.stateChanged('disconnected');
      }, 10000);
    }

    if (this.counter % 3 === 1) { this.stateChanged('failed'); }
    if (this.counter % 3 === 0) { this.stateChanged('forbidden'); }
    this.counter += 1;
  }

  sendCAD(): void { }
  refresh(): void {
    console.log('refresh console screen');
  }
  dispose(): void { }
}
