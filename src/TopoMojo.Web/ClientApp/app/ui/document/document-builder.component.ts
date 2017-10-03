import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { TopologyService } from '../../api/topology.service';
import { Topology } from '../../api/gen/models';
import { SettingsService } from '../../svc/settings.service';

@Component({
    selector: 'doc-builder',
    templateUrl: 'document-builder.component.html',
    styleUrls: [ 'document-builder.component.css']
})
export class DocumentBuilderComponent implements OnInit, OnDestroy {

    constructor(
        private svc : TopologyService,
        private route: ActivatedRoute,
        private settings: SettingsService
    ) {
        this.settings.changeLayout({ embedded : true });

    }

    topo: Topology;
    errors: any[] = [];
    loading: boolean = true;
    showImageDiv: boolean;
    private appName: string;

    ngOnInit() {
        this.appName = (this.settings.branding.applicationName || "TopoMojo") + " Document";
        let id = +this.route.snapshot.paramMap.get('id');
        this.svc.getTopology(id).finally(
            () => {
                this.loading = false;
            })
            .subscribe(
            (result: Topology) => {
                console.log(result);
                this.topo = result;
            },
            (err) => {
                this.onError(err);
            }
        );
    }

    ngOnDestroy() {
        this.settings.changeLayout({embedded: false});
    }
    onError(err) {
        //let text = JSON.parse(err.text());
        this.errors.push(err.error);
        console.debug(err.error.message);
    }

    toggleImageDiv() {
        this.showImageDiv = !this.showImageDiv;
    }
}