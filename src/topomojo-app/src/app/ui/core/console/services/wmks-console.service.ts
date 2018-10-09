import { Injectable } from '@angular/core';
import { ConsoleService } from './console.service';
declare var WMKS: any;

@Injectable()
export class WmksConsoleService implements ConsoleService {
  private wmks;
  options: any = {
    rescale: true,
    changeResolution: true,
    useVNCHandshake: false,
    position: 0, // WMKS.CONST.Position.CENTER,
  };
  stateChanged: Function = (state: string) => { };

  constructor() { }

  connect(url: string, stateCallback: Function, options: any = {} ): void {

    if (stateCallback) { this.stateChanged = stateCallback; }
    this.options = {...this.options, ...options};

    if (this.wmks) {
      this.wmks.destroy();
      this.wmks = null;
    }

    let wmks = WMKS.createWMKS(options.canvasId, this.options)
    .register(WMKS.CONST.Events.CONNECTION_STATE_CHANGE, function(event, data) {

      switch (data.state) {
        case WMKS.CONST.ConnectionState.CONNECTED:
        stateCallback('connected');
        break;

        case WMKS.CONST.ConnectionState.DISCONNECTED:
        stateCallback('disconnected');
        wmks.destroy();
        wmks = null;
        break;
      }
    })
    .register(WMKS.CONST.Events.REMOTE_SCREEN_SIZE_CHANGE, function (e, data) {
      // debug('wmks remote_screen_size_change: ' + data.width + 'x' + data.height);
      // TODO: if embedded, pass along dimension to canvas wrapper element
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

    this.wmks = wmks;

    try {
      this.wmks.connect(url);
    } catch (err) {
      stateCallback('failed');
    }
  }

  disconnect() {
    if (this.wmks) {
      this.wmks.disconnect();
      this.stateChanged('disconnected');
      if (this.options.hideDisconnectedScreen ) {
        this.dispose();
      }
    }
  }

  sendCAD(): void {
    if (this.wmks) {
      this.wmks.sendCAD();
    }
  }

  refresh(): void {
    if (this.wmks && this.options.rescale) {
      this.wmks.updateScreen();
    }
  }

  toggleScale() {
    if (this.wmks) {
      this.options.rescale = !this.options.rescale;
      this.wmks.setOption('rescale', this.options.rescale);
    }
  }

  fullscreen() {
    if (this.wmks && !this.wmks.isFullScreen() && this.wmks.canFullScreen()) {
      this.wmks.enterFullScreen();
    }
  }

  showKeyboard() {
    if (this.wmks) {
      this.wmks.showKeyboard();
    }
  }

  showExtKeypad() {
    if (this.wmks) {
      this.wmks.toggleExtendedKeypad();
    }
  }

  showTrackpad() {
    if (this.wmks) {
      this.wmks.toggleTrackpad();
    }
  }

  dispose(): void {
    if (this.wmks && this.wmks.destroy) {
      console.log('disposing wmks');
      this.wmks.destroy();
      this.wmks = null;
    }
  }
}
