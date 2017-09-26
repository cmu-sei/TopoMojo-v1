import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, CanLoad,
    Router, Route, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { AuthService } from './auth.service';

@Injectable()
export class AdminGuard implements CanActivate, CanActivateChild {

    constructor(
        private authService: AuthService,
        private router: Router
        ){ }

    canLoad(route: Route): boolean {
        let url = `/${route.path}`;
        //console.log('AdminGuard#canLoad ' + url)
        return this.isAdmin(url);
    }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ) : boolean {

        let url: string = state.url;
        return this.isAdmin(url);
    }

    canActivateChild(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ): boolean {

        return this.canActivate(route, state);
    }

    isAdmin(url: string) : boolean {
        //console.log('AdminGuard#isAuthenticated()');

        if (this.authService.isAdmin()) {
            return true;
        }

        this.router.navigate(['notallowed']);
        return false;
    }
}