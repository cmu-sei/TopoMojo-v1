import { Component, OnInit, Input, Output } from '@angular/core';
import { HttpEventType, HttpResponse } from '@angular/common/http';
import { FileService } from '../../../api/file.service';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'topomojo-file-uploader',
  templateUrl: './file-uploader.component.html',
  styleUrls: ['./file-uploader.component.scss']
})
export class FileUploaderComponent implements OnInit {
  @Input() bucketId = '';
  @Input() multiple = false;
  queuedFiles: any[] = [];
  pendingFiles: any[] = [];
  loading: boolean;
  uploading: boolean;
  publicId = '00000000-0000-0000-0000-000000000000';
  errorMessage: string;
  errors: Array<Error> = [];

  constructor(
    private fileSvc: FileService
  ) { }

  ngOnInit() {
  }

  private fileSelectorChanged(e) {
    this.queuedFiles = [];
    for (let i = 0; i < e.target.files.length; i++) {
      const file = e.target.files[i];
      this.queuedFiles.push({
        bucketId: this.bucketId,
        key: this.bucketId + '-' + file.name,
        name: file.name,
        file: file,
        public: false,
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
    this.fileSvc.uploadIso(qf.bucketId, qf.file).pipe(
      finalize(() => this.finished(qf))
    ).subscribe(
        (event) => {
          if (event.type === HttpEventType.UploadProgress) {
            qf.progress = Math.round(100 * event.loaded / event.total);
            // TODO: broadcast notifaction to workspace at some interval
          } else if (event instanceof HttpResponse) {
            // if (!qf.name.match(/.*\.iso/)) { qf.name += '.iso'; }
            qf.progress = 100;
            // this.select(`${this.id}/${qf.name}`);
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
    console.log(this.uploading);
  }

  toggleScope(qf): void {
    qf.public = !qf.public;
    qf.bucketId = (qf.public) ? this.publicId : this.bucketId;
  }
}
