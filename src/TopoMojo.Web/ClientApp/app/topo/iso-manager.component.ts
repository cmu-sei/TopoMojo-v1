import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { TopoService } from './topo.service';

@Component({
    selector: 'iso-manager',
    templateUrl: 'iso-manager.component.html'
})

export class IsoManagerComponent implements OnInit {

    constructor(
        private service: TopoService
    ) { }

    @Input() id: string;
    @Output() onSelected: EventEmitter<string> = new EventEmitter<string>();
    isos: Array<any> = [];
    queuedFiles : any[] = [];
    loading: boolean;
    uploading: boolean;

    ngOnInit() {
    }

    refresh() {
        this.loading = true;
        this.service.getIsos(this.id).subscribe(
            (result) => {
                this.isos = result.iso;
            },
            (err) => {
            },
            () => {
                this.loading = false;
            }
        )
    }

    select(iso: string) {
        this.onSelected.emit(iso);
    }

    fileSelectorChanged(e) {
        // console.log(e.srcElement.files);
        this.queuedFiles = [ e.srcElement.files[0] ];
    }

    filesQueued() {
        return this.queuedFiles.length > 0;
    }

    upload() {
        for (let i = 0; i < this.queuedFiles.length; i++)
            this.uploadFile(this.queuedFiles[i]);
        this.queuedFiles = [];
    }

    uploadFile(file) {
        // console.log(file);
        this.service.uploadIso(this.id, file).subscribe(
            (result) => {
                this.select("private/" + file.name);
            }
        );
    }

}