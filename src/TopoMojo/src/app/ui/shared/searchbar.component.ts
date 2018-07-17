import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'search-bar',
    templateUrl: './searchbar.component.html'
})
export class SearchBarComponent {
    timer: any;
    term: string = '';
    last: string = '';
    @Input() hasMore: boolean;
    @Output() onSearch: EventEmitter<string> = new EventEmitter<string>();
    @Output() onMore: EventEmitter<string> = new EventEmitter<string>();

    refresh(term) {
        this.term = term.trim();
        if (this.timer) { clearTimeout(this.timer);}
        this.timer = setTimeout(() => this.timerFired(), 500);
    }

    fire() {
        this.onSearch.emit(this.term);
    }

    timerFired() {
        if (this.term !== this.last) {
            this.last = this.term;
            this.onSearch.emit(this.term);
        }
    }

    more() {
        this.onMore.emit('');
    }
}