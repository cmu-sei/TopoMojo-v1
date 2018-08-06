
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedVmService } from './gen/vm.service';
import { KeyValuePair, Vm, VmAnswer, VmQuestion, VmStateEnum, VmTask } from './gen/models';
import { LayoutService } from '../svc/layout.service';

@Injectable()
export class VmService extends GeneratedVmService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings,
       private layoutSvc: LayoutService
    ) { super(http, api); }

    public openConsole(id, name) {
        this.layoutSvc.showTab('/console/' + id + '/' + name.match(/[^#]*/)[0]);
    }
}
