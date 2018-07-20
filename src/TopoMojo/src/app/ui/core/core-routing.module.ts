import { NgModule } from '@angular/core';
import { RouterModule, Routes, ActivatedRoute } from '@angular/router';
import { CoreComponent } from './core.component';
import { HomeComponent } from './home.component';
import { NavbarComponent } from './navbar.component';
import { NotFoundComponent } from './notfound.component';
import { AboutPanelComponent } from './about-panel.component';
import { NotAllowedComponent } from './notallowed.component';

const routes: Routes = [
    {
        path: 'home',
        component: CoreComponent,
        children: [
            { path: 'about', component: AboutPanelComponent },
            { path: 'notfound', component: NotFoundComponent },
            { path: 'notallowed', component: NotAllowedComponent },
            { path: '', component: HomeComponent },
        ]
    }
];

@NgModule({
    imports: [ RouterModule.forChild(routes) ],
    exports: [ RouterModule ]
})
export class CoreRoutingModule {
    static components = [
        CoreComponent,
        HomeComponent,
        NavbarComponent,
        NotFoundComponent,
        NotAllowedComponent,
        AboutPanelComponent
    ];
}
