import { Disposable, Uri, workspace } from 'vscode';
import { ICommandResult } from './proxy';
import { VdfProxy } from './vdfProxy';
import { VdfProxyHandler } from './vdfProxyHandler';

let _vdfProxyService: VdfProxyService;

export function setVdfProxyService(vdfProxyFactory: VdfProxyService) {
  _vdfProxyService = vdfProxyFactory;
}

export function getVdfProxyService(): VdfProxyService {
  if (_vdfProxyService === undefined) {
    throw "VdfProxyFactory has not been initialized.";
  }

  return _vdfProxyService;
}

export class VdfProxyService implements Disposable {
  private vdfProxyHandler?: VdfProxyHandler<ICommandResult>;

  constructor(private extensionRootPath: string) {
    this.getVdfProxyHandler(null);
  }

  dispose() {
    if (this.vdfProxyHandler) {
      this.vdfProxyHandler.dispose();
    }
  }

  //TODO: Should be able to handle multiple workspaces.
  public getVdfProxyHandler<T extends ICommandResult>(
    resource?: Uri
  ): VdfProxyHandler<T> {
    if (!this.vdfProxyHandler) {
      const vdfProxy = new VdfProxy(this.extensionRootPath, workspace.rootPath);
      this.vdfProxyHandler = new VdfProxyHandler(vdfProxy);
    }

    return this.vdfProxyHandler! as VdfProxyHandler<T>;
  }
}
