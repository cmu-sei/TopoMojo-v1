import { Component, OnInit, OnChanges, Input } from '@angular/core';
import { Converter } from 'showdown/dist/showdown';
import { DocumentService } from './document.service';
import { SHOWDOWN_OPTS } from '../shared/constants/ui-params';

@Component({
    //moduleId: module.id,
    selector: 'document-editor',
    templateUrl: 'document-editor.component.html'
})
export class DocumentEditorComponent implements OnInit {

    @Input()
    id : any;
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
    rendered : string;
    private converter : Converter;
    dirty : boolean;

    constructor(
        private service: DocumentService,
    ) {
        this.converter = new Converter(SHOWDOWN_OPTS);
    }

    ngOnInit() {
        //load doc from id
        this.load();
        this.render();
    }

    ngOnChanges() {

    }

    render() {
        this.rendered = this.converter.makeHtml(this.markdown);
    }

    load() {
        this.service.loadDoc(this.id).subscribe(result => {
            this.markdown = result;
            this.render();
        });
    }

    save() {
        this.service.saveDoc(this.id, this.markdown).subscribe(result => {
            this.dirty = false;
        });
    }
}