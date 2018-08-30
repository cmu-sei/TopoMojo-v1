import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ErrorDivComponent } from './error-div.component';

describe('ErrorDivComponent', () => {
  let component: ErrorDivComponent;
  let fixture: ComponentFixture<ErrorDivComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ErrorDivComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ErrorDivComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
