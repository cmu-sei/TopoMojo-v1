import { NgModule } from '@angular/core';
import { RouterModule , Router, PreloadAllModules, PreloadingStrategy} from '@angular/router';
//import { FormsModule } from '@angular/forms';
import { UniversalModule } from 'angular2-universal';
import { CoreModule } from './core/core.module';
import { TopoModule } from './topo/topo.module';
import { AdminModule } from './admin/admin.module';
import { AppComponent } from './app.component'

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent
    ],
    imports: [
        UniversalModule, // Must be first import. This automatically imports BrowserModule, HttpModule, and JsonpModule too.
        CoreModule,
        TopoModule,
        AdminModule,
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'notfound' }
        ])
    ]
})
export class AppModule {
    // Diagnostic only: inspect router configuration
//   constructor(router: Router) {
//     console.log('Routes: ', JSON.stringify(router.config, undefined, 2));
//   }
}
