import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'topomojo-admin',
  templateUrl: './admin.component.html',
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent implements OnInit {
  navLinks: Array<NavLink> = [
    { label: 'Gamespace', path: 'mojo' },
    { label: 'Workspace', path: 'topo' },
    { label: 'Template', path: 'tempo' },
    { label: 'Machines', path: 'vms' },
    { label: 'People', path: 'people' }
  ];
  constructor() { }

  ngOnInit() {
  }

}

export interface NavLink {
  label?: string;
  path?: string;
}
