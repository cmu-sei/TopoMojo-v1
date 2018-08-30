import { Injectable } from '@angular/core';
import { CanActivate, CanActivateChild } from '@angular/router';
import { UserService } from './user.service';
import { Profile } from '../api/gen/models';
import { debounceTime } from 'rxjs/operators';

@Injectable()
export class AdminGuard implements CanActivate, CanActivateChild {

    private profile: Profile = {};

    constructor(
        private userSvc: UserService
    ) {

        // TODO: this fails if navigating directly to <host>/admin because profile load will race
        this.userSvc.profile$.subscribe(
            (p: Profile) => {
                this.profile = p;
            }
        );
    }

    canLoad(): boolean {
        return this.profile.isAdmin;
    }

    canActivate(
                ): boolean {

        return this.profile.isAdmin;
    }

    canActivateChild(
                ): boolean {

        return this.profile.isAdmin;
    }

}
