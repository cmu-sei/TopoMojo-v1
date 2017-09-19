
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { VirtualVm,VirtualVmStateEnum,VirtualVmQuestion,VirtualVmTask,KeyValuePair,VirtualVmAnswer } from "./api-models";

@Injectable()
export class VmService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public ticketVm(id: string) : Observable<any> {
		return this.http.get("/api/vm/" + id + "/ticket");
	}
	public loadVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/load");
	}
	public resolveVm(id: number) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/resolve");
	}
	public startVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/start");
	}
	public stopVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/stop");
	}
	public saveVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/save");
	}
	public revertVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/revert");
	}
	public deleteVm(id: string) : Observable<VirtualVm> {
		return this.http.delete("/api/vm/" + id + "/delete");
	}
	public changeVm(id: string, change: KeyValuePair) : Observable<VirtualVm> {
		return this.http.post("/api/vm/" + id + "/change", change);
	}
	public deployVm(id: number) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/deploy");
	}
	public initVm(id: number) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/init");
	}
	public answerVm(id: string, answer: VirtualVmAnswer) : Observable<VirtualVm> {
		return this.http.post("/api/vm/" + id + "/answer", answer);
	}
	public isosVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/isos");
	}
	public netsVm(id: string) : Observable<VirtualVm> {
		return this.http.get("/api/vm/" + id + "/nets");
	}

}
