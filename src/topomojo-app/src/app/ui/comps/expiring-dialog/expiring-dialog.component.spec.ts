import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ExpiringDialogComponent } from './expiring-dialog.component';

describe('ExpiringDialogComponent', () => {
  let component: ExpiringDialogComponent;
  let fixture: ComponentFixture<ExpiringDialogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ExpiringDialogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ExpiringDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
