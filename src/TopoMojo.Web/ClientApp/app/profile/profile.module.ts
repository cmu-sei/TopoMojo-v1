import { NgModule, Optional, SkipSelf  } from '@angular/core';
import { HttpModule } from '@angular/http';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';
import { FormsModule } from '@angular/forms';
import { ProfileComponent } from './profile.component';

@NgModule({
    declarations: [
        ProfileComponent,
    ],
    imports: [
        SharedModule,
        RouterModule.forChild([
            { path: 'profile', component: ProfileComponent }
        ])
    ]
})
export class ProfileModule {
}