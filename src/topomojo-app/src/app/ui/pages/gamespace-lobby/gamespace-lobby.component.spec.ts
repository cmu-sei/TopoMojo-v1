import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GamespaceLobbyComponent } from './gamespace-lobby.component';

describe('GamespaceLobbyComponent', () => {
  let component: GamespaceLobbyComponent;
  let fixture: ComponentFixture<GamespaceLobbyComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GamespaceLobbyComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GamespaceLobbyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
