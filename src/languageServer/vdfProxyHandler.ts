import { Disposable } from "vscode";
import { VdfProxy } from "./vdfProxy";
import { ICommand, IExecutionCommand, ICommandResult } from "./proxy";

export class VdfProxyHandler<R extends ICommandResult> implements Disposable {
	public constructor(private vdfProxy: VdfProxy) {}

	public get VdfProxy(): VdfProxy {
		return this.vdfProxy;
	}

	public sendCommand(cmd: ICommand<R>): Promise<R | undefined> {
		const executionCmd = <IExecutionCommand<R>>cmd;
		executionCmd.id = executionCmd.id || this.vdfProxy.getNextCommandId();

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
