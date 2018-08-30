import { Component, OnInit, Input, AfterViewInit, ViewChild, OnChanges, SimpleChange, SimpleChanges } from '@angular/core';
import { Template, ChangedTemplate, Topology } from '../../../api/gen/models';
import { NgForm } from '@angular/forms';
import { debounceTime } from 'rxjs/operators';
import { TemplateService } from '../../../api/template.service';
import { IsoDataSource, IsoFile } from '../../datasources';
import { TopologyService } from '../../../api/topology.service';

@Component({
  selector: 'topomojo-template-settings',
  templateUrl: './template-settings.component.html',
  styleUrls: ['./template-settings.component.scss']
})
export class TemplateSettingsComponent implements OnInit, AfterViewInit {
  @Input() template: Template;
  @Input() hasVm = false;
  // @Input() workspaceId: string;
  @ViewChild(NgForm) form: NgForm;
  isoSource: IsoDataSource;
  showingIsos = false;

  constructor(
    private service: TemplateService,
    private topologySvc: TopologyService
  ) { }

  ngOnInit() {
    this.isoSource = new IsoDataSource(this.topologySvc, this.template.topologyGlobalId);
  }

  ngAfterViewInit() {
    // this.form.valueChanges.pipe(debounceTime(1000)).subscribe(
    //   (model) => {
    //     if (this.form.valid && this.form.touched && model.id) {
    //       // this.service.putTemplate(model as ChangedTemplate).subscribe(
    //       //   // TODO: animate feedback
    //       // );
    //     }
    //   }
    // );
  }

  update(form: NgForm) {
    if (this.form.valid && this.form.value.id) {
      this.service.putTemplate(this.form.value as ChangedTemplate).subscribe(
        // TODO: animate feedback
        (t) => {
          this.form.reset(this.form.value);
        }
      );
    }
  }

  isoChanged(iso: IsoFile) {
    this.template.iso = iso.path;
    this.showingIsos = false;
  }
}
