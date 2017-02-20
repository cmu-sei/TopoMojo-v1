import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild, CanLoad,
    Router, Route, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { AuthService } from './auth.service';

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {

    constructor(
        private authService: AuthService,
        private router: Router
        ){ }

    canLoad(route: Route): boolean {
        let url = `/${route.path}`;
        //console.log('AuthGuard#canLoad ' + url)
        return this.isAuthenticated(url);
    }

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ) : boolean {

        let url: string = state.url;
        return this.isAuthenticated(url);
    }

    canActivateChild(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot
        ): boolean {

        return this.canActivate(route, state);
    }

    isAuthenticated(url: string) : boolean {
        //console.log('AuthGuard#isAuthenticated()');

        if (this.authService.isAuthenticated(url)) {
            return true;
        }

        this.router.navigate(['/login']);
        return false;
    }
}