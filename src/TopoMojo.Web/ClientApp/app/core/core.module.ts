import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { CoreAuthGuard } from './auth-guard.service';
import { CoreAuthHttp } from './auth-http';
import { CoreAuthService } from './auth.service';
import { CoreRoutingModule } from './core-routing.module';
import { SharedModule } from '../shared/shared.module';

@NgModule({
    imports: [ SharedModule, HttpModule, CoreRoutingModule ],
    declarations: [ CoreRoutingModule.components ],
    providers: [ CoreAuthHttp, CoreAuthService, CoreAuthGuard ]
})
export class CoreModule { }