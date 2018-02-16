import { AfterViewChecked, ElementRef, ViewChild, Component, OnInit, OnChanges, SimpleChanges,
    Input, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../svc/auth.service';
import { SettingsService } from '../../svc/settings.service';
import { NotificationService, TopoEvent, Actor, ChatMessage } from '../../svc/notification.service';
import { ChatService } from '../../api/chat.service';
import { Message } from '../../api/gen/models';

import {Observable, Subscription, Subject} from 'rxjs/Rx';
import { ClipboardService } from '../../svc/clipboard.service';
import { setTimeout, clearTimeout } from 'timers';

@Component({
    selector: 'chat-messages',
    templateUrl: './messages.component.html',
    styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnChanges {
    @Input() space: any;
    @ViewChild('scrollMe') private messagePanel: ElementRef;
    notifier: NotificationService;
    private messages:Array<Message> = new Array<Message>();
    private newMessage : string = '';
    private subs: Subscription[] = [];
    private autoScroll: boolean = true;
    private typingSource: Subject<boolean> = new Subject<boolean>();
    private typing$: Observable<boolean> = this.typingSource.asObservable();
    private typers: string = "";
    private typingTimer: any;
    private typingMonitor : any;
    private key: string;
    private showHistoryButton: boolean = false;

    constructor(
        private authService:AuthService,
        private route:ActivatedRoute,
        private router:Router,
        private service: ChatService,
        private settings: SettingsService,
        notifier: NotificationService,
        private clipboard: ClipboardService
    ) {
        this.notifier = notifier;
    }

    ngOnChanges(changes: SimpleChanges) {

        if (!this.space || !this.space.globalId)
            return;

        this.key = this.space.globalId;
        this.moreHistory();

        this.subs.push(
            this.notifier.presenceEvents.subscribe(
                (event) => {

                }
            ),
            this.notifier.chatEvents.subscribe(
                (event: TopoEvent) => {
                    switch (event.action) {
                        case "CHAT.TYPING":
                        this.typers = this.notifier.actors.filter((a) => a.typing).map((a) => a.name).join();
                        break;

                        case "CHAT.ADDED":
                        this.messages.push(
                            {
                                authorName: event.actor.name,
                                text: event.model
                            } as Message
                        );
                        break;
                    }
                }
            ),
            this.typing$.debounceTime(500).subscribe(
                (v) => {
                    this.notifier.typing(v);
                }
            )
        );

        this.typingSource.next(false);

    }

    typing() {
        this.typingSource.next(true);
        clearTimeout(this.typingMonitor);
        this.typingMonitor = setTimeout(() => {
            this.typingSource.next(false);
        }, 1000);
    }

    submitMessage() {
        if (!this.newMessage || this.newMessage == '\n') {
            this.newMessage = "";
            return;
        }

        let text = this.newMessage;
        // this.messages.push({
        //     text: text,
        //     actor: this.authService.currentUser.profile.name,
        // });
        this.newMessage = "";

        //this.notifier.sendChat(msg.text);
        this.service.postChat({
            roomId: this.key,
            text: text
        }).subscribe(
            (result : Message) => {
                //let the notification handle it.
            }
        );
    }

    pasteUrl() {
        this.clipboard.copyToClipboard(this.space.shareCode);
    }

    ngAfterViewChecked() {
        this.scrollToBottom();
    }

    moreHistory() {
        let take = 25;
        let marker = (this.messages.length) ? this.messages[0].id : 0;
        this.service.getChat(this.key, marker, take).subscribe(
            (result: Message[]) => {
                // if (result && result.length > 0) {
                    this.messages.unshift(...result.reverse());
                // }
                this.showHistoryButton = result.length == take;
            }
        );
    }

    private onScroll() {
        let element = this.messagePanel.nativeElement
        let atBottom = element.scrollHeight - element.scrollTop === element.clientHeight
        this.autoScroll = atBottom;
    }

    private scrollToBottom(): void {
        try {
            if (this.autoScroll) {
                let element = this.messagePanel.nativeElement
                element.scrollTop = element.scrollHeight;
            }
        } catch(err) { }
    }


    ngOnDestroy() {

        this.subs.forEach(sub => {
            sub.unsubscribe();
        })
    }
}