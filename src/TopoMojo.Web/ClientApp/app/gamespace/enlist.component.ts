import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { GamespaceService } from './gamespace.service';
import 'rxjs/add/operator/switchMap';

@Component({
    //moduleId: module.id,
    //selector: 'enlist',
    template: `
        <p>Validating enlistment code...</p>
    `
})
export class GamespaceEnlistComponent implements OnInit {

    constructor(
        private service: GamespaceService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    ngOnInit(): void {
        let code = this.route.snapshot.paramMap.get("code");
        this.service.enlist(code)
            .subscribe(result => {
                this.router.navigate(['/mojo']);
            }, (err) => { this.service.onError(err); });

    }



}