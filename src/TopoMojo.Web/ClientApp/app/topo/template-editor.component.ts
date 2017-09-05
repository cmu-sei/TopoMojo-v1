import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { TopoService } from './topo.service';
import { NotificationService } from '../shared/notification.service';

@Component({
    //moduleId: module.id,
    selector: 'template-editor',
    templateUrl: 'template-editor.component.html',
    styleUrls: [ 'template-editor.component.css' ]
})
export class TemplateEditorComponent implements OnInit {
    @Input() tref: any;
    @Input() trefs: any[];
    @Input() topo: any;
    @Output() onRemoved: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('cloneHelp') cloneHelp: any;
    editing: boolean;
    cloneVisible: boolean;
    vm: any;
    isosVisible: boolean;
    uploadVisible: boolean;

    constructor(
        private service : TopoService,
        private notifier: NotificationService
    ) { }

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

        this.service.removeTemplate(this.tref).subscribe(
            (result) => {
                if (result) {
                    this.notifier.sendTemplateEvent("TEMPLATE.REMOVED", this.tref);
                    for (let i=0; i<this.trefs.length; i++) {
                        if (this.trefs[i].id == this.tref.id) {
                            this.trefs.splice(i, 1);
                            break;
                        }
                    }
                    //this.onRemoved.emit(this.tref);
                }
            },
            (err) => { this.service.onError(err); }
        );
    }

    save() {
        //this.editing = false;
        //this.tref.template = null;
        this.service.updateTemplate(this.tref).subscribe(
            (result) => {
                //this.notifier.sendTemplateEvent("TEMPLATE.UPDATED", this.tref);
            },
            (err) => { this.service.onError(err); }
        );
    }

    clone() {
        if ((!this.vm) || (this.vm.id))
            return; //don't clone unless empty vm loaded

        this.cloneHelp.toggle();
        this.service.cloneTemplate(this.tref).subscribe(
            (result) => {
                this.tref = result as any;
                this.cascadeFields();
            },
            (err) => { this.service.onError(err); }
        );
    }

    toggleClone() {
        this.cloneVisible = !this.cloneVisible;
    }

    vmLoaded(vm) {
        this.vm = vm;
    }

    isoChanged(iso : string) {
        this.tref.iso = iso;
        this.save();
        this.isosVisible = false;
        // todo: if vm, change iso
    }
}