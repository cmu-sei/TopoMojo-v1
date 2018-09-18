import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRouteSnapshot, ActivatedRoute, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'topomojo-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  navLinks: Array<NavLink> = [
    { label: 'Dashboard', path: 'dash' },
    { label: 'Gamespace', path: 'mojo' },
    { label: 'Workspace', path: 'topo' },
    { label: 'Template', path: 'tempo' },
    { label: 'Machines', path: 'vms' },
    { label: 'People', path: 'people' }
  ];
  showDefault = true;

  constructor(
    private router: Router,
    private route: ActivatedRoute
  ) { }

  ngOnInit() {
    this.router.navigate(['admin', 'dash']);
  }

}

export interface NavLink {
  label?: string;
  path?: string;
}
