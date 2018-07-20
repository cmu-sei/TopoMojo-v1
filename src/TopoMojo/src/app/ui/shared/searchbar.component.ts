import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'search-bar',
    templateUrl: './searchbar.component.html'
})
export class SearchBarComponent {
    timer: any;
    term = '';
    last = '';
    @Input() hasMore: boolean;
    @Output() searched: EventEmitter<string> = new EventEmitter<string>();
    @Output() mored: EventEmitter<string> = new EventEmitter<string>();

    refresh(term) {
        this.term = term.trim();
        if (this.timer) { clearTimeout(this.timer); }
        this.timer = setTimeout(() => this.timerFired(), 500);
    }

    fire() {
        this.searched.emit(this.term);
    }

    timerFired() {
        if (this.term !== this.last) {
            this.last = this.term;
            this.searched.emit(this.term);
        }
    }

    more() {
        this.mored.emit('');
    }
}
