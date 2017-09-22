
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedVmService } from "./gen/vm.service";
import { VirtualVm,VirtualVmStateEnum,VirtualVmQuestion,VirtualVmTask,KeyValuePair,VirtualVmAnswer } from "./gen/models";
import { ExternalNavService } from "../shared/external-nav.service";

@Injectable()
export class VmService extends GeneratedVmService {

    constructor(
       protected http: HttpClient,
       private nav : ExternalNavService
    ) { super(http); }

    openConsole(id: string, name: string) {
        this.nav.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }
}
