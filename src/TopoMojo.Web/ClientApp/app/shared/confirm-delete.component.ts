import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
    // moduleId: module.id,
    selector: 'confirm-delete',
    template: `
        <button *ngIf="!deleteMsgVisible && !asLink"
            class="btn btn-danger"
            [tooltip]="'DELETE'|translate|lowercase"
            (click)="confirm()">
            <i class="fa fa-trash"></i> <span translate>{{ label }}</span>
        </button>
        <button *ngIf="!deleteMsgVisible && asLink"
            class="btn btn-link"
            [tooltip]="'DELETE'|translate|lowercase"
            (click)="confirm()">
            <i class="fa fa-trash text text-danger"></i> <span class="text text-danger" translate>{{ label }}</span>
        </button>
        <p *ngIf="deleteMsgVisible" class="alert alert-danger">
            {{ prompt | translate }}
            <button (click)="delete()" class="btn btn-danger btn-sm">
                <i class="fa fa-trash"></i> <span translate>DELETE</span></button>
            <button (click)="cancel()" class="btn btn-danger btn-sm">
                <i class="fa fa-remove"></i> <span translate>CANCEL</span></button>
        </p>
    `
})
export class ConfirmDeleteComponent implements OnInit {

    deleteMsgVisible: boolean;
    @Input() prompt: string = "CONFIRM";
    @Input() label: string = '';
    @Input() asLink: boolean;
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