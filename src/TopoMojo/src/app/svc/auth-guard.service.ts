import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {

    constructor(
        private authSvc: AuthService,
        private router: Router
    ) {
    }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ) : Promise<boolean> {

        return this.authSvc.isAuthenticated().then(a => {
            if (a) return a;
            // let hint = state.url.split('?').pop().match(/auth-hint=([^&]+)/);
            // if (hint) {
            //     this.authSvc.externalLogin(state.url);
            // }
            // else {
            // }
            this.authSvc.redirectUrl = state.url;
            this.router.navigate(["/auth/login"]);
            return false;
        });
    }

    canActivateChild(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ): Promise<boolean> {

        return this.canActivate(route, state);
    }

}