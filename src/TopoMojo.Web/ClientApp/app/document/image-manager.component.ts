import { Component, OnInit, Input, Inject } from '@angular/core';
import { DocumentService } from './document.service';
import { DOCUMENT } from "@angular/platform-browser";

export interface DocImage {
    filename: string;
}

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
    `]
})
export class ImageManagerComponent implements OnInit {

    @Input() id : string;
    images : DocImage[] = [];
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
        this.service.upload(this.id, file).subscribe((result : DocImage) => {
            let found = false;
            for (let i = 0; i < this.images.length; i++) {
                found = this.images[i].filename == result.filename;
                if (found) break;
            }
            if (!found)
                this.images.push(result);
        });
    }

    imagePath(img: DocImage) : string {
        return "/docs/" + this.id + "/"+ img.filename;
    }

    list() {
        this.service.listFiles(this.id).subscribe((result : DocImage[]) => {
            this.images = result;
        });
    }

    genMd(img : DocImage) {
        let url = this.imagePath(img);
        this.clipboardText = "![" + img.filename + "](" + url + ")";
        let el = this.dom.getElementById("clipboardText") as HTMLTextAreaElement;
        el.value = this.clipboardText;
        el.select();
        this.dom.execCommand("copy");
    }

    delete(img : DocImage) {
        this.service.delete(this.id, img.filename).subscribe((result : DocImage) => {
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