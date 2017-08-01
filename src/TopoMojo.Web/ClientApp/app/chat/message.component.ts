import { Component, OnChanges, Input } from '@angular/core';
import { MessageModel } from './chat.service';
import { Converter } from 'showdown/dist/showdown';
import { SHOWDOWN_OPTS } from '../shared/constants/ui-params';

@Component({
    selector: 'chat-message',
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

    @Input() message: MessageModel;
    private converter: Converter;
    private renderedHtml: string;

    constructor(
    ) {
        this.converter = new Converter(SHOWDOWN_OPTS);
    }

    ngOnChanges() {
        if (this.message) {
            this.renderedHtml = this.converter.makeHtml(this.message.text);
        }
    }

}