import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopologyService } from '../../../api/topology.service';

@Component({
  selector: 'topomojo-workspace-creator',
  templateUrl: './workspace-creator.component.html',
  styleUrls: ['./workspace-creator.component.scss']
})
export class WorkspaceCreatorComponent implements OnInit {

  @Input() redirect = true;
  name = '';
  description = '';
  visible = false;
  errorMessage = '';

  constructor(
    private router: Router,
    private workspaceSvc: TopologyService
  ) { }

  ngOnInit() {
  }

  clicked(): void {
    if (this.visible && this.name) {
      // TODO: send and route or show error
      this.workspaceSvc.postTopology({
        name: this.name,
        description: this.description
      }).subscribe((ws) => {
        this.router.navigate(['/topo', ws.id]);
        this.name = '';
      });
    }
    this.visible = !this.visible;
  }
}
