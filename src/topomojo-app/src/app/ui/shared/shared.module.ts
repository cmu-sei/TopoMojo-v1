import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UntaggedStringPipe } from './directives/untagged.directive';
import { AgedDatePipe, ShortDatePipe } from './directives/ago.directive';
import { ErrorDivComponent } from './error-div/error-div.component';
import { ExpiringDialogComponent } from './expiring-dialog/expiring-dialog.component';
import { ConfirmButtonComponent } from './confirm-button/confirm-button.component';
import { SelectTableComponent } from './select-table/select-table.component';
import { FileUploaderComponent } from './file-uploader/file-uploader.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VmControllerComponent } from './vm-controller/vm-controller.component';
import { VmQuestionComponent } from './vm-question/vm-question.component';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

const mats = [
  MatIconModule,
  MatButtonModule,
  MatInputModule,
  MatTableModule,
  MatPaginatorModule,
  MatTooltipModule,
  MatProgressSpinnerModule,
  MatDialogModule,
  MatSlideToggleModule
];

const shared = [
  UntaggedStringPipe,
  AgedDatePipe,
  ShortDatePipe,
  ErrorDivComponent,
  ExpiringDialogComponent,
  ConfirmButtonComponent,
  SelectTableComponent,
  FileUploaderComponent,
  VmControllerComponent,
  VmQuestionComponent
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ...mats
  ],
  declarations: [...shared],
  exports: [
    CommonModule,
    ...shared
  ],
})
export class SharedModule { }
