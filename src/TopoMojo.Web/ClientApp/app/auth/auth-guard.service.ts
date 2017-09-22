import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AuthService } from './auth.service';

@Injectable()
export class AuthGuardService implements CanActivate, CanActivateChild {

    constructor(
        private authService: AuthService,
        private router: Router
    ) { }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ) : Promise<boolean> {
        return this.authService.isAuthenticated().then(a => {
            //console.log("isAuth'd: " + a);
            if (a) return a;
            let hint = state.url.split('?').pop().match(/auth-hint=([^&]+)/);
            if (hint) {
                this.authService.externalLogin(state.url);
            }
            else {
                this.authService.redirectUrl = state.url;
                this.router.navigate(["/auth/login"]);
            }
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