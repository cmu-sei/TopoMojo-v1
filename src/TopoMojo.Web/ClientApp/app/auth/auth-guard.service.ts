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
            console.log("isAuth'd: " + a);
            if (a) return a;
            //this.authService.initiateLogin(state.url);
            this.authService.redirectUrl = state.url;
            this.router.navigate(["/login"]);
            //this.router.navigateByUrl("/login?url=" + state.url);
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