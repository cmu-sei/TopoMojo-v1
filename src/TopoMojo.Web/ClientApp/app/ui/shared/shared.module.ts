import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { SearchBarComponent } from './searchbar.component';
import { EntityListComponent } from './entity-list.component';
import { InlineEditorComponent} from './inline-editor.component';
import { Tooltip } from './tooltip.directive';
import { TooltipContent } from './tooltip.component';
import { InlineHelpComponent } from './inline-help.component';
import { UntaggedStringPipe, FormatErrorPipe, AgedDatePipe } from './pipes';
import { ConfirmDeleteComponent } from './confirm-delete.component';
import { NotificationPanelComponent }  from './notification-panel.component';
import { ErrorDivComponent} from './error-div.component';
export * from "./tooltip.directive";
export * from "./tooltip.component";


@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        TranslateModule
    ],
    declarations: [
        SearchBarComponent,
        EntityListComponent,
        InlineEditorComponent,
        InlineHelpComponent,
        Tooltip,
        TooltipContent,
        UntaggedStringPipe,
        FormatErrorPipe,
        AgedDatePipe,
        ConfirmDeleteComponent,
        NotificationPanelComponent,
        ErrorDivComponent
    ],
    exports: [
        CommonModule,
        FormsModule,
        TranslateModule,
        SearchBarComponent,
        EntityListComponent,
        InlineEditorComponent,
        InlineHelpComponent,
        Tooltip,
        TooltipContent,
        UntaggedStringPipe,
        FormatErrorPipe,
        AgedDatePipe,
        ConfirmDeleteComponent,
        NotificationPanelComponent,
        ErrorDivComponent
    ],
    providers: [

    ],
    entryComponents: [
        TooltipContent
    ]
})
export class SharedModule {}