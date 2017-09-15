import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { GamespaceService } from '../api/gamespace.service';
import { VmService } from '../vm/vm.service';

@Component({
    selector: 'controlbar',
    templateUrl: 'controlbar.component.html'
})
export class ControlBarComponent implements OnInit {

    constructor(
        private router: Router,
        private service: GamespaceService,
        private vmService: VmService
    ) { }

    @Input() game : any;
    @Input() loading: boolean;

    ngOnInit() {

    }

    // launch() {
    //     this.launching = true;
    //     this.status = "Launching topology instance...";
    //     this.service.launchInstance(this.game.id).subscribe(
    //         (result) => {
    //             this.summary = result;
    //             this.status = '';
    //             this.launching = false;
    //         },
    //         (err) => {
    //             this.errorMessage = err.json().message;
    //             //this.service.onError(err);
    //         }
    //     );
    // }

    launchConsole(id) {
        console.log('launch console ' + id);
        //this.vmService.launchPage("/vm/display/" + id);
    }

    destroy() {
        this.service.deleteGamespace(this.game.id)
        .subscribe(result => {
            this.game = null;
            this.router.navigate(['/']);
        });
    }

}