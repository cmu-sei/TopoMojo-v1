
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedVmService } from './gen/vm.service';
import { KeyValuePair, Vm, VmAnswer, VmQuestion, VmStateEnum, VmTask } from './gen/models';
import { SettingsService } from '../svc/settings.service';

@Injectable()
export class VmService extends GeneratedVmService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings,
       private settingSvc: SettingsService
    ) { super(http, api); }

    public openConsole(id, name) {
        this.settingSvc.showTab('/console/' + id + '/' + name.match(/[^#]*/)[0]);
    }
}
