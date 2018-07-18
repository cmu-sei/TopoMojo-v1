import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { RouterModule} from '@angular/router';
import { TranslateModule, TranslateLoader} from '@ngx-translate/core';
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
        RouterModule.forRoot([
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: '**', redirectTo: 'home/notfound' }
        ])
    ]
})
export class AppModule {
}
