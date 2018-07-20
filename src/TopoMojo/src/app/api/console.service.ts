
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiSettings } from './api-settings';
import { GeneratedConsoleService } from './gen/console.service';
import {  } from './gen/models';

@Injectable()
export class ConsoleService extends GeneratedConsoleService {

    constructor(
       protected http: HttpClient,
       protected api: ApiSettings
    ) { super(http, api); }
}
