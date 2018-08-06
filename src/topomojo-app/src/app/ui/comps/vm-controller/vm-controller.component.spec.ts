import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VmControllerComponent } from './vm-controller.component';

describe('VmControllerComponent', () => {
  let component: VmControllerComponent;
  let fixture: ComponentFixture<VmControllerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VmControllerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VmControllerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
