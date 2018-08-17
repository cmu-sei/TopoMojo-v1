import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { TopologyService } from '../../../api/topology.service';
import { GamespaceService } from '../../../api/gamespace.service';

@Component({
  selector: 'topomojo-enlist',
  templateUrl: './enlist.component.html',
  styleUrls: ['./enlist.component.scss']
})
export class EnlistComponent implements OnInit {
  complete = false;
  errors = new Array<Error>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private workspaceSvc: TopologyService,
    private gamespaceSvc: GamespaceService
  ) { }

  ngOnInit() {
    const code = this.route.snapshot.paramMap.get('code');
    const url = this.route.snapshot.pathFromRoot.map(o => o.url[0]).join('/');
    // TODO: clean this up with a single api endpoint
    const query = (url.startsWith('/mojo'))
      ? this.gamespaceSvc.postPlayerCode(code)
      : this.workspaceSvc.postWorkerCode(code);

    query.pipe(
      finalize(() => this.complete = true)
    ).subscribe(
      (result) => {
        this.router.navigate(['/topo'] );
      },
      (err) => this.errors.push(err.error || err)
    );
  }

}
