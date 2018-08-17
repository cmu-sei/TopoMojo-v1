import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TemplateDetailFormComponent } from './template-detail-form.component';

describe('TemplateDetailFormComponent', () => {
  let component: TemplateDetailFormComponent;
  let fixture: ComponentFixture<TemplateDetailFormComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TemplateDetailFormComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TemplateDetailFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
