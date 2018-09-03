import { CancellationToken, SymbolInformation, SymbolKind } from 'vscode';

export interface IDefinitionRange {
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
  range: IDefinitionRange;
}

export interface IHoverItem {
  text: string;
  description: string;
  docstring: string;
  signature: string;
}

export interface ICommandError {
  message: string;
}

export interface ICommandResult {
  requestId: number;
}

export interface IHoverResult extends ICommandResult {
  items: IHoverItem[];
}
export interface IDefinitionResult extends ICommandResult {
  definitions: IDefinition[];
}

export interface IInternalResult extends ICommandResult {
  MessageType: IPCMessage;
  Message: string;
  MetaData: string;
}

export interface ICommand<T extends ICommandResult> {
  telemetryEvent?: string;
  command: CommandType;
  possibleWord: String;
  source?: string;
  fileName: string;
  lineIndex: number;
  columnIndex: number;
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
  Symbols
}

export enum IPCMessage {
  SymbolNotFound,
  NoProviderFound,
  LanguageServerIndexing,
  LanguageServerIndexingComplete
}
