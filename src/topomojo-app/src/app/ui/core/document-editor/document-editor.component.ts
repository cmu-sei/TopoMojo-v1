import { Component, OnInit, Input, Inject, OnDestroy, ViewChild } from '@angular/core';
import { Converter } from 'showdown/dist/showdown';
import { DocumentService } from '../../../api/document.service';
import { SettingsService } from '../../../svc/settings.service';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, finalize } from 'rxjs/operators';
import { of, interval, timer, Observable, Subscription } from 'rxjs';
import { ToolbarService } from '../../svc/toolbar.service';
import { MatDrawer } from '@angular/material/sidenav';

@Component({
  selector: 'topomojo-document-editor',
  templateUrl: 'document-editor.component.html',
  styleUrls: ['document-editor.component.scss']
})
export class DocumentEditorComponent implements OnInit, OnDestroy {

  @ViewChild('imageDrawer') imageDrawer: MatDrawer;
  private converter: Converter;
  id: string;
  rendered: string;
  dirty: boolean;
  showImageDiv: boolean;
  markdown = '';
  private saveInterval$: Observable<number>;
  private saveInterval: Subscription;
  example = `
# Title
#### Subtitle

This is an introductory paragraph.
*Probably* important!

0. Do some task.
0. Do another task.

> Block quote formatting

Inline \`code formatting\` example.

    prompt> cat hello world | base64 > encoded.txt

or
\`\`\`
main() : void {
    console.log("hello, world");
}
\`\`\`

Normal markdown image linking works using \`![caption](url)\`.
If you need to store graphics, use the image manager to upload,
then paste in the MD text.
`;

  constructor(
    private service: DocumentService,
    private settingsSvc: SettingsService,
    private route: ActivatedRoute,
    private router: Router,
    private toolbar: ToolbarService
  ) {
    this.converter = new Converter(settingsSvc.settings.showdown);
  }

  ngOnInit() {

    setTimeout(() => this.initToolbar(), 1);

    this.id = this.route.snapshot.params['key'];
    // this.router.navigate([{ outlets: { sidenav: ['images', this.id]}}]);

    this.service.getDocument(this.id).pipe(
      catchError(err => of('# Document Title')),
      finalize(() => this.render())
    ).subscribe(
      (text: string) => {
        this.markdown = text;
      }
    );
  }

  ngOnDestroy() {
    this.toolbar.reset();
    // this.router.navigate([{ outlets: { sidenav: null}}]);

    if (this.saveInterval) { this.saveInterval.unsubscribe(); }
  }

  reRender() {
    if (!this.dirty) {
      // this.toolbar.buttons[0].color = 'accent';
      if (!this.saveInterval) {
        this.saveInterval = interval(30000).subscribe(() => this.save());
      }
    }
    this.dirty = true;
    this.render();
  }

  render() {
    this.rendered = this.converter.makeHtml(this.markdown);
  }

  save() {
    if (this.dirty) {
      // this.toolbar.buttons[0].color = 'default';
      // this.dirty = false;
      this.service.putDocument(this.id, this.markdown)
        .subscribe(result => {
          this.dirty = false;
        // this.toolbar.buttons[0].color = 'default';
        });
    }
  }

  initToolbar() {
    this.toolbar.sideComponent = 'docimages';
    this.toolbar.sideData = { key: this.id };
    this.toolbar.addButtons([
      {
        text: 'save',
        icon: 'cloud_upload',
        clicked: () => this.save(),
      },
      {
        text: 'toggle image manager',
        icon: 'image',
        clicked: () => this.toolbar.toggleSide()
      }
    ]);
  }

}
