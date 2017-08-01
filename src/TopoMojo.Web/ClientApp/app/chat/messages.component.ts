import { AfterViewChecked, ElementRef, ViewChild, Component, OnInit, OnChanges, SimpleChanges,
    Input, OnDestroy, Inject } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { DOCUMENT } from "@angular/platform-browser";
import {AuthService} from "../auth/auth.service";
import { SettingsService } from "../auth/settings.service";
import { ChatService, MessageModel, ActorModel, PageModel } from './chat.service';
import { SignalR, BroadcastEventListener, SignalRConnection } from 'ng2-signalr';

import {Observable, Subscription, Subject} from 'rxjs/Rx';
import 'rxjs/add/operator/catch';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/switchMap';
import 'rxjs/add/operator/concatAll';
import 'rxjs/add/operator/debounceTime';


@Component({
    selector: 'chat-messages',
    templateUrl: './messages.component.html',
    styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnChanges {
    @Input() game: any;
    @ViewChild('scrollMe') private messagePanel: ElementRef;
    @Input() connection : SignalRConnection;
    private members: Array<ActorModel> = new Array<ActorModel>();
    private messages:Array<MessageModel> = new Array<MessageModel>();
    private newMessage : string = '';
    private subs: Subscription[] = [];
    private autoScroll: boolean = true;
    private messagePage: PageModel = new PageModel();
    private typingSource: Subject<boolean> = new Subject<boolean>();
    private typing$: Observable<boolean> = this.typingSource.asObservable();
    private typers: string = "";
    private typingTimer: any;

    constructor(
        private authService:AuthService,
        private route:ActivatedRoute,
        private router:Router,
        private service: ChatService,
        private settings: SettingsService,
        @Inject(DOCUMENT) private dom : Document
    ) {
        //this.connection = this.route.snapshot.data['connection'];
    }

    ngOnChanges(changes: SimpleChanges) {

        if (!this.game || !this.game.globalId)
            return;

        this.subs.push(
            this.connection.listenFor('ping').subscribe(
                (msg: ActorModel) => {
                    //console.log("ping: " + msg.name);
                    this.updateMember(msg, true);
                    this.connection.invoke('Pong', this.game.globalId);
                }
            )
        );

        this.subs.push(
            this.connection.listenFor('pong').subscribe(
                (msg: ActorModel) => {
                    //console.log(msg);
                    //console.log("pong: " + msg.name);
                    this.updateMember(msg, true);

                }
            )
        );

        this.subs.push(
            this.connection.listenFor("pung").subscribe(
                (msg: ActorModel) => {
                    //console.log("pung: " + msg.name);
                    this.updateMember(msg, false);

                }
            )
        );

        this.subs.push(
            this.connection.listenFor("posted").subscribe(
                (msg: MessageModel) => {
                    //console.log(msg);
                    this.messages.push(msg);
                }
            )
        );

        this.subs.push(
            this.connection.listenFor("typing").subscribe(
                (actor: ActorModel) => {
                    //console.log(actor.name + " is typing...");
                    this.typers = actor.name;
                    window.clearTimeout(this.typingTimer);
                    this.typingTimer = window.setTimeout(() => {
                        this.typers = "";
                    }, 1000);
                }
            )
        );

        this.subs.push(
            this.typing$.debounceTime(300).subscribe(
                () => {
                    if (!this.newMessage.endsWith('\n'))
                        this.connection.invoke("Typing", this.game.globalId);
                }
            )
        );


        this.connection.invoke("Listen", this.game.globalId);
        this.connection.invoke("Ping", this.game.globalId);

        // this.service.members(this.id).subscribe(
        //     (result : Array<MemberModel>) => {
        //         this.members = result;
        //     }
        // );
        // this.service.messages(this.id, this.messagePage).subscribe(
        //     (result) => {
        //         this.messages = result;
        //     }
        // );
    }

    //Todo: should be keying on member id, not name
    updateMember(member: ActorModel, online : boolean) : void {
        for (let i = 0; i < this.members.length; i++) {
            if (this.members[i].name == member.name) {
                this.members[i].online = online;
                return;
            }
        }
        member.online = online;
        this.members.push(member);
    }

    typing() {
        this.typingSource.next(true);
    }

    submitMessage() {
        if (!this.newMessage || this.newMessage == '\n') {
            this.newMessage = "";
            return;
        }

        let msg = new MessageModel();
        msg.text = this.newMessage;
        msg.actor = new ActorModel();
        msg.actor.id = this.authService.currentUser.profile.id;
        msg.actor.name = this.authService.currentUser.profile.name;
        this.messages.push(msg);
        this.newMessage = "";

        this.connection.invoke("Post", this.game.globalId, msg.text);
    }

    copyToClipboard(text: string) {
        let el = this.dom.getElementById("clipboardText") as HTMLTextAreaElement;
        el.value = text;
        el.select();
        this.dom.execCommand("copy");
    }

    pasteUrl() {
        this.copyToClipboard(this.game.shareCode);
    }

    ngAfterViewChecked() {
        this.scrollToBottom();
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
        try {
            this.connection.invoke("Leave", this.game.globalId);
        } catch(ex) { }

        this.subs.forEach(sub => {
            sub.unsubscribe();
        })
    }
}