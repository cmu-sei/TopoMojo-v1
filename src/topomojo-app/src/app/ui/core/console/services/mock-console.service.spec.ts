import { TestBed, inject } from '@angular/core/testing';

import { MockConsoleService } from './mock-console.service';

describe('MockConsoleService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MockConsoleService]
    });
  });

  it('should be created', inject([MockConsoleService], (service: MockConsoleService) => {
    expect(service).toBeTruthy();
  }));
});
