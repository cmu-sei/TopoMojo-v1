
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedService } from "./_service";
import { KeyValuePair,VirtualVm,VirtualVmAnswer,VirtualVmQuestion,VirtualVmStateEnum,VirtualVmTask } from "./models";

@Injectable()
export class GeneratedVmService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public ticketVm(id: string) : Observable<any> {
		return this.http.get<any>("/api/vm/" + id + "/ticket");
	}
	public loadVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/load");
	}
	public resolveVm(id: number) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/resolve");
	}
	public startVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/start");
	}
	public stopVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/stop");
	}
	public saveVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/save");
	}
	public revertVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/revert");
	}
	public deleteVm(id: string) : Observable<VirtualVm> {
		return this.http.delete<VirtualVm>("/api/vm/" + id + "/delete");
	}
	public changeVm(id: string, change: KeyValuePair) : Observable<VirtualVm> {
		return this.http.post<VirtualVm>("/api/vm/" + id + "/change", change);
	}
	public deployVm(id: number) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/deploy");
	}
	public initVm(id: number) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/init");
	}
	public answerVm(id: string, answer: VirtualVmAnswer) : Observable<VirtualVm> {
		return this.http.post<VirtualVm>("/api/vm/" + id + "/answer", answer);
	}
	public isosVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/isos");
	}
	public netsVm(id: string) : Observable<VirtualVm> {
		return this.http.get<VirtualVm>("/api/vm/" + id + "/nets");
	}

}

