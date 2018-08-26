import { Disposable, CancellationToken, CancellationTokenSource } from "vscode";
import { VdfProxy } from "./vdfProxy";
import {
  ICommand,
  IExecutionCommand,
  ICommandResult,
  CommandType
} from "./proxy";

export class VdfProxyHandler<R extends ICommandResult> implements Disposable {
  private commandCancellationTokenSources: Map<
    CommandType,
    CancellationTokenSource
  >;

  public constructor(private vdfProxy: VdfProxy) {
    this.commandCancellationTokenSources = new Map<
      CommandType,
      CancellationTokenSource
    >();
  }

  public get VdfProxy(): VdfProxy {
    return this.vdfProxy;
  }

  // Check cancellation here
  public sendCommand(
    cmd: ICommand<R>,
    token?: CancellationToken
  ): Promise<R | undefined> {
    const executionCmd = <IExecutionCommand<R>>cmd;
    executionCmd.id = executionCmd.id || this.vdfProxy.getNextCommandId();

    // If a previous command of this same type exist, Cancel it!
    if (this.commandCancellationTokenSources.has(cmd.command)) {
      const ct = this.commandCancellationTokenSources.get(cmd.command);
      if (ct) {
        ct.cancel();
      }
    }

    const cancellation = new CancellationTokenSource();
    this.commandCancellationTokenSources.set(cmd.command, cancellation);
    executionCmd.token = cancellation.token;

    return this.vdfProxy.sendCommand<R>(executionCmd).catch(reason => {
      console.error(reason);
      return undefined;
    });
  }

  public dispose() {
    if (this.vdfProxy) {
      this.vdfProxy.dispose();
    }
  }
}
