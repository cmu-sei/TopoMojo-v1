
import { Injectable } from "@angular/core";
//import { HttpClient } from "@angular/common/http";
import { AuthHttp } from "../auth/auth-http";
import { Observable } from 'rxjs/Rx';
import { UrlHelper } from "./url-helper";
import { AccountsCredentials } from "./api-models";

@Injectable()
export class AccountService {

    constructor(
        private http: AuthHttp
        //private http: HttpClient
    ) { }

	public loginAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post("/api/account/login", model);
	}
	public otpAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post("/api/account/otp", model);
	}
	public tfaAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post("/api/account/tfa", model);
	}
	public registerAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post("/api/account/register", model);
	}
	public resetAccount(model: AccountsCredentials) : Observable<any> {
		return this.http.post("/api/account/reset", model);
	}
	public confirmAccount(model: AccountsCredentials) : Observable<boolean> {
		return this.http.post("/api/account/confirm", model);
	}
	public refreshAccount() : Observable<any> {
		return this.http.get("/api/account/refresh");
	}

}
