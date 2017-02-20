import { NgModule } from '@angular/core';
import { CommonModule }       from '@angular/common';
import { FormsModule }        from '@angular/forms';
import { SearchBarComponent } from './searchbar.component';
import { EntityListComponent } from './entity-list.component';
import { InlineEditorComponent} from './inline-editor.component';
import { Tooltip } from './tooltip.directive';
import { TooltipContent } from './tooltip.component';
import { Collapser } from './collapser.component';
import { InlineHelpComponent } from './inline-help.component';

export * from "./tooltip.directive";
export * from "./tooltip.component";


@NgModule({
    imports: [
        CommonModule,
        FormsModule
    ],
    declarations: [
        SearchBarComponent,
        EntityListComponent,
        InlineEditorComponent,
        InlineHelpComponent,
        Tooltip,
        TooltipContent,
        Collapser
    ],
    exports: [
        SearchBarComponent,
        EntityListComponent,
        InlineEditorComponent,
        InlineHelpComponent,
        Tooltip,
        TooltipContent,
        Collapser,
        CommonModule,
        FormsModule
    ],
    entryComponents: [
        TooltipContent
    ]
})
export class SharedModule {}