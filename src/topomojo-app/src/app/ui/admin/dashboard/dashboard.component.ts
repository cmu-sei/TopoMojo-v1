import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../api/admin.service';
import { CachedConnection } from '../../../api/gen/models';

@Component({
  selector: 'topomojo-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {

  connections = new Array<CachedConnection>();
  announcement = '';
  exportIds = '';
  importResult = new Array<string>();

  constructor(
    private adminSvc: AdminService
  ) { }

  ngOnInit() {
    this.refresh();
  }

  refresh() {
    this.adminSvc.getAdminLive().subscribe(
      (list: CachedConnection[]) => {
        this.connections = list;
      }
    )
  }

  announce() {
    this.adminSvc.postAdminAnnounce(this.announcement).subscribe(
      () => {
        this.announcement = '';
      }
    );
  }

  import() {
    this.adminSvc.getAdminImport().subscribe(
      (r: string[]) => {
        this.importResult = r;
      }
    );
  }

  export() {
    const ids = this.exportIds.split(/[,\ ]/).map(v => +v);
    this.adminSvc.postAdminExport(
      ids
    ).subscribe(() => this.exportIds = '');
  }

  trackById(i: number, item: CachedConnection): string {
    return item.id;
  }
}
