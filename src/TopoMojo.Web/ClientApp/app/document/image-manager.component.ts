import { Component, OnInit, Input, Inject } from '@angular/core';
import { HttpEvent, HttpEventType, HttpResponse } from "@angular/common/http";
import { DOCUMENT } from '@angular/platform-browser';
import { DocumentService } from '../api/document.service';
import { CustomService } from '../api/custom.service';
import { ImageFile } from "../api/api-models";

@Component({
    //moduleId: module.id,
    selector: 'image-manager',
    templateUrl: 'image-manager.component.html',
    styleUrls: ['image-manager.component.css']
})
export class ImageManagerComponent implements OnInit {

    @Input() id : string;
    images : ImageFile[] = [];
    queuedFiles : any[] = [];
    clipboardText : string = '';

    constructor(
        private service : DocumentService,
        private custom : CustomService,
        @Inject(DOCUMENT) private dom : Document
    ) { }

    ngOnInit() {
        this.list();
    }

    // fileSelectorChanged(e) {
    //     console.log(e.srcElement.files);
    //     this.queuedFiles = e.srcElement.files;
    // }
    private fileSelectorChanged(e) {
        //console.log(e.srcElement.files);
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

    filesQueued() {
        return this.queuedFiles.length > 0;
    }

    dequeueFile(qf) {
        this.queuedFiles.splice(this.queuedFiles.indexOf(qf),1);
    }
    upload() {
        for (let i = 0; i < this.queuedFiles.length; i++)
            this.uploadFile(this.queuedFiles[i]);
        //this.queuedFiles = [];
    }

    uploadFile(qf) {
        this.custom.uploadImage(this.id, qf.file)
        .finally(() => this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1))
        .subscribe(
            (event) => {
                if (event.type === HttpEventType.UploadProgress) {
                    qf.progress = Math.round(100 * event.loaded / event.total);
                } else if (event instanceof HttpResponse) {
                    let imagefile = event.body;
                    let found = this.images.filter(
                        (e) => {
                            return e.filename == imagefile.filename;
                        }
                    );
                    if (found.length == 0)
                        this.images.push(imagefile);
                }
            },
            (err) => {

            }
        );
        // this.custom.uploadImage(this.id, file)
        // .subscribe((result : ImageFile) => {
        //     let found = this.images.filter(
        //         (e) => {
        //             return e.filename == result.filename;
        //         }
        //     );
        //     if (found.length == 0)
        //         this.images.push(result);

        // });
    }

    imagePath(img: ImageFile) : string {
        return "/docs/" + this.id + "/"+ img.filename;
    }

    list() {
        this.service.getImages(this.id)
        .subscribe((result : ImageFile[]) => {
            this.images = result;
        });
    }

    copyToClipboard(text : string) : void {
        let el = this.dom.getElementById("image-clipboard") as HTMLTextAreaElement;
        //console.log(el);
        el.value = text;
        el.select();
        this.dom.execCommand("copy");
    }

    genMd(img : ImageFile) : void {
        this.copyToClipboard(`![${img.filename}](${this.imagePath(img)})`);
    }

    delete(img : ImageFile) {
        this.custom.deleteImage(this.id, img.filename)
        .subscribe((result : ImageFile) => {
            this.removeImage(result.filename);
        });
    }

    private removeImage(fn) {
        for (let i = 0; i < this.images.length; i++) {
            if (this.images[i].filename == fn) {
                this.images.splice(i, 1);
                break;
            }
        }
    }
}