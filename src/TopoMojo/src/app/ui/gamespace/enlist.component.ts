import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { GamespaceService } from '../../api/gamespace.service';

@Component({
    templateUrl: 'enlist.component.html'
})
export class GamespaceEnlistComponent implements OnInit {

    constructor(
        private service: GamespaceService,
        private route: ActivatedRoute,
        private router: Router
    ) { }

    complete: boolean;
    errors: any[] = [];

    ngOnInit(): void {
        let code = this.route.snapshot.paramMap.get("code");
        this.service.enlistPlayer(code)
            .subscribe(
                result => {
                    this.router.navigate(['/mojo']);
                },
                (err) => { this.onError(err); },
                () => { this.complete = true;}
            );
    }

    onError(err) {
        this.errors.push(err.error);
        //console.debug(err.error.message);
    }
}