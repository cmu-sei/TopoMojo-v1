import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, Router, Route, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService, UserProfile } from './auth.service';

@Injectable()
export class AdminGuard implements CanActivate, CanActivateChild {

    private profile: UserProfile;

    constructor(
        private authService: AuthService,
        private router: Router
        ){
            this.authService.profile$.subscribe(
                (p : UserProfile) => {
                    this.profile = p;
                }
            )
        }

    canLoad(route: Route): boolean {
        return this.profile.isAdmin;
    }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ) : boolean {

        return this.profile.isAdmin;
    }

    canActivateChild(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ): boolean {

        return this.profile.isAdmin;
    }

}