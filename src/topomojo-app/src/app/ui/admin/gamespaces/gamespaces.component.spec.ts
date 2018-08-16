import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GamespacesComponent } from './gamespaces.component';

describe('GamespacesComponent', () => {
  let component: GamespacesComponent;
  let fixture: ComponentFixture<GamespacesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GamespacesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GamespacesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
