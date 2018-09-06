import { CancellationToken, CancellationTokenSource, Disposable } from "vscode";
import {
	CommandType,
	ICommand,
	ICommandResult,
	IExecutionCommand
} from "./proxy";
import { VdfProxy } from "./vdfProxy";

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

	public sendNonCancellableCommand(
		cmd: ICommand<R>,
		token?: CancellationToken
	): Promise<R | undefined> {
		const executionCmd = <IExecutionCommand<R>>cmd;
		executionCmd.id = executionCmd.id || this.vdfProxy.getNextCommandId();
		executionCmd.token = token;

		return this.vdfProxy.sendCommand<R>(executionCmd).catch(reason => {
			console.error(reason);
			return undefined;
		});
	}
	public dispose() {
		if (this.vdfProxy) {
			this.vdfProxy.dispose();
		}

		this.commandCancellationTokenSources.forEach(ct => {
			if (ct) ct.cancel;
		});
	}
}
