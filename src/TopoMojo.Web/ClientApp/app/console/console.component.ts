import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { VmService } from '../api/vm.service';
//import { WMKS } from 'vmware-wmks/wmks.min';

@Component({
    selector: 'console',
    templateUrl: 'console.component.html',
    styleUrls: [ 'console.component.css' ]
})
export class ConsoleComponent implements OnInit {

    wmks : any;
    showTools : boolean;
    networks : string[] = [];

    constructor(
        private service : VmService,
        private route : ActivatedRoute
    ) { }

    ngOnInit() {
        this.reconnect();
    }

    reconnect() {
        // this.wmks = WMKS.createWMKS('console-canvas-div',{
        //     rescale: false,
        //     position: 0,
        //     changeResolution: false,
        //     useVNCHandshake: false
        // })
        // .register(WMKS.CONST.Events.CONNECTION_STATE_CHANGE, function(event,data){
        //     if(data.state == WMKS.CONST.ConnectionState.CONNECTED){
        //         console.log('connection state change : connected');
        //         //$("#console-tools-connected-div").removeClass('hidden');

        //         //uiConnected();
        //         // wmks.enableInputDevice(0);
        //         // wmks.enableInputDevice(1);
        //         // wmks.enableInputDevice(2);

        //     }
        //     if(data.state == WMKS.CONST.ConnectionState.CONNECTING){
        //         console.log('connection state change : connecting ' + data.vvc + ' ' + data.vvcSession);
        //     }
        //     if(data.state == WMKS.CONST.ConnectionState.DISCONNECTED){
        //         console.log('connection state change : disconnected ' + data.reason + ' ' + data.code);
        //         // wmks.disableInputDevice(0);
        //         // wmks.disableInputDevice(1);
        //         // wmks.disableInputDevice(2);
        //         this.wmks.disconnect();
        //         this.wmks.destroy();

        //         //wmks = null;
        //         //uiDisconnected();
        //     }

        // })
        // .register(WMKS.CONST.Events.REMOTE_SCREEN_SIZE_CHANGE, function (e, data) {
        //     console.log('wmks remote_screen_size_change: ' + data.width + 'x' + data.height);
        //     // $('#console-canvas-div')[0].width=data.width;
        //     // $('#console-canvas-div')[0].height=data.height;
        // })
        // .register(WMKS.CONST.Events.HEARTBEAT, function (e, data) {
        //     console.log('wmks heartbeat: ' + data);
        // })
        // .register(WMKS.CONST.Events.COPY, function (e, data) {
        //     console.log('wmks copy: ' + data);
        // })
        // .register(WMKS.CONST.Events.ERROR, function (e, data) {
        //     console.log('wmks error: ' + data.errorType);

        // })
        // .register(WMKS.CONST.Events.FULL_SCREEN_CHANGE, function (e, data) {
        //     console.log('wmks full_screen_change: ' + data.isFullScreen);
        // });

        // // this.route.params
        // //     .switchMap((params: Params) => this.service.console(params['id']))
        // //     .subscribe(result => {
        // //        this.wmks = WMKS.createWMKS();
        // //     });

        // console.log(this.wmks);
    }

    toggleTools() {
        this.showTools = !this.showTools;
    }

    tryFullScreen() {

    }

    tryScaling() {

    }

    toggleNets() {

    }
}