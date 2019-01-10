import {
	CancellationToken,
	SymbolInformation,
	SymbolKind,
	DiagnosticSeverity
} from "vscode";

export interface IRange {
	startLine: number;
	startColumn: number;
	endLine: number;
	endColumn: number;
}

export interface IDefinition {
	rawType: string;
	type: number;
	text: string;
	kind: SymbolKind;
	container: string;
	filePath: string;
	range: IRange;
}

export interface ICommandError {
	message: string;
}

export interface ICommandResult {
	requestId: number;
	isInternal: boolean;
	messageType: IPCMessage;
	message: string;
	metadata: string;
}

export interface IHoverResult extends ICommandResult {
	main: string;
	contents: string;
	hoverMetadata: string;
}
export interface IDefinitionResult extends ICommandResult {
	definitions: IDefinition[];
}

export interface IDiagnostic {
	message: string;
	serverity: DiagnosticSeverity;
	range: IRange;
}

export interface IDiagnosticResult extends ICommandResult {
	filePath: string;
	diagnostics: IDiagnostic[];
}

export interface ICommand<T extends ICommandResult> {
	telemetryEvent?: string;
	command: CommandType;
	possibleWord?: String;
	source?: string;
	fileName: string;
	lineIndex?: number;
	columnIndex?: number;
}

export interface Deferred<T> {
	readonly promise: Promise<T>;
	readonly resolved: boolean;
	readonly rejected: boolean;
	readonly completed: boolean;
	resolve(value?: T | PromiseLike<T>);
	// tslint:disable-next-line:no-any
	reject(reason?: any);
}

class DeferredImpl<T> implements Deferred<T> {
	private _resolve!: (value?: T | PromiseLike<T>) => void;
	// tslint:disable-next-line:no-any
	private _reject!: (reason?: any) => void;
	private _resolved: boolean = false;
	private _rejected: boolean = false;
	private _promise: Promise<T>;

	// tslint:disable-next-line:no-any
	constructor(private scope: any = null) {
		// tslint:disable-next-line:promise-must-complete
		this._promise = new Promise<T>((res, rej) => {
			this._resolve = res;
			this._reject = rej;
		});
	}

	public resolve(value?: T | PromiseLike<T>) {
		this._resolve.apply(this.scope ? this.scope : this, arguments);
		this._resolved = true;
	}

	// tslint:disable-next-line:no-any
	public reject(reason?: any) {
		this._reject.apply(this.scope ? this.scope : this, arguments);
		this._rejected = true;
	}

	get promise(): Promise<T> {
		return this._promise;
	}

	get resolved(): boolean {
		return this._resolved;
	}

	get rejected(): boolean {
		return this._rejected;
	}

	get completed(): boolean {
		return this._rejected || this._resolved;
	}
}

// tslint:disable-next-line:no-any
export function createDeferred<T>(scope: any = null): Deferred<T> {
	return new DeferredImpl<T>(scope);
}

export interface IExecutionCommand<T extends ICommandResult>
	extends ICommand<T> {
	id: number;
	token: CancellationToken;
	deferred?: Deferred<T>;
	delay?: number;
}

export enum CommandType {
	Arguments,
	Completions,
	Hover,
	Usages,
	Definitions,
	Symbols,
	Diagnostics
}

export enum IPCMessage {
	None,
	SymbolNotFound,
	NoProviderFound,
	LanguageServerIndexing,
	LanguageServerIndexingComplete
}
