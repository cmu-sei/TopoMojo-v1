import { Injectable } from '@angular/core';
import { ConsoleService } from './console.service';
declare var WMKS: any;

@Injectable()
export class WmksConsoleService implements ConsoleService {
  private wmks;
  private options: any = {
    rescale: true,
    changeResolution: true,
    useVNCHandshake: false,
    position: WMKS.CONST.Position.CENTER
  };
  stateChanged: Function = () => { };

  constructor() { }

  connect(url: string, stateCallback: Function, options: any = {} ): void {
    if (typeof stateCallback === 'function') { this.stateChanged = stateCallback; }

    this.options = { ...this.options, ...options };

    if (this.wmks) {
      this.wmks.destroy();
    }

    this.wmks = WMKS.createWMKS(options.canvasId, this.options)

    .register(WMKS.CONST.Events.CONNECTION_STATE_CHANGE, function(event, data) {
      switch (data.state) {
        case WMKS.CONST.ConnectionState.CONNECTED:
        this.stateChanged('connected');
        break;

        case WMKS.CONST.ConnectionState.DISCONNECTED:
        this.wmks.destroy();
        this.stateChanged('disconnected');
        break;
      }
    })
    .register(WMKS.CONST.Events.REMOTE_SCREEN_SIZE_CHANGE, function (e, data) {

    })
    .register(WMKS.CONST.Events.HEARTBEAT, function (e, data) {
        // debug('wmks heartbeat: ' + data);
    })
    .register(WMKS.CONST.Events.COPY, function (e, data) {
        // debug('wmks copy: ' + data);
    })
    .register(WMKS.CONST.Events.ERROR, function (e, data) {
        // debug('wmks error: ' + data.errorType);

    })
    .register(WMKS.CONST.Events.FULL_SCREEN_CHANGE, function (e, data) {
        // debug('wmks full_screen_change: ' + data.isFullScreen);
    });

    try {
      this.wmks.connect(url);
    } catch (err) {
      this.stateChanged('failed');
    }
  }

  sendCAD(): void {
    if (this.wmks) {
      this.wmks.sendCAD();
    }
  }

  refresh(): void {
    if (this.wmks) {
      this.wmks.updateScreen();
    }
  }

  dispose(): void {
    if (this.wmks) {
      this.wmks.disconnect();
    }
  }
}
