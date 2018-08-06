
import { Injectable } from '@angular/core';
import { SettingsService } from '../svc/settings.service';

@Injectable()
export class ApiSettings {

    constructor(
        settingsSvc: SettingsService
    ) {
        this.url = settingsSvc.settings.urls.apiUrl;
    }

    url: string;
}
