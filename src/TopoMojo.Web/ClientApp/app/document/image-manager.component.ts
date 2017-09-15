import { Component, OnInit, Input, Inject } from '@angular/core';
import { DOCUMENT } from "@angular/platform-browser";
import { DocumentService } from '../api/document.service';
import { ImageFile } from "../api/api-models";

@Component({
    //moduleId: module.id,
    selector: 'image-manager',
    templateUrl: 'image-manager.component.html',
    styles: [`
        .offscreen {
            height: 0px;
            width: 0px;
            resize: none;
            border: 0px;
        }
        h4 {
            display: inline-block;
        }
        .upload-ui {
            display: inline-block;
        }
    `]
})
export class ImageManagerComponent implements OnInit {

    @Input() id : string;
    images : ImageFile[] = [];
    queuedFiles : any[] = [];
    clipboardText : string = '';

    constructor(
        private service : DocumentService,
        @Inject(DOCUMENT) private dom : Document
    ) { }

    ngOnInit() {
        this.list();
    }

    fileSelectorChanged(e) {
        console.log(e.srcElement.files);
        this.queuedFiles = e.srcElement.files;
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
        this.service.postImage(this.id, file)
        .subscribe((result : ImageFile) => {
            let found = this.images.filter(
                (e) => {
                    return e.filename == result.filename;
                }
            );
            if (found.length == 0)
                this.images.push(result);

            // let found = false;
            // for (let i = 0; i < this.images.length; i++) {
            //     found = this.images[i].filename == result.filename;
            //     if (found) break;
            // }
            // if (!found)
        });
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

    genMd(img : ImageFile) {
        let url = this.imagePath(img);
        this.clipboardText = "![" + img.filename + "](" + url + ")";
        let el = this.dom.getElementById("clipboardText") as HTMLTextAreaElement;
        el.value = this.clipboardText;
        el.select();
        this.dom.execCommand("copy");
    }

    delete(img : ImageFile) {
        this.service.deleteImage(this.id, img.filename)
        .subscribe((result : ImageFile) => {
            this.removeImage(result.filename);
        });
    }

    imageClicked(img) {
        console.log(img);
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