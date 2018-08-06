import {Injectable, Injector} from '@angular/core';
import {HttpEvent, HttpInterceptor, HttpHandler, HttpRequest} from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(
        private injector : Injector
    ) {}
    private auth: AuthService

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (req.url.match(/settings.*\.json$/))
            return next.handle(req);

        this.auth = this.injector.get(AuthService);
        const authHeader = this.auth.auth_header();
        const authReq = req.clone({setHeaders: {Authorization: authHeader}});
        return next.handle(authReq);
    }
}