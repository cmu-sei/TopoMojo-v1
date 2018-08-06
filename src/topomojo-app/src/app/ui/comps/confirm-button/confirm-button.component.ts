import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'topomojo-confirm-button',
  templateUrl: './confirm-button.component.html',
  styleUrls: ['./confirm-button.component.scss']
})
export class ConfirmButtonComponent implements OnInit {
  @Input() icon = 'delete';
  @Input() text = 'Delete';
  @Input() color = 'warn';
  @Input() disabled = false;
  @Input() fab = false;
  @Output() confirmed = new EventEmitter();
  confirming = false;

  constructor() { }

  ngOnInit() {
  }

  confirm() {
    this.confirmed.emit();
    this.confirming = false;
  }
}
