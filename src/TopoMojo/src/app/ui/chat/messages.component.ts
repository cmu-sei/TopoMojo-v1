import { ElementRef, ViewChild, Component, OnChanges, Input } from '@angular/core';
import { NotificationService, TopoEvent } from '../../svc/notification.service';
import { ChatService } from '../../api/chat.service';
import { Message } from '../../api/gen/models';

import { Observable ,  Subject ,  Subscription } from "rxjs";
import { debounceTime } from 'rxjs/operators';
import { ClipboardService } from '../../svc/clipboard.service';
//import { setTimeout, clearTimeout } from 'timers';

@Component({
    selector: 'chat-messages',
    templateUrl: './messages.component.html',
    styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnChanges {
    @Input() space: any;
    @ViewChild('scrollMe') private messagePanel: ElementRef;
    notifier: NotificationService;
    messages:Array<Message> = new Array<Message>();
    typers: string = "";
    newMessage : string = '';
    showHistoryButton: boolean = false;
    private subs: Subscription[] = [];
    private autoScroll: boolean = true;
    private typingSource: Subject<boolean> = new Subject<boolean>();
    private typing$: Observable<boolean> = this.typingSource.asObservable();
    // private typingTimer: any;
    // private typingMonitor : any;
    private key: string;

    constructor(
        private service: ChatService,
        notifier: NotificationService,
        private clipboard: ClipboardService
    ) {
        this.notifier = notifier;
    }

    ngOnChanges() {

        if (!this.space || !this.space.globalId)
            return;

        this.key = this.space.globalId;
        this.moreHistory();

        this.subs.push(
            this.notifier.presenceEvents.subscribe(
                () => {
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
            this.typing$.pipe(debounceTime(500)).subscribe(
                (v) => {
                    this.notifier.typing(v);
                }
            )
        );

        //this.typingSource.next(false);

    }

    typing() {
        this.typingSource.next(true);
        // clearTimeout(this.typingMonitor);
        // this.typingMonitor = setTimeout(() => {
        //     this.typingSource.next(false);
        // }, 1000);
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
            () => {
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

    onScroll() {
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