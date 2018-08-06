
export interface ConsoleService {
  connect(url: string, stateCallback: Function, options: any);
  sendCAD();
  refresh();
  dispose();
}
