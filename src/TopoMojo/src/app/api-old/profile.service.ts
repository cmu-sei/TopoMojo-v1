
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { GeneratedProfileService } from "./gen/profile.service";
import { Profile,ProfileSearchResult,Search } from "./gen/models";

@Injectable()
export class ProfileService extends GeneratedProfileService {

    constructor(
       protected http: HttpClient
    ) { super(http); }
}
