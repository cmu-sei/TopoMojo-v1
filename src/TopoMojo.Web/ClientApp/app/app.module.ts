import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
//import { APP_BASE_HREF } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { RouterModule , Router, PreloadAllModules, PreloadingStrategy} from '@angular/router';
// import { SignalRModule } from 'ng2-signalr';
//import { SignalRConfiguration } from 'ng2-signalr';
import { TranslateModule, TranslateLoader} from '@ngx-translate/core';
import { TranslateHttpLoader} from '@ngx-translate/http-loader';

// import { AuthService } from './svc/auth.service';
// import { AuthInterceptor } from './svc/http-auth-interceptor';
// import { AuthGuard } from './svc/auth-guard.service';
// import { AdminGuard } from './svc/admin-guard.service';
// import { NotificationService } from './svc/notification.service';
// import { createTranslateLoader, createSignalRConfig } from './svc/settings.service';
import { createTranslateLoader } from './svc/settings.service';
import { SvcModule } from './svc/svc.module';
import { AdminModule } from './ui/admin/admin.module';
import { ApiModule } from './api/api.module';
import { AuthModule } from './ui/auth/auth.module';
import { ChatModule } from './ui/chat/chat.module';
import { CoreModule } from './ui/core/core.module';
import { GamespaceModule } from './ui/gamespace/gamespace.module';
import { SharedModule } from './ui/shared/shared.module';
import { WorkspaceModule } from './ui/workspace/workspace.module';

import { AppComponent } from './app.component'

@NgModule({
    bootstrap: [ AppComponent ],
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        BrowserAnimationsModule,
        HttpClientModule,
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: (createTranslateLoader),
                deps: [HttpClient]
            }
        }),
        ApiModule,
        SvcModule,
        SharedModule,
        AdminModule,
        AuthModule,
        ChatModule,
        CoreModule,
        GamespaceModule,
        WorkspaceModule,
        //SignalRModule.forRoot(createSignalRConfig),
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'home/notfound' }
        ])
    ]
})
export class AppModule {
}
