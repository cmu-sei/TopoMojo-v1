import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '../../svc/auth-guard.service';
//import { ConnectionResolver } from '../../svc/connection.resolver';
import { GamespaceComponent } from './gamespace.component';
import { PlayerComponent } from './player.component';
import { GamespaceEnlistComponent } from './enlist.component';

const routes: Routes = [
    {
        path: 'mojo',
        canActivate: [AuthGuard],
        children: [
            {
                path: '',
                children: [
                    {
                        path: ':id',
                        component: PlayerComponent
                        //resolve: { connection : ConnectionResolver }
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
        GamespaceEnlistComponent
     ]
}