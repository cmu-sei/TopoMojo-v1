
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from 'rxjs/Rx';
import { GeneratedService } from "./_service";
import { AccountsCredentials } from "./models";

@Injectable()
export class GeneratedAccountService extends GeneratedService {

    constructor(
       protected http: HttpClient
    ) { super(http); }

	public loginAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post<any>("/api/account/login", model);
	}
	public otpAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post<any>("/api/account/otp", model);
	}
	public tfaAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post<any>("/api/account/tfa", model);
	}
	public registerAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post<any>("/api/account/register", model);
	}
	public resetAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post<any>("/api/account/reset", model);
	}
	public confirmAccount(model: AccountsCredentials) : Observable<boolean> {
		return this.http.post<boolean>("/api/account/confirm", model);
	}
	public refreshAccount() : Observable<any> {
		return this.http.get<any>("/api/account/refresh");
	}

}

