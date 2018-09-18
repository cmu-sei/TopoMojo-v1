import { Component, OnInit, Input } from '@angular/core';
import { DocumentService } from '../../../api/document.service';
import { finalize } from 'rxjs/operators';
import { HttpEventType, HttpResponse } from '@angular/common/http';
import { ImageFile } from '../../../api/gen/models';
import { SettingsService } from '../../../svc/settings.service';
import { ActivatedRoute } from '@angular/router';
import { ToolbarService } from '../../svc/toolbar.service';
import { ClipboardService } from '../../../svc/clipboard.service';

@Component({
  selector: 'topomojo-document-image-manager',
  templateUrl: './document-image-manager.component.html',
  styleUrls: ['./document-image-manager.component.scss']
})
export class DocumentImageManagerComponent implements OnInit {
  @Input() bucketId = '';
  @Input() multiple = true;
  images: ImageFile[] = [];
  queuedFiles: any[] = [];
  loading: boolean;
  uploading: boolean;
  errors: Array<Error> = [];
  currentClip = '';

  constructor(
    private route: ActivatedRoute,
    private fileSvc: DocumentService,
    private settingsSvc: SettingsService,
    private toolbar: ToolbarService,
    private clipboard: ClipboardService
  ) { }

  ngOnInit() {
    this.bucketId = this.route.snapshot.paramMap.get('key');
    console.log(this.bucketId);
    // this.bucketId = this.toolbar.sideData.key;
    this.list();
  }

  list() {
    this.images = [];
    this.fileSvc.getImages(this.bucketId)
    .subscribe((result: ImageFile[]) => {
        this.images = result;
    });
  }

  freshUrl(img: ImageFile): string {
    return this.imageUrl + '?' + Date.now().toString();
  }

  imageUrl(img: ImageFile): string {
    return this.settingsSvc.settings.urls.docUrl + '/docs/' + this.bucketId + '/' + img.filename;
  }

  genMd(img: ImageFile): void {
    // this.copyToClipboard(`![${img.filename}](${this.imagePath(img)} =480x*)`);
    const md = `![${img.filename}](${this.imageUrl(img)} =480x*)`;
    this.clipboard.copyToClipboard(md);
    this.currentClip = img.filename;
  }

  delete(img: ImageFile) {
    this.fileSvc.deleteImage(this.bucketId, img.filename)
    .subscribe((result: ImageFile) => {
        this.images.splice(this.images.indexOf(img), 1);
    });
  }

  fileSelectorChanged(e) {
    // console.log(e.srcElement.files);
    this.queuedFiles = [];
    for (let i = 0; i < e.target.files.length; i++) {
      const file = e.target.files[i];
      this.queuedFiles.push({
        key: this.bucketId + '-' + file.name,
        name: file.name,
        file: file,
        progress: -1,
        state: 'remove_circle',
        error: ''
      });
    }
  }

  dequeueFile(qf) {
    this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1);
  }

  filesQueued() {
    return this.queuedFiles.length > 0;
  }

  canUpload() {
    return this.queuedFiles.length > 0 && !this.uploading;
  }

  upload() {
    this.uploading = true;
    for (let i = 0; i < this.queuedFiles.length; i++) {
      this.uploadFile(this.queuedFiles[i]);
    }
  }

  private uploadFile(qf): void {
    qf.state = 'inprogress';
    this.fileSvc.uploadImage(this.bucketId, qf.file).pipe(
      finalize(() => this.finished(qf))
    ).subscribe(
        (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            qf.progress = Math.round(100 * event.loaded / event.total);
          } else if (event instanceof HttpResponse) {
            qf.progress = 100;
            const item = this.images.findIndex(i => i.filename === event.body.filename);
            if (item >= 0) { this.images.splice(item, 1); }
            this.images.push(event.body);
          }
        },
        (err) => qf.error = err.error || err,
        // () => this.finished(qf)
      );
  }

  private finished(qf) {
    qf.state = (qf.progress === 100) ? 'check_circle' : 'error';
    // this.queuedFiles.splice(this.queuedFiles.indexOf(qf), 1);
    this.uploading = this.queuedFiles.filter(f => f.state === 'inprogress').length > 0;

  }

}
