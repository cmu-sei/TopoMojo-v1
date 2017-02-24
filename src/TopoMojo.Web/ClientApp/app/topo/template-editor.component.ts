import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { TopoService } from './topo.service';

@Component({
    //moduleId: module.id,
    selector: 'template-editor',
    templateUrl: 'template-editor.component.html',
    styleUrls: [ 'template-editor.component.css' ]
})
export class TemplateEditorComponent implements OnInit {
    @Input() tref: any;
    @Output() onRemoved: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('cloneHelp') cloneHelp: any;
    editing: boolean;
    cloneVisible: boolean;
    vm: any;

    constructor(private service : TopoService) { }

    ngOnInit() {
        this.cascadeFields();
    }

    cascadeFields() {
        let tref = this.tref;
        if (!tref.name) tref.name = tref.template.name;
        if (!tref.description) tref.description = tref.template.description;
        if (!tref.iso) tref.iso = tref.template.iso;
        let detail = JSON.parse(tref.template.detail);
        if (!tref.networks) tref.networks = JSON.parse(tref.template.detail).Eth
            .map(function(e){ return e.Net;})
            .join(', ');
    }

    edit() {
        this.editing = !this.editing;
    }

    remove() {
        if ((!this.vm) || (this.vm.id))
            return; //don't remove if vm created

        this.service.removeTemplate(this.tref)
        .subscribe(result => {
            if (result) {
                this.onRemoved.emit(this.tref);
            }
        }, (err) => { this.service.onError(err); });
    }

    save() {
        this.editing = false;
        //this.tref.template = null;
        this.service.updateTemplate(this.tref)
        .subscribe(result => {
        }, (err) => { this.service.onError(err); });
    }

    clone() {
        if ((!this.vm) || (this.vm.id))
            return; //don't clone unless empty vm loaded

        this.cloneHelp.toggle();
        //this.cloneVisible = false;
        this.service.cloneTemplate(this.tref)
        .subscribe(result => {
            this.tref = result as any;
            this.cascadeFields();
        }, (err) => { this.service.onError(err); });
    }

    toggleClone() {
        this.cloneVisible = !this.cloneVisible;
    }

    vmLoaded(vm) {
        this.vm = vm;
    }
}