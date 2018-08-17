import { NgModule } from '@angular/core';
import { LayoutModule } from '@angular/cdk/layout';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';

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

import { NavbarComponent } from './navbar/navbar.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { WorkspaceComponent } from './workspace/workspace.component';
import { WorkspaceLobbyComponent } from './workspace-lobby/workspace-lobby.component';
import { WelcomeComponent } from './welcome/welcome.component';
import { GamespaceLobbyComponent } from './gamespace-lobby/gamespace-lobby.component';
import { WorkspaceCreatorComponent } from './workspace-creator/workspace-creator.component';
import { WorkspaceSummaryComponent } from './workspace-summary/workspace-summary.component';
import { LoginComponent } from './login/login.component';
import { OidcSilentComponent } from './oidc-silent/oidc-silent.component';
import { WorkspaceSettingsComponent } from './workspace-settings/workspace-settings.component';
import { TemplateSelectorComponent } from './template-selector/template-selector.component';
import { TemplateComponent } from './template/template.component';
import { ConsoleComponent } from './console/console.component';
import { TemplateSettingsComponent } from './template-settings/template-settings.component';
import { DocumentEditorComponent } from './document-editor/document-editor.component';
import { GamespaceComponent } from './gamespace/gamespace.component';
import { LogoutComponent } from './logout/logout.component';
import { ChatPanelComponent } from './chat-panel/chat-panel.component';
import { ChatMessageComponent } from './chat-message/chat-message.component';
import { EnlistComponent } from './enlist/enlist.component';
import { DocumentImageManagerComponent } from './document-image-manager/document-image-manager.component';
import { SharedModule } from '../shared/shared.module';

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
    SharedModule,
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
            { path: '', component: DocumentImageManagerComponent, outlet: 'sidenav' },
            { path: '', component: DocumentEditorComponent }
        ]},
        { path: '', component: WorkspaceLobbyComponent }
      ]},
      { path: 'mojo', children: [
        { path: ':id', children: [
          { path: 'live', component: GamespaceComponent, canActivate: [AuthGuard] },
          { path: '', component: ChatPanelComponent, outlet: 'sidenav' },
          { path: '', component: GamespaceComponent }
        ]},
        { path: 'enlist/:code', component: EnlistComponent, canActivate: [AuthGuard] },
        { path: '', component: GamespaceLobbyComponent }
      ]},
      { path: 'console/:id/:name', component: ConsoleComponent, canActivate: [AuthGuard] }
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
    OidcSilentComponent,
    WorkspaceSettingsComponent,
    TemplateSelectorComponent,
    TemplateComponent,
    ConsoleComponent,
    TemplateSettingsComponent,
    DocumentEditorComponent,
    GamespaceComponent,
    LogoutComponent,
    ChatPanelComponent,
    ChatMessageComponent,
    EnlistComponent,
    DocumentImageManagerComponent,
  ],
  exports: [
    WelcomeComponent,
    NavbarComponent,
    PageNotFoundComponent,
    ChatPanelComponent,
    DocumentImageManagerComponent
  ]
})
export class CoreModule { }
