
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Observable';
import { GeneratedVmService } from "./gen/vm.service";
import { KeyValuePair,VirtualVm,VirtualVmAnswer,VirtualVmQuestion,VirtualVmStateEnum,VirtualVmTask } from "./gen/models";
import { SettingsService } from '../svc/settings.service';

@Injectable()
export class VmService extends GeneratedVmService {

    constructor(
       protected http: HttpClient,
       private settings: SettingsService
    ) { super(http); }

    public openConsole(id, name) {
        this.settings.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

}
