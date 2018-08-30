
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from '../api-settings';
import { GeneratedService } from './_service';
import { DisplayInfo, KeyValuePair, Vm, VmAnswer, VmOperation, VmOperationTypeEnum, VmOptions, VmQuestion, VmStateEnum, VmTask } from './models';

@Injectable()
export class GeneratedVmService extends GeneratedService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }

    public getVmTicket(id: string): Observable<DisplayInfo> {
        return this.http.get<DisplayInfo>(this.api.url + '/api/vm/' + id + '/ticket');
    }
    public getVms(tag: string): Observable<Array<Vm>> {
        return this.http.get<Array<Vm>>(this.api.url + '/api/vms' + this.paramify({tag: tag}));
    }
    public getVm(id: string): Observable<Vm> {
        return this.http.get<Vm>(this.api.url + '/api/vm/' + id);
    }
    public deleteVm(id: string): Observable<Vm> {
        return this.http.delete<Vm>(this.api.url + '/api/vm/' + id);
    }
    public postVmAction(op: VmOperation): Observable<Vm> {
        return this.http.post<Vm>(this.api.url + '/api/vm/action', op);
    }
    public postVmChange(id: string, change: KeyValuePair): Observable<Vm> {
        return this.http.post<Vm>(this.api.url + '/api/vm/' + id + '/change', change);
    }
    public postVmAnswer(id: string, answer: VmAnswer): Observable<Vm> {
        return this.http.post<Vm>(this.api.url + '/api/vm/' + id + '/answer', answer);
    }
    public getVmIsos(id: string): Observable<VmOptions> {
        return this.http.get<VmOptions>(this.api.url + '/api/vm/' + id + '/isos');
    }
    public getVmNets(id: string): Observable<VmOptions> {
        return this.http.get<VmOptions>(this.api.url + '/api/vm/' + id + '/nets');
    }
    public getTemplateVm(id: number): Observable<Vm> {
        return this.http.get<Vm>(this.api.url + '/api/template/' + id + '/vm');
    }
    public postTemplateDeploy(id: number): Observable<Vm> {
        return this.http.post<Vm>(this.api.url + '/api/template/' + id + '/deploy', {});
    }
    public postTemplateDisks(id: number): Observable<Vm> {
        return this.http.post<Vm>(this.api.url + '/api/template/' + id + '/disks', {});
    }
    public postHostReload(host: string): Observable<any> {
        return this.http.post<any>(this.api.url + '/api/host/' + host + '/reload', {});
    }

}
