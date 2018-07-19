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
  fileName: string;
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

export interface ICommand<T extends ICommandResult> {
  telemetryEvent?: string;
  command: CommandType;
  source?: string;
  fileName: string;
  lineIndex: number;
  columnIndex: number;
}

export interface IExecutionCommand<T extends ICommandResult>
  extends ICommand<T> {
  id: number;
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
