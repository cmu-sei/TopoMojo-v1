import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuardService } from '../auth/auth-guard.service';
import { ConnectionResolver } from '../shared/connection.resolver';
import { GamespaceComponent } from './gamespace.component';
import { PlayerComponent } from './player.component';
import { ControlBarComponent } from './controlbar.component';
import { ConsoleComponent } from './console.component';
import { GamespaceEnlistComponent } from './enlist.component';

const routes: Routes = [
    {
        path: 'mojo',
        canActivate: [AuthGuardService],
        children: [
            {
                path: '',
                //canActivateChild: [ AuthGuardService ],
                children: [
                    {
                        path: ':id',
                        component: PlayerComponent,
                        resolve: { connection : ConnectionResolver }
                     },
                    {
                        path: 'enlist',
                        children: [
                            { path: ':code', component: GamespaceEnlistComponent }
                        ]
                    },
                    { path: '', component: GamespaceComponent }
                ]
            }
        ]
    }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class GamespaceRoutingModule {
    static components = [
        GamespaceComponent,
        PlayerComponent,
        ControlBarComponent,
        ConsoleComponent,
        GamespaceEnlistComponent
     ]
}