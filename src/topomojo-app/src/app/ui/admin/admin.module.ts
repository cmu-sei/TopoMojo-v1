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

@NgModule({
  imports: [
    CommonModule,
    MatTabsModule,
    RouterModule.forChild([
      { path: 'admin', component: AdminComponent, canActivate: [AdminGuard, AuthGuard], children: [
        {path: '', children: [
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
  declarations: [AdminComponent, WorkspacesComponent, GamespacesComponent, TemplatesComponent, MachinesComponent, PeopleComponent]
})
export class AdminModule { }
