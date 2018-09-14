import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopologyService } from '../../../api/topology.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'topomojo-workspace-creator',
  templateUrl: './workspace-creator.component.html',
  styleUrls: ['./workspace-creator.component.scss']
})
export class WorkspaceCreatorComponent implements OnInit {

  @Input() redirect = true;
  name = '';
  description = '';
  errors = new Array<Error>();

  constructor(
    private router: Router,
    private workspaceSvc: TopologyService
  ) { }

  ngOnInit() {
  }

  clicked(): void {
    if (!!this.name) {
      this.workspaceSvc.postTopology({
        name: this.name,
        description: this.description
      }).subscribe(
        (ws) => {
          this.router.navigate(['/topo', ws.id]);
          this.name = '';
        },
        (err: HttpErrorResponse) => {
          if (err.error.message.match(/WorkspaceLimitException/)) {
            err.error.message = 'You have reached the workspace limit and so cannot create one now.';
          }
          this.errors.push(err.error);
        }
      );
    }
  }
}
