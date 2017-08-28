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
    pendingFiles : any[] = [];
    loading: boolean;
    uploading: boolean;
    errorMessage: string;

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

    private fileSelectorChanged(e) {
        console.log(e.srcElement.files);
        this.queuedFiles = [];
        for (let i = 0; i < e.srcElement.files.length; i++) {
            let file = e.srcElement.files[i];
            this.queuedFiles.push({
                key: this.id + "-" + file.name,
                name: file.name,
                file: file,
                progress: -1
            });
        }
        //this.queuedFiles = [ e.srcElement.files[0] ];
    }
    dequeueFile(qf) {
        this.queuedFiles.splice(this.queuedFiles.indexOf(qf),1);
    }

    filesQueued() {
        return this.queuedFiles.length > 0;
    }

    canUpload() {
        return this.queuedFiles.length > 0 && !this.uploading;
    }

    upload() {
        this.uploading = true;
        for (let i = 0; i < this.queuedFiles.length; i++)
            this.uploadFile(this.queuedFiles[i]);
        //this.queuedFiles = [];
    }

    private uploadFile(qf) {
        qf.progress = 0;
        this.service.uploadIso(this.id, qf.key, qf.file).subscribe(
            (result) => {
                this.select(qf.name);
            },
            (err) => {
                console.log(err.json());
                this.errorMessage = err.json().message;
            },
            () => {
                this.queuedFiles = [];
                this.uploading = false;
            }
        );
    }

}