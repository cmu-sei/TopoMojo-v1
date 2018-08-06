import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkspaceSettingsComponent } from './workspace-settings.component';

describe('WorkspaceSettingsComponent', () => {
  let component: WorkspaceSettingsComponent;
  let fixture: ComponentFixture<WorkspaceSettingsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkspaceSettingsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkspaceSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
