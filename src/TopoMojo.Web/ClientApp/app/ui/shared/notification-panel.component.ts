import { Component, OnInit, OnDestroy } from '@angular/core';
import {Observable, Subscription, Subject} from 'rxjs/Rx';
import { NotificationService } from '../../svc//notification.service';

@Component({
    selector: 'notification-panel',
    templateUrl: './notification-panel.component.html',
    styleUrls: ['./notification-panel.component.css']
})
export class NotificationPanelComponent implements OnInit, OnDestroy {
    constructor(
        private svc : NotificationService
    ) { }

    messages : Array<Notification> = new Array<Notification>();
    events: Array<any> = new Array<any>();

    private subs: Subscription[] = [];

    ngOnInit() {
        this.subs.push(
            this.svc.presenceEvents.subscribe(
                (event) => {
                    this.push(event);
                }
            ),
            this.svc.topoEvents.subscribe(
                (event) => {
                    this.push(event);
                }
            ),
            this.svc.templateEvents.subscribe(
                (event) => {
                    this.push(event);
                }
            ),
            this.svc.vmEvents.subscribe(
                (event) => {
                    this.push(event);
                }
            )
        );
    }

    push(event : any) : void {
        this.events.push(event);
        setTimeout(() => {
            this.pop(event);
        }, 8000);
    }

    pop(event : any) : void {
        this.events.splice(this.events.indexOf(event), 1);
    }

    ngOnDestroy() {
        for (let i = 0; i < this.subs.length; i++) {
            this.subs[i].unsubscribe();
        }
    }

}

export class Notification {
    id : string;
    text : string;
    progress: number;

    constructor(text: string, id?: string, progress? : number) {
        this.text = text;
        this.id = id;
        this.progress = progress;
    }
}