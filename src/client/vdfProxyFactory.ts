import { Disposable, Uri, workspace } from "vscode";
import { VdfProxyHandler } from "./vdfProxyHandler";
import { VdfProxy } from "./vdfProxy";
import { ICommandResult } from "./proxy";

export class VdfProxyFactory implements Disposable {
  private vdfProxyHandler?: VdfProxyHandler<ICommandResult>;

  constructor(private extensionRootPath: string) {
    this.getVdfProxyHandler(null);
  }

  dispose() {
    if (this.vdfProxyHandler) this.vdfProxyHandler.dispose();
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
