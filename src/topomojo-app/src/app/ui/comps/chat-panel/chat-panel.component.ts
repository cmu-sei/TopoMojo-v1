import { Component, OnInit, Input, ViewChild, ElementRef, OnChanges, OnDestroy, AfterViewChecked } from '@angular/core';
import { NotificationService, HubEvent, Actor } from '../../../svc/notification.service';
import { Message } from '../../../api/gen/models';
import { Subscription, Subject, Observable } from 'rxjs';
import { ChatService } from '../../../api/chat.service';
import { distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'topomojo-chat-panel',
  templateUrl: './chat-panel.component.html',
  styleUrls: ['./chat-panel.component.scss']
})
export class ChatPanelComponent implements OnInit, OnDestroy, AfterViewChecked {
  @ViewChild('scrollMe') private messagePanel: ElementRef;
  messages = new Array<RelativeMessage>();
  typers = '';
  newMessage = '';
  showHistoryButton = false;
  private lastMsg: RelativeMessage = { msg: {}, hdr: true, time: '', date: '', utc: 0};
  private timeFormat = { hour12: true, hour: '2-digit', minute: '2-digit' };
  private dateFormat = { weekday: 'short', month: 'short', day: '2-digit', year: 'numeric' };
  private subs: Subscription[] = [];
  private autoScroll = true;
  private typingSource: Subject<boolean> = new Subject<boolean>();
  private typing$: Observable<boolean> = this.typingSource.asObservable();
  private typingMonitor: any;
  key: string;
  actors: Array<Actor>;
  constructor(
    private service: ChatService,
    private notifier: NotificationService
    // private clipboard: ClipboardService
  ) {
  }

  ngOnInit() {

    this.subs.push(
      this.notifier.key$.subscribe(key => {
        this.key = key;
        this.messages = [];
        this.moreHistory();
      }),
      this.notifier.actors$.subscribe(actors => {
        this.actors = actors;
        this.typers = this.actors.filter((a) => a.typing).map((a) => a.name).join();
      }),
      this.notifier.chatEvents.subscribe(
        (event: HubEvent) => {
          switch (event.action) {
            case 'CHAT.TYPING':
              // this.actors.forEach(a => { if (a.id === event.actor.id) { a.typing = event.model; }});
              break;

            case 'CHAT.ADDED':
              this.messages.push(this.wrapMsg(event.model));
              break;
          }
        }
      ),
      this.typing$.pipe(
        distinctUntilChanged()
      ).subscribe(
        (v) => {
          this.notifier.typing(v);
        }
      )
    );
  }

  typing() {
    this.typingSource.next(true);
    clearTimeout(this.typingMonitor);
    this.typingMonitor = setTimeout(() => {
      this.typingSource.next(false);
    }, 1000);
  }

  submitMessage() {
    if (!this.newMessage || this.newMessage === '\n') {
      this.newMessage = '';
      return;
    }

    const text = this.newMessage;
    this.newMessage = '';

    // this.messages.push({
    //   text: text,
    //   authorName: 'me',
    // });

    this.service.postChat({
      roomId: this.key,
      text: text
    }).subscribe();
  }

  pasteUrl() {
    // this.clipboard.copyToClipboard(this.space.shareCode);
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  moreHistory() {
    if (this.key) {
      const take = 25;
      const marker = (this.messages.length) ? this.messages[0].msg.id : 0;
      this.service.getChats(this.key, marker, take).subscribe(
        (result: Message[]) => {
          const currentLast = this.lastMsg;
          this.lastMsg = this.messages.length ? this.messages[0] : { msg: {}, hdr: true, time: '', date: '', utc: 0};
          this.messages.unshift(...result.reverse().map(m => this.wrapMsg(m)));
          this.lastMsg = currentLast;
          this.showHistoryButton = result.length === take;
        }
      );
    }
  }

  onScroll() {
    const element = this.messagePanel.nativeElement;
    const atBottom = element.scrollHeight - element.scrollTop === element.clientHeight;
    this.autoScroll = atBottom;
  }

  private scrollToBottom(): void {
    try {
      if (this.autoScroll) {
        const element = this.messagePanel.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    } catch (err) { }
  }

  wrapMsg(msg: Message): RelativeMessage {
    const utc = Date.parse(msg.whenCreated + ' UTC');
    const dt = new Date(utc);
    const rm = {
      msg: msg,
      hdr: msg.authorName !== this.lastMsg.msg.authorName || utc - this.lastMsg.utc > 300000,
      time: dt.toLocaleTimeString('en-US', this.timeFormat),
      date: dt.getDate() !== new Date(this.lastMsg.utc).getDate() ? dt.toLocaleDateString('en-US', this.dateFormat) : '',
      utc: utc
    };
    // if (rm.date) { console.log(rm.date); }
    this.lastMsg = rm;
    return rm;
  }

  ngOnDestroy() {

    this.subs.forEach(sub => {
      sub.unsubscribe();
    });
  }
}

export interface RelativeMessage {
  msg: Message;
  hdr: boolean;
  date: string;
  time: string;
  utc: number;
}
