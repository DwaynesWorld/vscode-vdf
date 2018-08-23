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

    const fileName = document.fileName;
    const range = document.getWordRangeAtPosition(position);
    const possibleWord = document.getText(range);
    const command = CommandType.Definitions;
    const lineIndex = position.line;
    const columnIndex = range.isEmpty
      ? position.character
      : range.end.character;

    const cmd: ICommand<IDefinitionResult> = {
      command,
      possibleWord,
      fileName,
      columnIndex,
      lineIndex
    };

    if (document.isDirty) {
      cmd.source = document.getText();
    }

    return this.vdfProxyFactory
      .getVdfProxyHandler<IDefinitionResult>(document.uri)
      .sendCommand(cmd)
      .then(result => {
        console.log(result);
        const locations = result.definitions.map(def => {
          try {
            const uri = vscode.Uri.file(def.fileName);
            const { startLine, startColumn, endLine, endColumn } = def.range;
            return new vscode.Location(
              uri,
              new vscode.Range(startLine, startColumn, endLine, endColumn)
            );
          } catch {
            return;
          }
        });

        return Promise.resolve(locations);
      });
  }
}
