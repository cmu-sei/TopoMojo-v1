import { NgModule, Optional, SkipSelf  } from '@angular/core';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from './auth.service';
import { AuthHttp } from './auth-http';
import { AuthGuardService } from './auth-guard.service';
import { AuthPendingComponent } from './auth-pending.component';
import { AuthFailedComponent } from './auth-failed.component';
import { AuthTestComponent } from './auth-test.component';
import { LoginComponent } from './login.component';
import { SettingsService } from './settings.service';
import { AuthComponent } from './auth.component';

@NgModule({
    declarations: [
        AuthComponent,
        AuthPendingComponent,
        AuthFailedComponent,
        AuthTestComponent,
        LoginComponent
    ],
    providers: [
        AuthService,
        AuthGuardService,
        AuthHttp,
        SettingsService
    ],
    exports: [
        HttpModule
    ],
    imports: [
        HttpModule,
        CommonModule,
        FormsModule,
        RouterModule.forChild([
            {
                path: 'auth',
                component: AuthComponent,
                children: [
                    { path: 'nope', component: AuthFailedComponent },
                    { path: 'test', component: AuthTestComponent },
                    { path: 'login', component: LoginComponent },
                    { path: 'oidc', component: AuthPendingComponent },
                    { path: '', redirectTo: 'login', pathMatch: 'full'}
                ]
            }
        ])
    ]
})
export class AuthModule {
    constructor (@Optional() @SkipSelf() parentModule: AuthModule) {
        if (parentModule) {
            throw new Error(
            'AuthModule is already loaded. Import it in the AppModule only');
        }
    }
}