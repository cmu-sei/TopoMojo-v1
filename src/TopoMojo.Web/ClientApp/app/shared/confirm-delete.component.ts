import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    // moduleId: module.id,
    selector: 'confirm-delete',
    template: `
        <button *ngIf="!deleteMsgVisible" class="btn btn-danger" (click)="confirm()">
            <i class="fa fa-trash"></i> {{ label }}
        </button>
        <p *ngIf="deleteMsgVisible" class="alert alert-danger">
            {{ prompt }}
            <button (click)="delete()" class="btn btn-danger btn-sm">
                <i class="fa fa-trash"></i> Delete</button>
            <button (click)="cancel()" class="btn btn-danger btn-sm">
                <i class="fa fa-remove"></i> Cancel</button>
        </p>
    `
})
export class ConfirmDeleteComponent implements OnInit {

    deleteMsgVisible: boolean;
    @Input() prompt: string = "Please confirm.";
    @Input() label: string = '';
    @Output() onDelete : EventEmitter<boolean> = new EventEmitter<boolean>();

    constructor() { }

    ngOnInit() {
    }

    confirm() {
        this.deleteMsgVisible = true;
    }

    cancel() {
        this.deleteMsgVisible = false;
    }

    delete() {
        this.deleteMsgVisible = false;
        this.onDelete.emit(true);
    }
}