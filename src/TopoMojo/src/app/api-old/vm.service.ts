
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { GeneratedVmService } from "./gen/vm.service";
import { LayoutService } from "../svc/layout.service";

@Injectable()
export class VmService extends GeneratedVmService {

    constructor(
       protected http: HttpClient,
       private layoutSvc: LayoutService
    ) { super(http); }

    public openConsole(id, name) {
        this.layoutSvc.showTab('/console/' + id + "/" + name.match(/[^#]*/)[0]);
    }

}
