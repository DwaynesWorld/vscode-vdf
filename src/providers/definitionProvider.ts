import * as vscode from "vscode";
import { VdfProxyFactory } from "../languageServer/vdfProxyFactory";
import {
  IDefinitionResult,
  ICommand,
  CommandType
} from "../languageServer/proxy";

export class VdfDefinitionProvider implements vscode.DefinitionProvider {
  constructor(private vdfProxyFactory: VdfProxyFactory) {}

  provideDefinition(
    document: vscode.TextDocument,
    position: vscode.Position,
    token: vscode.CancellationToken
  ): Thenable<vscode.Definition> {
    if (document.lineAt(position.line).text.match(/^\s*\/\//)) {
      return Promise.resolve(null);
    }

    if (position.character <= 0) {
      return Promise.resolve(null);
    }

    const filename = document.fileName;
    const range = document.getWordRangeAtPosition(position);
    const columnIndex = range.isEmpty
      ? position.character
      : range.end.character;

    const cmd: ICommand<IDefinitionResult> = {
      command: CommandType.Definitions,
      fileName: filename,
      columnIndex: columnIndex,
      lineIndex: position.line
    };

    if (document.isDirty) {
      cmd.source = document.getText();
    }

    return this.vdfProxyFactory
      .getVdfProxyHandler<IDefinitionResult>(document.uri)
      .sendCommand(cmd)
      .then(result => {
        console.log(result);
        const range = result.definitions[0].range;

        return Promise.resolve(
          new vscode.Location(
            document.uri,
            new vscode.Range(
              range.startLine,
              range.startColumn,
              range.endLine,
              range.endColumn
            )
          )
        );
      });
  }
}
