import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../api/admin.service';
import { VmService } from '../../api/vm.service';

@Component({
    selector: 'admin-settings',
    templateUrl: 'settings.component.html',
    styleUrls: ['settings.component.css']
})
export class AdminSettingsComponent implements OnInit {

    constructor(
        private svc : AdminService,
        private vmSvc: VmService
    ) { }

    announceText: string = "";
    settings: string = "";
    hostField: string = "";
    errors: any[] = [];

    ngOnInit() {
    }

    load() : void {
        this.svc.getsettingsAdmin().subscribe(
            (result) => {
                this.settings = JSON.stringify(result, null, 2);
            }
        )
    }

    save() : void {
        try {
            let s = JSON.parse(this.settings);
            this.svc.savesettingsAdmin(s).subscribe(
                (result) => {

                }
            );
        } catch(Error) {
            this.errors.push(Error);
        }
    }
    announce() : void {
        console.log('announce: ' + this.announceText);
        if (this.announceText && !!this.announceText.trim()) {
            this.svc.announceAdmin(this.announceText).subscribe(
                (result) => {
                    this.announceText = "";
                }
            );
        }
    }

    reloadHost() : void {
        this.vmSvc.reloadHost(this.hostField).subscribe(
            () => {
            }
        );
    }
}