import { Injectable } from '@angular/core';
import { AuthService, AuthTokenState } from './auth.service';
import { ProfileService } from '../api/profile.service';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { Profile } from '../api/gen/models';

@Injectable()
export class UserService {
    tokenState: AuthTokenState;
    profile: Profile = {};
    public profile$: BehaviorSubject<Profile> = new BehaviorSubject<Profile>(this.profile);

    constructor(
        private authSvc: AuthService,
        private profileSvc: ProfileService
    ) {
        this.authSvc.tokenState$.subscribe(
            (state: AuthTokenState) => {
                this.tokenStateChanged(state);
            }
        );
    }

    tokenStateChanged(state: AuthTokenState): void {
        this.tokenState = state;
        switch (state) {
            case AuthTokenState.valid:
                this.getProfile(false).subscribe(
                    (p: Profile) => {
                        this.profile = p;
                        this.profile$.next(this.profile);
                    }
                );
                break;

            case AuthTokenState.invalid:
                this.profile = {};
                this.profile$.next(this.profile);
                break;
        }
    }

    getProfile(reload?: boolean): Observable<Profile> {
        if (this.profile.id && !reload) {
            return of(this.profile);
        }

        return this.profileSvc.getProfile();
    }

}
