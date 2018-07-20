import { Component, OnChanges, Input, Inject } from '@angular/core';
import { Converter } from 'showdown/dist/showdown';
import { SHOWDOWN_OPTS } from '../../svc/settings.service';
import { Message } from '../../api/gen/models';

@Component({
    selector: 'app-chat-message',
    templateUrl: 'message.component.html',
    styles: [`
    div.message {
        margin: 4px 8px 16px 8px;
    }
    div.text {
        margin: 2px 16px;
    }
    `]
})
export class MessageComponent implements OnChanges {

    @Input() message: Message;
    private converter: Converter;
    private renderedHtml: string;

    constructor(
        @Inject(SHOWDOWN_OPTS) private md_opts
    ) {
        this.converter = new Converter(md_opts);
    }

    ngOnChanges() {
        if (this.message) {
            this.renderedHtml = this.converter.makeHtml(this.message.text);
        }
    }

}
