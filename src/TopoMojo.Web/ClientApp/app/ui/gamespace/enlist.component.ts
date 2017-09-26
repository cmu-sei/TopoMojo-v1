import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRouteSnapshot, Params } from '@angular/router';
import { GamespaceService } from '../../api/gamespace.service';

@Component({
    template: `
        <p>Validating enlistment code...</p>
    `
})
export class GamespaceEnlistComponent implements OnInit {

    constructor(
        private service: GamespaceService,
        private route: ActivatedRouteSnapshot,
        private router: Router
    ) { }

    ngOnInit(): void {
        let code = this.route.paramMap.get("code");
        this.service.enlistPlayer(code)
            .subscribe(result => {
                this.router.navigate(['/mojo']);
            }, (err) => { });
    }

}