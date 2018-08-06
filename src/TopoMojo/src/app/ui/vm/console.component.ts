import { Component, OnInit, ViewChild, ViewRef, AfterViewInit, ElementRef, Renderer2, Input, Injector, HostListener, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VmService } from '../../api/vm.service';
import { FileService } from '../../api/file.service';
import { DisplayInfo, VmOperationTypeEnum } from '../../api/gen/models';
import { catchError } from 'rxjs/operators';
import { throwError as ObservableThrower, of } from 'rxjs';
import { Title } from '@angular/platform-browser';
import { MockConsoleService } from './services/mock-console.service';
import { WmksConsoleService } from './services/wmks-console.service';
import { ConsoleService } from './services/console.service';

@Component({
  selector: 'app-console',
  templateUrl: './console.component.html',
  styleUrls: ['./console.component.css'],
  providers: [
    MockConsoleService,
    WmksConsoleService
  ]
})
export class ConsoleComponent implements AfterViewInit, OnDestroy {

  @Input() index = 0;
  @Input() id: string;
  info: DisplayInfo = {};
  state = 'loading';
  shadowstate = 'loading';
  shadowTimer: any;
  canvasId = '';
  stateButtonIcons: any = {};
  console: ConsoleService;
  @ViewChild('consoleCanvas') consoleCanvas: ElementRef;
  @ViewChild('stateBtn') stateBtn: ElementRef;
  stateBtnRef: any;

  constructor(
    private injector: Injector,
    private route: ActivatedRoute,
    private renderer: Renderer2,
    private titleSvc: Title,
    private vmSvc: VmService
  ) { }

  ngAfterViewInit() {
    this.stateBtnRef = this.stateBtn['nativeElement'];

    const el = this.consoleCanvas['nativeElement'];
    this.canvasId = el.id + this.index;
    el.id += this.index;

    this.titleSvc.setTitle(`console: ${this.route.snapshot.paramMap.get('name')}`);
    this.info.id = this.id || this.route.snapshot.paramMap.get('id');
    this.reload();
  }

  ngOnDestroy() {
    if (this.console) { this.console.dispose(); }
  }

  changeState(state: string): void {
    this.state = state;
    this.shadowstate = state;
    if (this.shadowTimer) { clearTimeout(this.shadowTimer); }
    this.shadowTimer = setTimeout(() => { this.shadowstate = ''; }, 5000);
    this.updateActiveButtonClasses();
  }

  stateButtonClicked(): void {
    this.stateBtnRef.blur();
    switch (this.state) {
      case 'stopped':
      this.start();
      break;

      default:
      this.reload();
      break;
    }
  }

  updateActiveButtonClasses(): any {
    this.stateButtonIcons = {
      'fa': true,
      'fa-circle-o-notch fa-spin': this.state === 'loading' || this.state === 'starting',
      'fa-power-off': this.state === 'stopped',
      'fa-refresh': this.state === 'disconnected',
      'fa-ban': this.state === 'forbidden',
      'fa-times': this.state === 'failed',
      'text text-danger': this.state === 'forbidden' || this.state === 'failed',
      'hidden': this.state === 'connected'
    };
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
        this.info = info;
        this.console = (this.isMock())
          ? this.injector.get(MockConsoleService)
          : this.injector.get(WmksConsoleService);
        if (info.isRunning) {
          this.console.connect(
            this.info.url,
            (state: string) => { this.changeState(state); },
            { canvasId: this.canvasId }
          );
        } else {
          this.changeState('stopped');
        }
      },
      (err: Error) => {
        // show error
        // this.setState('failed');
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

  isConnected(): boolean {
    return this.state === 'connected';
  }
  isMock(): boolean {
    return this.info.conditions && this.info.conditions.match(/mock/).length > 0;
  }

  showMockConnected(): boolean {
    return this.isMock() && this.state === 'connected';
  }

  @HostListener('window:resize', ['$event'])
  onResize(event) {
    // this will re-center the console
    this.console.refresh();
  }
}
