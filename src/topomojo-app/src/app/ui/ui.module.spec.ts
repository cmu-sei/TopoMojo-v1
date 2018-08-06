import { UiModule } from './ui.module';

describe('MaterialModule', () => {
  let materialModule: UiModule;

  beforeEach(() => {
    materialModule = new UiModule();
  });

  it('should create an instance', () => {
    expect(materialModule).toBeTruthy();
  });
});
