
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedAdminService } from "./gen/admin.service";
import {  } from './gen/models';

@Injectable()
export class AdminService extends GeneratedAdminService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
