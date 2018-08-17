import { Component, OnInit, Input } from '@angular/core';
import { Router } from '@angular/router';
import { TopologySummary } from '../../../api/gen/models';

@Component({
  selector: 'topomojo-workspace-summary',
  templateUrl: './workspace-summary.component.html',
  styleUrls: ['./workspace-summary.component.scss']
})
export class WorkspaceSummaryComponent implements OnInit {

  @Input() summary: TopologySummary;

  constructor(
    private router: Router
  ) { }

  ngOnInit() {
  }

}
