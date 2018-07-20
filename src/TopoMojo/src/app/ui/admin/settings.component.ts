import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../api/admin.service';
import { VmService } from '../../api/vm.service';

@Component({
    selector: 'app-admin-settings',
    templateUrl: 'settings.component.html',
    styleUrls: ['settings.component.css']
})
export class AdminSettingsComponent implements OnInit {

    constructor(
        private svc: AdminService,
        private vmSvc: VmService
    ) { }

    announceText = '';
    settings = '';
    hostField = '';
    errors: any[] = [];
    importResults = '';
    liveUsers = '';

    ngOnInit() {
    }

    load(): void {
        this.svc.getAdminGetsettings().subscribe(
            (result) => {
                this.settings = JSON.stringify(result, null, 2);
            }
        );
    }

    save(): void {
        try {
            const s = JSON.parse(this.settings);
            this.svc.postAdminSavesettings(s).subscribe(
                (result) => {

                }
            );
        } catch (Error) {
            this.errors.push(Error);
        }
    }
    announce(): void {
        console.log('announce: ' + this.announceText);
        if (this.announceText && !!this.announceText.trim()) {
            this.svc.postAdminAnnounce(this.announceText).subscribe(
                (result) => {
                    this.announceText = '';
                }
            );
        }
    }

    reloadHost(): void {
        this.vmSvc.postHostReload(this.hostField).subscribe(
            () => {
            }
        );
    }

    import(): void {
        this.svc.getAdminImport().subscribe(
            (results: Array<string>) => {
                this.importResults = results.join('\n');
            }
        );
    }

    loadLiveUsers(): void {
        this.svc.getAdminLive().subscribe(
            (results: Array<any>) => {
                this.liveUsers = JSON.stringify(results, null, 4);
            }
        );
    }
}
