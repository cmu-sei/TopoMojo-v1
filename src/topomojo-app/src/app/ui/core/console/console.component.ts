import {
  Component, OnInit, ViewChild, ViewRef, AfterViewInit,
  ElementRef, Input, Injector, HostListener, OnDestroy
} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VmService } from '../../../api/vm.service';
import { FileService } from '../../../api/file.service';
import { DisplayInfo, VmOperationTypeEnum } from '../../../api/gen/models';
import { catchError, debounceTime, map, distinctUntilChanged } from 'rxjs/operators';
import { throwError as ObservableThrower, of, fromEvent, Observable, Subscription } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { MockConsoleService } from './services/mock-console.service';
import { WmksConsoleService } from './services/wmks-console.service';
import { ConsoleService } from './services/console.service';
import { ToolbarService } from '../../svc/toolbar.service';
import { MatDrawer } from '@angular/material/sidenav';
import { AuthService, AuthTokenState } from '../../../svc/auth.service';
import { IsoDataSource, IsoFile, VmNetDataSource } from '../../datasources';
import { TopologyService } from '../../../api/topology.service';

@Component({
  selector: 'topomojo-console',
  templateUrl: './console.component.html',
  styleUrls: ['./console.component.scss'],
  providers: [
    MockConsoleService,
    WmksConsoleService
  ]
})
export class ConsoleComponent implements OnInit, AfterViewInit, OnDestroy {

  @Input() index = 0;
  @Input() id: string;
  info: DisplayInfo = {};
  state = 'loading';
  shadowstate = 'loading';
  shadowTimer: any;
  canvasId = '';
  stateButtonIcons: any = {};
  stateIcon = '';
  console: ConsoleService;
  @ViewChild(MatDrawer) drawer: MatDrawer;
  @ViewChild('consoleCanvas') consoleCanvas: ElementRef;
  subs: Array<Subscription> = [];
  private hotspot = { x: 0, y: 0, w: 20, h: 20 };
  isoSource: IsoDataSource;
  netSource: VmNetDataSource;

  constructor(
    private injector: Injector,
    private route: ActivatedRoute,
    private titleSvc: Title,
    private vmSvc: VmService,
    private topologySvc: TopologyService,
    private toolbar: ToolbarService,
    private tokenSvc: AuthService
  ) {
  }

  ngOnInit() {
    this.info.id = this.id || this.route.snapshot.paramMap.get('id');
  }

  ngAfterViewInit() {
    this.toolbar.visible(false);

    this.initTokenWatch();
    this.initHotspot();

    const el = this.consoleCanvas['nativeElement'];
    this.canvasId = el.id + this.index;
    el.id += this.index;

    this.titleSvc.setTitle(`console: ${this.route.snapshot.paramMap.get('name')}`);
    setTimeout(() => this.reload(), 1);
  }

  ngOnDestroy() {
    this.subs.forEach(s => s.unsubscribe());
    if (this.console) { this.console.dispose(); }
  }

  changeState(state: string): void {
    console.log(state);
    this.state = state;
    this.shadowState(state);
    this.drawer.close();

    switch (state) {
      case 'stopped':
        this.stateIcon = 'power_settings_new';
        break;

      case 'disconnected':
        this.stateIcon = 'refresh';
        break;

      case 'forbidden':
        this.stateIcon = 'block';
        break;

      case 'failed':
        this.stateIcon = 'close';
        break;

      default:
        this.stateIcon = '';
    }
  }

  stateButtonClicked(): void {
    switch (this.state) {
      case 'stopped':
        this.start();
        break;

      default:
        this.reload();
        break;
    }
  }

  reload() {
    this.changeState('loading');

    this.vmSvc.getVmTicket(this.info.id)
      .pipe(
        catchError(((err: Error) => {
          return ObservableThrower(err);
        })
        ))
      .subscribe(
        (info: DisplayInfo) => {
          if (info.topoId !== this.info.topoId) {
            this.isoSource = new IsoDataSource(this.topologySvc, info.topoId);
            this.netSource = new VmNetDataSource(this.topologySvc, info.topoId);
          }
          this.info = info;
          this.console = (this.isMock())
            ? this.injector.get(MockConsoleService)
            : this.injector.get(WmksConsoleService);
          if (info.id) {
            if (info.isRunning) {
              this.console.connect(
                this.info.url,
                (state: string) => this.changeState(state),
                { canvasId: this.canvasId }
              );
            } else {
              this.changeState('stopped');
            }
          } else {
            this.changeState('failed');
          }
        },
        (err: Error) => {
          // show error
          this.changeState('failed');
        },
        () => {
        }
      );

  }

  start(): void {
    this.changeState('starting');
    this.vmSvc.postVmAction({
      id: this.info.id,
      type: VmOperationTypeEnum.start
    }).subscribe(
      () => {
        this.reload();
      }
    );
  }

  isoSelected(iso: IsoFile) {
    this.vmSvc.postVmChange(this.info.id, { key: 'iso', value: iso.path }).subscribe();
  }

  netSelected(net: IsoFile) {
    this.vmSvc.postVmChange(this.info.id, { key: 'net', value: net.path }).subscribe();
  }

  shadowState(state: string): void {
    this.shadowstate = state;
    if (this.shadowTimer) { clearTimeout(this.shadowTimer); }
    this.shadowTimer = setTimeout(() => { this.shadowstate = ''; }, 5000);
  }

  isConnected(): boolean {
    return this.state === 'connected';
  }

  isMock(): boolean {
    return this.info.conditions && this.info.conditions.match(/mock/) !== null;
  }

  showMockConnected(): boolean {
    return this.isMock() && this.state === 'connected';
  }

  toggleDrawer(): void {
    this.drawer.toggle();
  }

  initTokenWatch(): void {
    this.subs.push(
      this.tokenSvc.tokenState$.subscribe(
        (state: AuthTokenState) => {
          if (state === AuthTokenState.expired) {
            this.console.disconnect();
          }
        }
      )
    );
  }

  initHotspot(): void {
    this.hotspot.x = window.innerWidth - this.hotspot.w;
    this.subs.push(
      fromEvent(document, 'mousemove').pipe(
        debounceTime(100),
        map((e: MouseEvent) => {
          return this.isConnected() && e.clientX > this.hotspot.x && e.clientY < this.hotspot.h;
        }),
        distinctUntilChanged()
      ).subscribe((hot: boolean) => {
        if (hot) { this.drawer.open(); }
      })
    );
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    this.hotspot.x = event.target.innerWidth - this.hotspot.w;
    this.console.refresh();
  }
}
