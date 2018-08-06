import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { VmQuestionComponent } from './vm-question.component';

describe('VmQuestionComponent', () => {
  let component: VmQuestionComponent;
  let fixture: ComponentFixture<VmQuestionComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ VmQuestionComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(VmQuestionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
