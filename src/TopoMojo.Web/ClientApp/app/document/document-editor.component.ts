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
    markdown: string = "# Title";
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