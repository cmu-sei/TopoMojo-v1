import { TestBed, inject } from '@angular/core/testing';

import { WmksConsoleService } from './wmks-console.service';

describe('WmksConsoleService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WmksConsoleService]
    });
  });

  it('should be created', inject([WmksConsoleService], (service: WmksConsoleService) => {
    expect(service).toBeTruthy();
  }));
});
