import { Component, OnInit, Input, ViewChild } from '@angular/core';
import { TemplateDetail } from '../../../api/gen/models';
import { NgForm } from '@angular/forms';
import { TemplateService } from '../../../api/template.service';

@Component({
  selector: 'topomojo-template-detail-form',
  templateUrl: './template-detail-form.component.html',
  styleUrls: ['./template-detail-form.component.scss']
})
export class TemplateDetailFormComponent implements OnInit {
  @Input() template: TemplateDetail;
  @ViewChild(NgForm) form: NgForm;
  errors: Array<Error> = [];

  constructor(
    private templateSvc: TemplateService
  ) { }

  ngOnInit() {
  }

  update() {
    try {
        const s = JSON.parse(this.template.detail);
        this.templateSvc.putTemplateDetail(this.template).subscribe(
            (data) => {
                this.form.reset(this.form.value);
            },
            (err) => { }
        );
    } catch (e) {
        // this.errorMsg = e.split('\n').reverse().pop();
        this.errors.push(e);
    }
  }
}
