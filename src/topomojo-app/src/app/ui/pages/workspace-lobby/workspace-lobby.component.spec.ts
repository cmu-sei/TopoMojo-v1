import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { WorkspaceLobbyComponent } from './workspace-lobby.component';

describe('WorkspaceBrowserComponent', () => {
  let component: WorkspaceLobbyComponent;
  let fixture: ComponentFixture<WorkspaceLobbyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ WorkspaceLobbyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WorkspaceLobbyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
