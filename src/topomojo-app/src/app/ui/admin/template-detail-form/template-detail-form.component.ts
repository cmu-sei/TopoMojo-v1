import { Component, OnInit, Input } from '@angular/core';
import { TemplateDetail } from '../../../api/gen/models';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'topomojo-template-detail-form',
  templateUrl: './template-detail-form.component.html',
  styleUrls: ['./template-detail-form.component.scss']
})
export class TemplateDetailFormComponent implements OnInit {
  @Input() template: TemplateDetail;
  form: NgForm;

  constructor() { }

  ngOnInit() {
  }

}
