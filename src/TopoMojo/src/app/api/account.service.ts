
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedAccountService } from "./gen/account.service";
import { AccountsCredentials } from "./gen/models";

@Injectable()
export class AccountService extends GeneratedAccountService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

    public submit(method: string, creds : AccountsCredentials) : Observable<Object> {
        return this.http.post<Object>("/api/account/" + method, creds);
    }
}
