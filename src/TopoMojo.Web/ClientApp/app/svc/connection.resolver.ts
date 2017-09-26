import { Resolve } from '@angular/router';
import { SignalR, ISignalRConnection, IConnectionOptions } from 'ng2-signalr';
import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';

@Injectable()
export class ConnectionResolver implements Resolve<ISignalRConnection> {

    constructor(
        private _signalR: SignalR,
        private auth: AuthService
    )  { }

    resolve() {
        // console.log('ConnectionResolver. Resolving...');
        let config: IConnectionOptions = { qs: "bearer=" + this.auth.currentUser.access_token };
        return this._signalR.createConnection(config);
    }
}