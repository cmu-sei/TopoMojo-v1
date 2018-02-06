import { Component, OnInit, Input, Inject } from '@angular/core';
import { Converter } from 'showdown/dist/showdown';
import { DocumentService } from '../../api/document.service';
import { SHOWDOWN_OPTS } from '../../svc/settings.service';

@Component({
    selector: 'document-editor',
    templateUrl: 'document-editor.component.html',
    styleUrls: [ 'document-editor.component.css']
})
export class DocumentEditorComponent implements OnInit {

    private converter : Converter;
    @Input() id : string;
    rendered : string;
    dirty : boolean;
    showImageDiv: boolean;
    markdown: string = `
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
`

    constructor(
        private service: DocumentService,
        @Inject(SHOWDOWN_OPTS) private showdown_opts
    ) {
        this.converter = new Converter(showdown_opts);
    }

    ngOnInit() {
        this.service.getDocument(this.id)
        .finally(() => this.render())
        .subscribe(
            (text : string) => {
                this.markdown = text;
            }
        );
    }

    reRender() {
        this.dirty = true;
        this.render();
    }

    render() {
        this.rendered = this.converter.makeHtml(this.markdown);
    }

    save() {
        this.service.putDocument(this.id, this.markdown)
        .subscribe(result => {
            this.dirty = false;
        });
    }
}