import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LayoutModule } from '@angular/cdk/layout';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatListModule } from '@angular/material/list';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTreeModule } from '@angular/material/tree';
import { MatButtonModule } from '@angular/material/button';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatBadgeModule } from '@angular/material/badge';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatBottomSheetModule } from '@angular/material/bottom-sheet';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';

import { NavbarComponent } from './comps/navbar/navbar.component';
import { PageNotFoundComponent } from './pages/page-not-found/page-not-found.component';
import { WorkspaceComponent } from './pages/workspace/workspace.component';
import { WorkspaceLobbyComponent } from './pages/workspace-lobby/workspace-lobby.component';
import { WelcomeComponent } from './pages/welcome/welcome.component';
import { GamespaceLobbyComponent } from './pages/gamespace-lobby/gamespace-lobby.component';
import { WorkspaceCreatorComponent } from './comps/workspace-creator/workspace-creator.component';
import { WorkspaceSummaryComponent } from './comps/workspace-summary/workspace-summary.component';
import { AuthGuard } from '../svc/auth-guard.service';
import { LoginComponent } from './pages/login/login.component';
import { ExpiringDialogComponent } from './comps/expiring-dialog/expiring-dialog.component';
import { OidcSilentComponent } from './pages/oidc-silent/oidc-silent.component';
import { WorkspaceSettingsComponent } from './comps/workspace-settings/workspace-settings.component';
import { TemplateSelectorComponent } from './comps/template-selector/template-selector.component';
import { TemplateComponent } from './comps/template/template.component';
import { VmControllerComponent } from './comps/vm-controller/vm-controller.component';
import { ErrorDivComponent } from './comps/error-div/error-div.component';
import { ConsoleComponent } from './pages/console/console.component';
import { TemplateSettingsComponent } from './comps/template-settings/template-settings.component';
import { ConfirmButtonComponent } from './comps/confirm-button/confirm-button.component';
import { SelectTableComponent } from './comps/select-table/select-table.component';
import { FileUploaderComponent } from './comps/file-uploader/file-uploader.component';
import { DocumentEditorComponent } from './pages/document-editor/document-editor.component';
import { GamespaceComponent } from './pages/gamespace/gamespace.component';
import { UntaggedStringPipe } from './directives/untagged.directive';
import { LogoutComponent } from './pages/logout/logout.component';
import { ChatPanelComponent } from './comps/chat-panel/chat-panel.component';
import { ChatMessageComponent } from './comps/chat-message/chat-message.component';
import { EnlistComponent } from './pages/enlist/enlist.component';
import { DocumentImageManagerComponent } from './comps/document-image-manager/document-image-manager.component';
import { VmQuestionComponent } from './comps/vm-question/vm-question.component';

const mats = [
  MatFormFieldModule,
  MatInputModule,
  MatSlideToggleModule,
  MatMenuModule,
  MatSidenavModule,
  MatToolbarModule,
  MatDividerModule,
  MatExpansionModule,
  MatListModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatBadgeModule,
  MatChipsModule,
  MatIconModule,
  MatProgressSpinnerModule,
  MatDialogModule,
  MatTooltipModule,
  MatPaginatorModule,
  MatTableModule
];

@NgModule({
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    CommonModule,
    FormsModule,
    LayoutModule,
    ...mats,
    RouterModule.forChild([
      { path: 'login', component: LoginComponent },
      { path: 'logout', component: LogoutComponent },
      { path: 'oidc-silent', component: OidcSilentComponent },
      { path: 'invitation/:code', component: EnlistComponent, canActivate: [AuthGuard] },
      { path: 'topo', children: [
        { path: ':id', canActivate: [AuthGuard], children: [
          { path: '', component: ChatPanelComponent, outlet: 'sidenav' },
          { path: '', component: WorkspaceComponent }
        ]},
        { path: 'doc/:key', children: [
          // { path: '', children: [
            { path: '', component: DocumentImageManagerComponent, outlet: 'sidenav' },
            { path: '', component: DocumentEditorComponent }
          // ]},
        ]},
        // { path: ':id', component: ChatPanelComponent, outlet: 'sidenav' },
        { path: '', component: WorkspaceLobbyComponent }
      ]},
      { path: 'mojo', children: [
        { path: ':id', children: [
          { path: 'live', component: GamespaceComponent, canActivate: [AuthGuard] },
          { path: '', component: ChatPanelComponent, outlet: 'sidenav' },
          { path: '', component: GamespaceComponent }
        ]},
        // { path: ':id', component: ChatPanelComponent, outlet: 'sidenav' },
        { path: 'enlist/:code', component: EnlistComponent, canActivate: [AuthGuard] },
        { path: '', component: GamespaceLobbyComponent }
      ]},
      { path: 'console/:id/:name', component: ConsoleComponent, canActivate: [AuthGuard] },
      // { path: 'images/:key', component: DocumentImageManagerComponent, outlet: 'sidenav' }

    ])
  ],
  declarations: [
    WelcomeComponent,
    NavbarComponent,
    PageNotFoundComponent,
    WorkspaceLobbyComponent,
    WorkspaceComponent,
    GamespaceLobbyComponent,
    GamespaceComponent,
    WorkspaceCreatorComponent,
    WorkspaceSummaryComponent,
    LoginComponent,
    ExpiringDialogComponent,
    OidcSilentComponent,
    WorkspaceSettingsComponent,
    TemplateSelectorComponent,
    TemplateComponent,
    VmControllerComponent,
    ErrorDivComponent,
    ConsoleComponent,
    TemplateSettingsComponent,
    ConfirmButtonComponent,
    SelectTableComponent,
    FileUploaderComponent,
    DocumentEditorComponent,
    GamespaceComponent,
    UntaggedStringPipe,
    LogoutComponent,
    ChatPanelComponent,
    ChatMessageComponent,
    EnlistComponent,
    DocumentImageManagerComponent,
    VmQuestionComponent
  ],
  exports: [
    WelcomeComponent,
    NavbarComponent,
    PageNotFoundComponent,
    MatSidenavModule,
    ChatPanelComponent,
    DocumentImageManagerComponent
  ]
})
export class UiModule { }
