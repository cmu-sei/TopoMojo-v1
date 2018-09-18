import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminComponent } from './admin.component';
import { RouterModule } from '@angular/router';
import { AdminGuard } from '../../svc/admin-guard.service';
import { MatTabsModule } from '@angular/material/tabs';
import { WorkspacesComponent } from './workspaces/workspaces.component';
import { AuthGuard } from '../../svc/auth-guard.service';
import { GamespacesComponent } from './gamespaces/gamespaces.component';
import { TemplatesComponent } from './templates/templates.component';
import { MachinesComponent } from './machines/machines.component';
import { PeopleComponent } from './people/people.component';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { SharedModule } from '../shared/shared.module';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { TemplateDetailFormComponent } from './template-detail-form/template-detail-form.component';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ProfileSettingsComponent } from './profile-settings/profile-settings.component';
import { TemplateCreatorComponent } from './template-creator/template-creator.component';
import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    SharedModule,
    MatTabsModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatDividerModule,
    MatInputModule,
    MatSlideToggleModule,
    MatTooltipModule,
    RouterModule.forChild([
      { path: '', component: AdminComponent, canActivate: [AdminGuard, AuthGuard], children: [
        {path: '', children: [
          { path: 'dash', component: DashboardComponent },
          { path: 'topo', component: WorkspacesComponent },
          { path: 'mojo', component: GamespacesComponent },
          { path: 'tempo', component: TemplatesComponent },
          { path: 'vms', component: MachinesComponent },
          { path: 'people', component: PeopleComponent },
          // { path: '**', component: AdminComponent }
        ]}
      ]}
    ])
  ],
  declarations: [AdminComponent, WorkspacesComponent, GamespacesComponent, TemplatesComponent, MachinesComponent, PeopleComponent, TemplateDetailFormComponent, ProfileSettingsComponent, TemplateCreatorComponent, DashboardComponent]
})
export class AdminModule { }
