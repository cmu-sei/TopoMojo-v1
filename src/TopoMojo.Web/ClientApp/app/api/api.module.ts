import { AccountService } from "./account.service";
import { ConsoleService } from "./console.service";
import { DocumentService } from "./document.service";
import { FileService } from "./file.service";
import { GamespaceService } from "./gamespace.service";
import { ProfileService } from "./profile.service";
import { TemplateService } from "./template.service";
import { TopologyService } from "./topology.service";
import { VmService } from "./vm.service";
import { CustomService } from "./custom.service";

import { NgModule } from '@angular/core';

@NgModule({
    providers: [
        AccountService,
		ConsoleService,
		DocumentService,
		FileService,
		GamespaceService,
		ProfileService,
		TemplateService,
		TopologyService,
		VmService,
		CustomService
    ]
})
export class ApiModule { }
