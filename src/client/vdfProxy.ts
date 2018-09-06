import * as fs from "fs";
import * as path from "path";
import * as vscode from "vscode";
import { ChildProcess, spawn } from "child_process";
import {
	createDeferred,
	ICommand,
	ICommandResult,
	IExecutionCommand,
	IPCMessage
} from "./proxy";
import { getUI, UI } from "../common/ui";
import "../common/extensions";

export class VdfProxy implements vscode.Disposable {
	private proc?: ChildProcess;
	private workspacePath: string;
	private indexPath: string;
	private languageServerStarted: boolean;
	private cmdId: number = 0;
	private previousData = "";
	private commands = new Map<number, IExecutionCommand<ICommandResult>>();
	private commandQueue: number[] = [];
	private ui: UI;
	private retry: number = 0;

	constructor(private extensionRootDir: string, workspacePath: string) {
		this.workspacePath = workspacePath;
		this.indexPath = this.getIndexPath();
		this.ui = getUI();
		this.startLanguageServer().then(() => (this.languageServerStarted = true));
	}

	private async startLanguageServer(): Promise<void> {
		//TODO: Do any initializing
		return this.restartLanguageServer();
	}

	private restartLanguageServer(): Promise<void> {
		this.killProcess();
		this.clearPendingRequests();
		this.ui.IsUpdatingIndex = true;
		return this.initialize();
	}

	private initialize(): Promise<void> {
		console.log(process.env.NODE_ENV);

		const server =
			process.env.NODE_ENV && process.env.NODE_ENV === "development"
				? "src/server/VDFServer/VDFServer/bin/Debug/netcoreapp2.1/VDFServer.dll"
				: "resources/server/VDFServer.dll";

		let exePath = path.join(this.extensionRootDir, server);

		return this.spawnProcess(exePath, this.workspacePath, this.indexPath).catch(
			ex => {
				this.languageServerStarted = false;
				//TODO: Add Error handling
			}
		);
	}

	private async spawnProcess(
		exePath: string,
		workspacePath: string,
		indexPath: string
	) {
		var proc = spawn("dotnet", [exePath, indexPath, workspacePath]);

		proc.on("error", error => {
			console.log("Process error:", error);
			this.clearPendingRequests();
		});

		proc.on("message", message => {
			console.log("Process message:", message);
		});

		proc.on("close", close => {
			console.log("Process close:", close);
			if (this.retry < 10) {
				this.retry += 1;
				this.restartLanguageServer();
			}
		});

		proc.on("disconnect", disconnect => {
			console.log("Process disconnect:", disconnect);
		});

		proc.stdin.setDefaultEncoding("utf8");

		proc.stderr.on("data", data => {
			console.log("Error Data: ", data.toString());
			this.clearPendingRequests();
		});

		proc.stdout.on("data", stream => {
			const data: string = stream.toString();
			const processData: string = `${this.previousData}${data}`;
			this.previousData = processData;

			//console.log("Stream Data: ", data);
			//console.log("Process Data: ", processData);

			let results: ICommandResult[];
			try {
				results = processData.splitLines().map(data => JSON.parse(data));
				this.previousData = "";
			} catch (ex) {
				console.log("Error:", ex);
				// Possible we've only received part of the data,
				// don't clear previousData.
				return;
			}

			results.forEach(result => {
				if (result.isInternal) {
					this.handleInternalMessaging(result);
				} else {
					this.handleResult(result);
				}
			});

			return;
		});

		this.proc = proc;
	}

	private clearPendingRequests() {
		const items = this.commandQueue.splice(0, this.commandQueue.length);
		items.forEach(id => {
			if (this.commands.has(id)) {
				const cmd = this.commands.get(id);
				this.tryResolve(cmd, undefined);
				this.commands.delete(id);
			}
		});
	}

	private clearRequestFromQueue(
		result: ICommandResult
	): IExecutionCommand<ICommandResult> | undefined {
		const cmd = this.commands.get(result.requestId);
		if (cmd) {
			this.commands.delete(result.requestId);

			const index = this.commandQueue.indexOf(cmd.id);
			if (index !== -1) {
				this.commandQueue.splice(index, 1);
			}
		}

		return cmd;
	}

	private handleResult(result: ICommandResult) {
		if (result.requestId != -1 && this.commands.has(result.requestId)) {
			const cmd = this.clearRequestFromQueue(result);
			if (cmd) {
				if (cmd.token.isCancellationRequested) {
					this.tryResolve(cmd, undefined);
				} else {
					this.tryResolve(cmd, result);
				}
			}
		}
	}

	private handleInternalMessaging(result: ICommandResult) {
		switch (result.messageType) {
			case IPCMessage.LanguageServerIndexingComplete:
				this.ui.IsUpdatingIndex = false;
				vscode.window.setStatusBarMessage(
					`VDF Language Server Indexing Completed: Processing Time ${result.metaData.trim()} seconds`,
					10000
				);
				break;
			case IPCMessage.LanguageServerIndexing:
				vscode.window.showInformationMessage(
					"VDF Language Server is currently indexing files. Please try again when indexing has been completed."
				);
			case IPCMessage.SymbolNotFound:
			case IPCMessage.NoProviderFound:
				if (result.requestId != -1 && this.commands.has(result.requestId)) {
					const cmd = this.clearRequestFromQueue(result);
					if (cmd) {
						this.tryResolve(cmd, undefined);
					}
				}
				break;
			default:
				break;
		}
	}

	private killProcess() {
		if (this.proc) {
			try {
				this.proc.kill();
			} catch (ex) {
				console.log(ex);
			}
		}

		this.proc = undefined;
	}

	public getIndexPath(): string {
		let userPrefPath =
			process.env.APPDATA ||
			(process.platform == "darwin"
				? process.env.HOME + "/Library/Preferences"
				: "/var/local");

		userPrefPath = path.join(userPrefPath, "vdf");
		if (!fs.existsSync(userPrefPath)) fs.mkdirSync(userPrefPath);

		return userPrefPath;
	}

	public getNextCommandId(): number {
		const result = this.cmdId;
		this.cmdId += 1;
		return result;
	}

	public async sendCommand<T extends ICommandResult>(
		cmd: ICommand<T>
	): Promise<any> {
		if (!this.proc) {
			return Promise.reject(new Error("VDF Language Server not initialized"));
		}

		const executionCmd = <IExecutionCommand<T>>cmd;
		const payload = this.createPayload(executionCmd);

		this.commands.set(executionCmd.id, executionCmd);
		this.commandQueue.push(executionCmd.id);
		executionCmd.deferred = createDeferred<T>();

		try {
			this.proc.stdin.write(`${JSON.stringify(payload)}\n`);
			this.commands.set(executionCmd.id, executionCmd);
			this.commandQueue.push(executionCmd.id);
		} catch (ex) {
			console.error(ex);
			//If 'This socket is closed.' that means process didn't start at all (at least not properly).
			if (ex.message === "This socket is closed.") {
				this.restartLanguageServer();
			}

			return Promise.reject(ex);
		}

		return executionCmd.deferred.promise;
	}

	private createPayload<T extends ICommandResult>(
		cmd: IExecutionCommand<T>
	): any {
		const payload = {
			id: cmd.id,
			prefix: "",
			lookup: cmd.command,
			possibleWord: cmd.possibleWord,
			path: cmd.fileName,
			source: cmd.source,
			workspacePath: this.workspacePath,
			line: cmd.lineIndex,
			column: cmd.columnIndex
		};

		return payload;
	}

	public dispose() {
		this.clearPendingRequests();
		this.killProcess();
	}

	private tryResolve(
		command: IExecutionCommand<ICommandResult> | undefined | null,
		result: ICommandResult | PromiseLike<ICommandResult> | undefined
	): void {
		if (command && command.deferred) {
			command.deferred.resolve(result);
		}
	}
}
