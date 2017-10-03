import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { TopologyService } from '../../api/topology.service';
import { FileService } from '../../api/file.service';
import { VmOptions } from '../../api/gen/models';

@Component({
    selector: 'iso-manager',
    templateUrl: 'iso-manager.component.html',
    styles: [`
    .ellipsis {
        text-overflow: ellipsis;
        overflow: hidden;
        max-width: 100%;
    }
    `
    ]
})

export class IsoManagerComponent implements OnInit {

    constructor(
        private service: TopologyService,
        private fileSvc: FileService
    ) { }

    @Input() id: string;
    @Output() onSelected: EventEmitter<string> = new EventEmitter<string>();
    isos: Array<string> = [];
    queuedFiles : any[] = [];
    pendingFiles : any[] = [];
    loading: boolean;
    uploading: boolean;
    errorMessage: string;

    ngOnInit() {
    }

    refresh() {
        this.loading = true;
        this.service.isosTopology(this.id)
        .subscribe(
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

    private uploadFile(qf) : void {
        this.fileSvc.uploadIso(this.id, qf.key, qf.file)
            .finally(() => this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1))
            .subscribe(
                (event) => {
                    if (event.type === HttpEventType.UploadProgress) {
                        qf.progress = Math.round(100 * event.loaded / event.total);
                    } else if (event instanceof HttpResponse) {
                        this.select(qf.name);
                    }
                },
                (err) => {

                }
            );
    }

    trunc(text: string) {
        if (text.length > 40)
            text = text.substring(0,40) + "...";
        return text;
    }
    // private uploadFile(qf) {
    //     qf.progress = 0;
    //     // this.service.uploadIso(this.id, qf.key, qf.file).subscribe(
    //     //     (result) => {
    //     //         this.select(qf.name);
    //     //     },
    //     //     (err) => {
    //     //         console.log(err.json());
    //     //         this.errorMessage = err.json().message;
    //     //     },
    //     //     () => {
    //     //         this.queuedFiles = [];
    //     //         this.uploading = false;
    //     //     }
    //     // );
    // }

}