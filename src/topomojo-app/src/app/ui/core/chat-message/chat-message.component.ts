import { Component, OnInit, Input, OnChanges } from '@angular/core';
import { SettingsService } from '../../../svc/settings.service';
import { Converter } from 'showdown';
import { Message } from '../../../api/gen/models';
import { RelativeMessage } from '../chat-panel/chat-panel.component';

@Component({
  selector: 'topomojo-chat-message',
  templateUrl: './chat-message.component.html',
  styleUrls: ['./chat-message.component.scss']
})
export class ChatMessageComponent implements OnChanges {
  @Input() message: RelativeMessage;
  private converter: Converter;
  private renderedHtml: string;
  private dateBreak: string;

  constructor(
      private settingsSvc: SettingsService
  ) {
      this.converter = new Converter(settingsSvc.settings.showdown);
  }

  ngOnChanges() {
      if (this.message) {
        this.dateBreak = `<div class="post date"><hr/><span>${this.message.date}</span></div>`;
        this.renderedHtml = `<div class="post">${this.converter.makeHtml(this.message.msg.text)}</div>`;
    }
  }

}
