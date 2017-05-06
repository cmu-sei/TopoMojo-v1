import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { AuthGuard } from './auth-guard.service';
import { AuthHttp } from './auth-http';
import { AuthService } from './auth.service';
import { CoreRoutingModule } from './core-routing.module';
import { SharedModule } from '../shared/shared.module';

@NgModule({
    imports: [ SharedModule, HttpModule, CoreRoutingModule ],
    declarations: [ CoreRoutingModule.components ],
    providers: [ AuthHttp, AuthService, AuthGuard ]
})
export class CoreModule { }