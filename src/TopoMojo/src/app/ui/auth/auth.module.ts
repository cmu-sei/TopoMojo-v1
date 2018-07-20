import { NgModule  } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { AuthComponent } from './auth.component';
import { AuthPendingComponent } from './auth-pending.component';
import { AuthFailedComponent } from './auth-failed.component';
import { AuthTestComponent } from './auth-test.component';
import { LoginComponent } from './login.component';
import { ProfileEditorComponent } from './profile-editor.component';
import { AuthSilentComponent } from './auth-silent.component';

@NgModule({
    declarations: [
        AuthComponent,
        AuthPendingComponent,
        AuthSilentComponent,
        AuthFailedComponent,
        AuthTestComponent,
        LoginComponent,
        ProfileEditorComponent
    ],
    imports: [
        SharedModule,
        RouterModule.forChild([
            {
                path: 'auth',
                component: AuthComponent,
                children: [
                    { path: 'nope', component: AuthFailedComponent },
                    { path: 'test', component: AuthTestComponent },
                    { path: 'login', component: LoginComponent },
                    { path: 'oidc', component: AuthPendingComponent },
                    { path: 'oidc-silent', component: AuthSilentComponent },
                    { path: 'profile', component: ProfileEditorComponent },
                    { path: '', redirectTo: 'login', pathMatch: 'full'}
                ]
            }
        ])
    ]
})
export class AuthModule {

}
