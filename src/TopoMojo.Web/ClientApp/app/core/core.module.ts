import { NgModule } from '@angular/core';
import { SharedModule } from '../shared/shared.module';
import { AuthService } from './auth.service';
import { AuthGuard } from './auth-guard.service';
import { CoreRoutingModule } from './core-routing.module';
import { AuthHttp } from './auth-http';

@NgModule({
    imports: [ SharedModule, CoreRoutingModule ],
    declarations: [ CoreRoutingModule.components ],
    providers: [ AuthHttp, AuthService, AuthGuard ]
})
export class CoreModule { }