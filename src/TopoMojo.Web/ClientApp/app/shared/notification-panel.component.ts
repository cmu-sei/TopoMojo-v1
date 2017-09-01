import { Component, OnInit, OnDestroy } from '@angular/core';
import {Observable, Subscription, Subject} from 'rxjs/Rx';
import { NotificationService } from './notification.service';
import {TranslateService} from '@ngx-translate/core';

@Component({
    selector: 'notification-panel',
    templateUrl: './notification-panel.component.html',
    styleUrls: ['./notification-panel.component.css']
})
export class NotificationPanelComponent implements OnInit, OnDestroy {
    constructor(
        private svc : NotificationService,
        private translate: TranslateService
    ) { }

    messages : Array<Notification> = new Array<Notification>();
    events: Array<any> = new Array<any>();

    private subs: Subscription[] = [];

    ngOnInit() {
        this.subs.push(
            this.svc.presenceEvents.subscribe(
                (event) => {
                    this.push(event);
                    // this.add(new Notification(
                    //     event.actor.name + " " + event.action + "."
                    // ));
                }
            ),
            this.svc.topoEvents.subscribe(
                (event) => {
                    this.add(new Notification(
                        event.actor.name + " " + event.action + " topo."
                    ));
                }
            )
        );
    }

    add(msg : Notification) : void {
        this.messages.push(msg);
        setTimeout(() => {
            this.remove(msg);
        }, 8000);
    }
    push(event : any) : void {
        this.events.push(event);
        setTimeout(() => {
            this.pop(event);
        }, 8000);
    }

    remove(msg : Notification) : void {
        this.messages.splice(this.messages.indexOf(msg), 1);
    }
    pop(event : any) : void {
        this.events.splice(this.events.indexOf(event), 1);
    }

    dotranslate(event : any) {
        return this.translate.get(event.action, event.actor);
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