import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { Template, Vm, Topology } from '../../../api/gen/models';
import { TemplateService } from '../../../api/template.service';
import { VmService } from '../../../api/vm.service';
import { forkJoin, Observable } from 'rxjs';
import { mapTo } from 'rxjs/operators';
import { IsoDataSource } from '../../datasources';
import { TopologyService } from '../../../api/topology.service';
import { VmControllerComponent } from '../../shared/vm-controller/vm-controller.component';

@Component({
  selector: 'topomojo-template',
  templateUrl: './template.component.html',
  styleUrls: ['./template.component.scss']
})
export class TemplateComponent implements OnInit {

  @Input() template: Template;
  // @Input() workspaceId: string;
  @Output() deleted = new EventEmitter<Template>();
  @Output() cloned = new EventEmitter<Template>();
  vm: Vm = {};
  private isoSource: IsoDataSource;
  @ViewChild(VmControllerComponent) vmcontroller: VmControllerComponent;

  constructor(
    private templateSvc: TemplateService,
    private topologySvc: TopologyService,
    private vmSvc: VmService
  ) { }

  ngOnInit() {
    this.isoSource = new IsoDataSource(this.topologySvc, this.template.topologyGlobalId);
  }

  vmLoaded(vm: Vm) {
    this.vm = vm;
  }

  unlink() {
    this.templateSvc.postTemplateUnlink({
      templateId: this.template.id,
      topologyId: this.template.topologyId
    }).subscribe(t => {
      this.template = t;
      this.cloned.emit(t);
      this.vmcontroller.load();
    });
  }

  isoChanged(iso: string) {
    this.template.iso = iso;
    // this.save();
    console.log(iso);

    if (this.vm.id) {
        this.vmSvc.postVmChange(this.vm.id, { key: 'iso', value: iso }).subscribe(
            (result) => {

            }
        );
    }
  }

  delete() {

    let q: Observable<any> = this.templateSvc.deleteTemplate(this.template.id);
    if (!!this.vm.id) {
      q = forkJoin(q, this.vmSvc.deleteVm(this.vm.id).pipe(mapTo(true)));
    }
    q.subscribe(() => this.deleted.emit(this.template));
  }
}
