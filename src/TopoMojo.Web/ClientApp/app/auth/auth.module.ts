import { NgModule, Optional, SkipSelf  } from '@angular/core';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';
import { AuthHttp } from './auth-http';
import { AuthGuardService } from './auth-guard.service';
import { AuthPendingComponent } from './auth-pending.component';
import { AuthFailedComponent } from './auth-failed.component';
import { AuthTestComponent } from './auth-test.component';

@NgModule({
    declarations: [
        AuthPendingComponent,
        AuthFailedComponent,
        AuthTestComponent
    ],
    providers: [
        AuthService,
        AuthGuardService,
        AuthHttp
    ],
    exports: [
        HttpModule
    ],
    imports: [
        HttpModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'auth', component: AuthPendingComponent },
            { path: 'nope', component: AuthFailedComponent },
            { path: 'authtest', component: AuthTestComponent }
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