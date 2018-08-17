
export interface ConsoleService {
  connect(url: string, stateCallback: Function, options: any);
  disconnect();
  refresh();
  sendCAD();
  toggleScale();
  fullscreen();
  showKeyboard();
  showExtKeypad();
  showTrackpad();
  dispose();
}
