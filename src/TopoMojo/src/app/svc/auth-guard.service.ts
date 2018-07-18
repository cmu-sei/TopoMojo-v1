import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { AuthService, UserProfile } from './auth.service';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {

    //profile: UserProfile;
    constructor(
        private authSvc: AuthService,
        private router: Router
    ) {
        // this.authSvc.profile$.subscribe(
        //     (p: UserProfile) => {
        //         this.profile = p;
        //     }
        // )
    }

    // canActivate(
    //     route: ActivatedRouteSnapshot,
    //     state: RouterStateSnapshot
    // ) : boolean {
    //     if (this.profile.isAdmin)
    //         return true;

    //     this.authSvc.redirectUrl = state.url;
    //     this.router.navigate(["/auth/login"]);
    //     return false;
    // }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
    ) : Promise<boolean> {

        return this.authSvc.isAuthenticated().then(a => {
            console.log("isAuth'd: " + a);
            if (a) return a;
            let hint = state.url.split('?').pop().match(/auth-hint=([^&]+)/);
            if (hint) {
                this.authSvc.externalLogin(state.url);
            }
            else {
                this.authSvc.redirectUrl = state.url;
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