import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { DocumentImageManagerComponent } from './document-image-manager.component';

describe('DocumentImageManagerComponent', () => {
  let component: DocumentImageManagerComponent;
  let fixture: ComponentFixture<DocumentImageManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DocumentImageManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DocumentImageManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
